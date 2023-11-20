using dbModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using ProductScan.Models;
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
            var result = _fasContext.FAS_Lines.ToList();
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
            var result = _fasContext.Ct_StepScan.ToList();

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
            try
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

                //// If given step is start step in sequence
                //if (currentStepID == 0)
                //{
                //    // Trying to get next step (if it exists)
                //    try
                //    {
                //        nextStep = StepSequenceSteps[currentStepID];
                //    }
                //    catch { }

                //    // Returning previous step as null (because not exists)
                //    // Returning current and next step
                //    return (true, "Шаг является первым в StepSequence", "FIRST_STEP", new List<int?> { null, StepID, nextStep });
                //}

                // Checking is StepID in OperLog
                var operLogs = _fasContext.Ct_OperLog.Where(i => (i.PCBID == IDLaser)).OrderByDescending(i => i.ID).ToList();

                // Checking amount of given data
                if (operLogs.Count() < 1)
                    return (false, "Шаг не найден в OperLog", "NO_OPLOG", null);

                // Getting PCBID from operlog for given StepID
                var PCBID = operLogs[0].PCBID;

                // Getting all steps by PCBID order by ID Desc
                operLogs = _fasContext.Ct_OperLog.Where(i => (i.PCBID == PCBID)).OrderByDescending(i => i.ID).ToList();

                // Checking step in oper log
                var currentOperLog = operLogs[0];
                int currentOperLogStepID = Convert.ToInt32(currentOperLog.StepID);

                // TODO: Выбор варианта (PASS / FAIL) 
                if (currentOperLogStepID == StepID)
                    return (true, "Данный шаг пройден.<br>Изменить результат сканирования?", "STEP_DUP", null);

                // TODO: Выбор варианта (PASS / FAIL)
                if (currentOperLogStepID == previousStep)
                    return (true, "Подтвердите результат проверки:", "STEP_CONFIRM", null);

                // TODO: Вывести список шагов для этой платы (OperLog)
                return (false, "Шаги не неопределенны", "STEP_UNDEFINED", null);
            }

            catch
            {
                return (false, "Catched an error while parsing", "ERROR", null);
            }
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
                } catch { }


                bool statusPCBSerial = CheckSerialTHTStart(serialNumber);

                // If PCBSerial was not validated
                if (!statusPCBSerial)
                    return Json(new { status = "ok", success = "false", description = "Не найден серийный номер платы" });

                (bool status, string description, string statusCode, List<int?>? steps) = CheckStepIDInStepSequense(contractLotID, stepID, (int)idLaser);
                
                // TODO: Add new validations
                // If StepSequence was not validated
                if (!status)
                    return Json(new { status = "ok", success = "false", description = description, statusCode = statusCode });
                else
                    return Json(new { status = "ok", success = "true", description = description, statusCode = statusCode, pcbid = idLaser });


                //return Json(new { status = "ok", success = "true", description = "Валидация успешна" } );
            }

            catch (Exception e)
            {
                return Json(new { status = "error", description = e.ToString() });
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
                var result = _fasContext.Contract_LOT.Where(i => (i.СustomersID == CustomerID) && (i.IsActive == true)).ToList();

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
                var contractLots = _fasContext.Contract_LOT.Where(i => (i.СustomersID == CustomerID) && (i.ID == ContractLotID)).ToList();

                // Getting info from contract lots
                var ModelID = contractLots[0].ModelID;
                var FASNumberFormat2 = contractLots[0].FASNumberFormat2;
                var Specification = contractLots[0].Specification;

                var fasModels = _fasContext.FAS_Models.Where(i => (i.ModelId == ModelID)).ToList();

                // Creating data lists
                var ModelName = fasModels[0].ModelName;

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
                return Json(new { status = "error", description = e.ToString()});
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