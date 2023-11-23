using dbModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using ProductScan.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;

namespace ProductScan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FASContext _fasContext;
        private readonly SMDCOMPONETSContext _smdContext;

        public HomeController(ILogger<HomeController> logger, FASContext fasContext, SMDCOMPONETSContext smdContext)
        {
            _logger = logger;
            _fasContext = fasContext;
            _smdContext = smdContext;
        }

        private void AddCustomersToViewBag()
        {
            // Query data using LINQ
            var result = _fasContext.CT_Сustomers.ToList();

            // 'result' now contains the data retrieved from the database
            ViewBag.Customers = result;
        }

        private void AddFasLinesToViewBag()
        {
            // Query data using LINQ
            var result = _fasContext.FAS_Lines.OrderByDescending(i => i.LineID).ToList();
            var new_result = new List<Object>();

            // Lines validation
            foreach (var line in result)
            {
                if (line.LineName.Split(" ").Length <= 1)
                    continue;

                if (line.LineName.Contains("SMT") || line.LineName.Contains("Repair"))
                    continue;

                new_result.Add(line);
            }

            // 'result' now contains the data retrieved from the database
            ViewBag.FasLines = new_result;
        }

        private void AddStepsScanToViewBag()
        {
            // Query data using LINQ
            var result = _fasContext.Ct_StepScan.OrderByDescending(i => i.ID).ToList();

            // 'result' now contains the data retrieved from the database
            ViewBag.StepsScan = result;
        }

        /// <summary>
        /// Getting last 3 decimals from hexdecimal mask
        /// </summary>
        static List<int> GetLastSixCharsAsDecimalList(string fasNumberFormat2)
        {
            // Getting las six chars from string
            string lastSixChars = fasNumberFormat2.Substring(Math.Max(0, fasNumberFormat2.Length - 6)); // Get the last 6 characters

            // Constructing decimal list
            List<int> decimalList = new List<int>();

            // Adding each decimal to the list
            for (int i = 0; i < lastSixChars.Length; i += 2)
            {
                string hexSubstring = lastSixChars.Substring(i, 2);
                int decimalValue = Convert.ToInt32(hexSubstring, 16);
                decimalList.Add(decimalValue);
            }

            return decimalList;
        }

        /// <summary>
        /// Checking serial number by given mask
        /// </summary>
        private bool CheckSerialNumberByMask(string serialNumber, string fasNumberFormat2)
        {
            try
            {
                // Getting last numbers of FAS number format
                List<int> fasNumberFormatLengths = GetLastSixCharsAsDecimalList(fasNumberFormat2);

                // Getting serial number parts
                string startSerialNubmerPart = serialNumber.Substring(0, fasNumberFormatLengths[0]);
                string centerSerialNubmerPart = serialNumber.Substring(fasNumberFormatLengths[0], fasNumberFormatLengths[1]);
                string endSerialNubmerPart = serialNumber.Substring(fasNumberFormatLengths[1], fasNumberFormatLengths[2]);

                // Getting mask nubmer parts
                string startMaskNubmerPart = fasNumberFormat2.Substring(0, fasNumberFormatLengths[0]);
                string endMaskNubmerPart = fasNumberFormat2.Substring(fasNumberFormatLengths[1], fasNumberFormatLengths[2]);

                // If start of mask and start of serial number is not equal
                if (startMaskNubmerPart != startSerialNubmerPart)
                    return false;

                // If end of mask and end of serial number is not equal
                if (endMaskNubmerPart != endSerialNubmerPart)
                    return false;

                return true;
            }

            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checking is serial number is existing in dbo.LaserBase "Content" field
        /// </summary>
        private dynamic CheckSerialNumberByLaserBaseContent(string serialNumber)
        {
            try
            {
                // Trying to get Model
                var laserBases = _smdContext.LazerBases.Where(i => (i.Content == serialNumber)).FirstOrDefault();

                // Getting info from contract lots
                return laserBases!.IDLaser;
            }

            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checking is serial number is existing in dbo.THTStart "PCBserial" field (and PCBResult is true)
        /// </summary>
        private bool CheckSerialTHTStart(string serialNumber)
        {
            try
            {
                // Trying to get Model
                var thtStarts = _smdContext.THTStart.Where(i => (i.PCBserial == serialNumber) && (i.PCBResult == true)).ToList();

                // Checking amount of given data
                if (thtStarts.Count() < 1)
                    return false;

                return true;
            }

            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checking is StepID is first value in dbo.Contract_LOT "StepSequence" field
        /// </summary>
        private (bool, string, string, List<int?>?) CheckStepIDInStepSequense(int ContractLotID, int StepID, int IDLaser)
        {
                // Trying to get Model
                var contractLots = _fasContext.Contract_LOT.Where(i => i.ID == ContractLotID).ToList();

                var StepSequence = contractLots[0].StepSequence;
                List<int> StepSequenceSteps = new List<int>();

                // Filling in StepSequenceSteps
                // Adding each decimal to the list
                for (int i = 0; i < StepSequence?.Length; i += 2)
                {
                    string hexSubstring = StepSequence.Substring(i, 2);
                    int decimalValue = Convert.ToInt32(hexSubstring, 16);
                    StepSequenceSteps.Add(decimalValue);
                }

                // Constructing previous and next step
                int? previousStep = null;
                int? nextStep = null;

                // Current StepID ID in StepSequence
                int currentStepID = -1;

                // Getting current StepID ID in StepSequence
                for (int i = 0; i < StepSequenceSteps.Count(); i++)
                {
                    if (StepSequenceSteps[i] == StepID)
                    {
                        currentStepID = i;
                        break;
                    }
                }

                // If given StepID was not found
                if (currentStepID == -1)
                    return (false, "Шаг не был найден", "NO_STEP", null);

                // Saving is current step is first
                bool isCurrentStepIsFirst = false;

                // If given step is start step in sequence
                if (currentStepID == 0)
                {
                    // Trying to get next step (if it exists)
                    try
                    {
                        nextStep = StepSequenceSteps[currentStepID];
                    }
                    catch { }

                    // Returning previous step as null (because not exists)
                    // Returning current and next step
                    isCurrentStepIsFirst = true;
                }

                // Getting all steps by PCBID order by ID Desc
                var currentOperLog = _fasContext.Ct_OperLog.Where(i => (i.PCBID == IDLaser)).OrderByDescending(i => i.ID).FirstOrDefault();

                // Checking step in oper log
                short? currentOperLogStepID = currentOperLog?.StepID;

                if ((currentOperLogStepID == StepID) && (currentOperLogStepID != null))
                    return (true, "Данный шаг пройден", "STEP_DUP", null);

                if ((currentOperLogStepID == previousStep) && (currentOperLogStepID != null))
                    return (true, "Подтвердите результат проверки", "STEP_CONFIRM", null);

                // Finally checking is step is first step in StepSequence
                if (isCurrentStepIsFirst && (currentOperLogStepID == null))
                    return (true, "Шаг является первым в сиквенции", "FIRST_STEP", new List<int?> { null, StepID, nextStep });

                // TODO: Вывести список шагов для этой платы (OperLog)
                return (false, "Нарушена последовательность шагов", "STEP_UNDEFINED", null);

        }

        public IActionResult GetScanResult(string serialNumber, string fasNumberFormatString, int contractLotID, int stepID)
        {
            try
            {
                // Getting numbers formats
                string[] fasNumberFormats = fasNumberFormatString.Split(";");
                bool statusMask = false;

                foreach (var fasNumberFormat in fasNumberFormats)
                {
                    // Checking serial number
                    statusMask = CheckSerialNumberByMask(serialNumber, fasNumberFormat);

                    // Ending loop if at least one number is valid
                    if (statusMask)
                        break;
                }

                // If mask was not validated
                if (!statusMask)
                    return Json(new { status = "ok", success = "false", description = "Маска не совпадает с серийным номером" });

                var idLaser = CheckSerialNumberByLaserBaseContent(serialNumber);

                // If idLaser was not found
                try
                {
                    if (!(bool)idLaser)
                        return Json(new { status = "ok", success = "false", description = "Не найдены соответствия в LaserBase" });
                }
                catch { }


                //bool statusPCBSerial = CheckSerialTHTStart(serialNumber);

                //// If PCBSerial was not validated
                //if (!statusPCBSerial)
                //    return Json(new { status = "ok", success = "false", description = "Не найден серийный номер платы" });

                (bool status, string description, string statusCode, List<int?>? steps) = CheckStepIDInStepSequense(contractLotID, stepID, (int)idLaser);

                // TODO: Add new validations
                // If StepSequence was not validated
                if (!status)
                    return Json(new { status = "ok", success = "false", description = description, statusCode = statusCode, pcbid = idLaser });
                else
                    return Json(new { status = "ok", success = "true", description = description, statusCode = statusCode, pcbid = idLaser });


                //return Json(new { status = "ok", success = "true", description = "Валидация успешна" } );
            }

            catch (Exception e)
            {
                return Json(new { status = "error", description = e.ToString() });
            }
        }

        /// <summary>
        /// Getting test results amount for given LOTID and StepID
        /// </summary>
        public IActionResult GetTestResultsCounter(int LOTID, int StepID, int LineID)
        {
            try
            {
                // Constructing current date
                DateTime today = DateTime.Today;
                DateTime? startDatetime = null;
                DateTime? endDatetime = null;

                int hours = today.Hour;

                // Constructing day shift
                if (hours < 8 && hours < 20)
                {
                    startDatetime = new DateTime(today.Year, today.Month, today.Day, 08, 00, 00);
                    endDatetime = new DateTime(today.Year, today.Month, today.Day, 20, 00, 00);
                }

                // Constructing night shift
                else if (hours > 20 && hours < 8)
                {
                    startDatetime = new DateTime(today.Year, today.Month, today.Day, 20, 00, 00);
                    endDatetime = new DateTime(today.Year, today.Month, today.Day, 08, 00, 00);
                }

                else
                    return Json(new { status = "ok", success = "false" });

                // Trying to get Model
                var operLogs = _fasContext.Ct_OperLog.Where(i =>
                                (i.LOTID == LOTID) &&
                                (i.StepID == StepID) &&
                                (i.LineID == LineID) &&
                                (i.StepDate > startDatetime) &&
                                (i.StepDate < endDatetime))
                                .OrderBy(i => i.PCBID)
                                .ThenByDescending(i => i.StepDate).ToList();

                // Checking amount of given data
                if (operLogs.Count() < 1)
                    return Json(new { status = "ok", success = "false", description = "Не найдены логи" });

                // Constructing pass amount, fail amount, duplicates amount
                int passAmount = 0;
                int failAmount = 0;
                int duplicatesAmount = 0;

                // Getting pass amount, fail amount, duplicates amount
                for (int index = 0; index < operLogs.Count(); index++)
                {
                    // Getting logs
                    Ct_OperLog currentLog = operLogs[index];
                    Ct_OperLog prevLog = null;

                    // Getting previous log
                    if (index >= 1)
                        prevLog = operLogs[index - 1];

                    // Checking if previous log does not exists
                    if (prevLog == null)
                    {
                        // If current log result is PASS
                        if (currentLog.TestResultID == 2)
                        {
                            passAmount++;
                            continue;
                        }

                        // If current log result is FAIL
                        else if (currentLog.TestResultID == 3)
                        {
                            failAmount++;
                            continue;
                        }
                    }

                    // Checking if previous log does exists
                    else
                    {
                        // If current log PCBID is equal to previous log PCBID 
                        if (currentLog.PCBID == prevLog!.PCBID)
                        {
                            // Adding duplicates amount
                            duplicatesAmount++;
                            continue;
                        }

                        // If current log result is PASS
                        else if (currentLog.TestResultID == 2)
                        {
                            passAmount++;
                            continue;
                        }

                        // If current log result is FAIL
                        else if (currentLog.TestResultID == 3)
                        {
                            failAmount++;
                            continue;
                        }
                    }
                }

                // Constructing data
                var responseData = new
                {
                    pass_amount = passAmount,
                    fail_amount = failAmount,
                    duplicates_amount = duplicatesAmount
                };

                return Json(new { status = "ok", success = "true", responseData = responseData });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", success = "false", description = e.ToString() });
            }
        }

        public IActionResult Index()
        {
            ViewBag.Customers = new List<string>();
            // Adding to the ViewBag
            AddCustomersToViewBag();
            AddFasLinesToViewBag();
            AddStepsScanToViewBag();

            return View();
        }

        /// <summary>
        /// Checking is user RFID is existing in dbo.FAS_Users
        /// </summary>
        public IActionResult GetUserByRFID(string RFID)
        {
            try
            {
                // Trying to get Model
                var users = _fasContext.FAS_Users.Where(i => (i.RFID == RFID)).ToList();

                // Checking amount of given data
                if (users.Count() < 1)
                    return Json(new { status = "ok", success = "false", description = "Пользователь не найден" });

                return Json(new { status = "ok", success = "true", userid = users[0].UserID, username = users[0].UserName });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", success = "false", description = e.ToString() });
            }
        }

        /// <summary>
        /// Checking is user RFID is existing in dbo.FAS_Users and user is in admin
        /// </summary>
        public IActionResult GetAdminUser(string RFID)
        {
            try
            {
                // Trying to get Model
                var users = _fasContext.FAS_Users.Where(i => (i.RFID == RFID)).ToList();

                // Checking amount of given data
                if (users.Count() < 1)
                    return Json(new { status = "ok", success = "false", description = "Пользователь не найден" });

                // Checking is user is actually admin
                if (!(users[0].UsersGroupID == 1))
                    return Json(new { status = "ok", success = "false", description = "Пользователь не администратор" });

                return Json(new { status = "ok", success = "true", userid = users[0].UserID, username = users[0].UserName });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", success = "false", description = e.ToString() });
            }
        }

        public IActionResult GetLastStepInfoFromPCBID(int PCBID)
        {
            try
            {
                // Getting needed fields
                var operLogFields = _fasContext.Ct_OperLog.Where(i => (i.PCBID == PCBID)).OrderByDescending(i => i.ID).FirstOrDefault();
                var lazerBaseFields = _smdContext.LazerBases.Where(i => (i.IDLaser == operLogFields!.PCBID)).FirstOrDefault();
                var stepScanFields = _fasContext.Ct_StepScan.Where(i => (i.ID == operLogFields!.StepID)).FirstOrDefault();
                var testResultFields = _fasContext.Ct_TestResult.Where(i => (i.ID == operLogFields!.TestResultID)).FirstOrDefault();
                var fasUsersFields = _fasContext.FAS_Users.Where(i => (i.UserID == operLogFields!.StepByID)).FirstOrDefault();
                var errorCodeFields = _fasContext.FAS_ErrorCode.Where(i => (i.ErrorCodeID == operLogFields!.ErrorCodeID)).FirstOrDefault();

                // Parsing needed vars
                string Content = lazerBaseFields!.Content;
                string StepName = stepScanFields!.StepName;
                string Result = testResultFields!.Result;
                string UserName = fasUsersFields!.UserName;
                string StepDate = operLogFields!.StepDate.ToString()!;
                string ErrorCode = "";

                if (errorCodeFields != null)
                    ErrorCode = errorCodeFields!.ErrorCode;
                else
                    ErrorCode = "";

                // Constructing data
                var responseData = new
                {
                    content = Content,
                    step_name = StepName,
                    result = Result,
                    error_code = ErrorCode,
                    username = UserName,
                    step_date = StepDate,
                };

                return Json(new { status = "ok", success = "true", responseData });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", success = "false", description = e.ToString() });
            }
        }

        public IActionResult GetContractLotsFromCustomerID(int CustomerID)
        {
            try
            {
                // Query data using LINQ
                var result = _fasContext.Contract_LOT.Where(i =>
                (i.СustomersID == CustomerID) &&
                (i.IsActive == true))
                    .OrderByDescending(i => i.ID).ToList();

                // Creating data lists
                var IDList = new List<int>();
                var SpecificationList = new List<string>();
                foreach (var item in result)
                {
                    IDList.Add(item.ID);
                    SpecificationList.Add(item.Specification);
                }

                // Constricting data
                var responseData = new
                {
                    status = "ok",
                    id_list = IDList,
                    spec_list = SpecificationList
                };

                // Returning constructed data
                return Json(responseData);
            }

            catch (Exception e)
            {
                return Json(new { status = "error", description = e.ToString() });
            }
        }

        public IActionResult GetModel(int CustomerID, int ContractLotID)
        {
            try
            {
                // Trying to get Model
                var contractLot = _fasContext.Contract_LOT.Where(i => (i.СustomersID == CustomerID) && (i.ID == ContractLotID))
                    .FirstOrDefault();

                // Getting info from contract lots
                var ModelID = contractLot!.ModelID;
                var FASNumberFormat2 = contractLot!.FASNumberFormat2;
                var Specification = contractLot!.Specification;

                var fasModel = _fasContext.FAS_Models.Where(i => (i.ModelId == ModelID)).FirstOrDefault();

                // Creating data lists
                var ModelName = fasModel.ModelName;

                // Constricting data
                var responseData = new
                {
                    status = "ok",
                    fas_number_format_2 = FASNumberFormat2,
                    specification = Specification,
                    model_id = ModelID,
                    model_name = ModelName,
                };

                // Returning constructed data
                return Json(responseData);
            }

            catch (Exception e)
            {
                return Json(new { status = "error", description = e.ToString() });
            }
        }

        public IActionResult GetLastOperationsFromOperLog(int Amount, int PCBID)
        {
            try
            {
                //int a = 3;
                //int b = 4;
                //int c = 5;

                //int d = a > b ? b : c;

                // Trying to get Model
                var contractLots = _fasContext.Ct_OperLog.Where(o => o.PCBID == PCBID).Select(o => new OperLogModel
                    {
                        StepByID = o.StepByID,
                        ID = o.ID,
                        LOTID = o.LOTID,
                        StepID = o.StepID,
                        PCBID = o.PCBID,
                        StepName = _fasContext.Ct_StepScan.Where(l => l.ID == o.StepID).Select(l => l.StepName).FirstOrDefault()!,
                        Result = _fasContext.Ct_TestResult.Where(t => t.ID == o.TestResultID).Select(t => t.Result).FirstOrDefault()!,
                        UserName = _fasContext.FAS_Users.Where(u => u.UserID == o.StepByID).Select(u => u.UserName).FirstOrDefault()!,
                        LineName = _fasContext.FAS_Lines.Where(l => l.LineID == l.LineID).Select(l => l.LineName).FirstOrDefault()!,
                        ErrorCodeID = o.ErrorCodeID,
                        StepDate = o.StepDate,
                    }
                )
                .Where(i => (i.PCBID == PCBID))
                .Take(Amount)
                .OrderByDescending(i => (i.ID))
                .ToList();


                // Checking amount of given data
                if (contractLots.Count() < 1)
                    return Json(new { status = "error", success = "false" });

                return Json(new { status = "ok", success = "true", responseData = contractLots });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", success = "false", description = e.ToString() });
            }
        }

        public IActionResult InsertOperLog(int PCBID, int LOTID, short StepID, byte TestResultID, short StepByID, byte LineID, string? Descriptions, int? ErrorCodeID)
        {
            try
            {
                // Creating step date
                DateTime StepDate = DateTime.Now;

                // Opening operlogs
                var operLogs = _fasContext.Set<Ct_OperLog>();

                // Adding query to the oper log
                operLogs.Add(new Ct_OperLog
                {
                    PCBID = PCBID,
                    LOTID = LOTID,
                    TestResultID = TestResultID,
                    StepID = StepID,
                    StepDate = StepDate,
                    StepByID = StepByID,
                    LineID = LineID,
                    Descriptions = Descriptions,
                    ErrorCodeID = ErrorCodeID
                });

                // Saving changes
                _fasContext.SaveChanges();

                return Json(new { status = "ok" });
            }

            catch (Exception e)
            {
                return Json(new { status = "error" });
            }
        }

        public IActionResult GetErrorCodes(short ModelID)
        {
            try
            {
                // Trying to get Model
                var errorCodes = _fasContext.FAS_ErrorCode
                    .Join(_fasContext.FAS_Models, i => i.ErrGroup, m => m.ErrorGroupId, (i, m) => new
                    {
                        ErrorCodeID = i.ErrorCodeID,
                        ErrorCode = i.ErrorCode,
                        Description = i.Description,

                        ModelID = m.ModelId,
                        ErrorGroupID = m.ErrorGroupId,
                    })
                    .Where(i => i.ModelID == ModelID)
                    .ToList();

                // Checking amount of given data
                if (errorCodes.Count() < 1)
                    return Json(new { status = "ok", success = "false", description = "Коды не найдены" });

                // Constructing response data
                List<dynamic> responseData = new List<dynamic>();

                foreach (var error in errorCodes)
                {
                    var responseDataLine = new
                    {
                        error_code_id = error.ErrorCodeID,
                        error_code = error.ErrorCode,
                        description = error.Description,
                    };

                    responseData.Add(responseDataLine);
                }

                return Json(new { status = "ok", success = "true", responseData = responseData });
            }

            catch (Exception e)
            {
                return Json(new { status = "error", success = "false", description = e.ToString() });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}