function applyScan(TestResultID, ErrorCodeID) {
    // Initializing variables
    const PCBID = parseInt($(".apply-scan-info #pcbid-info span").text());
    const LOTID = parseInt($("#contract-lots").children("option:selected").attr("id"));
    const StepID = parseInt($("#step-info span").attr("id"));
    let StepByID = parseInt($(".current-user").attr("id"));
    const LineID = parseInt($("#fas-lines").children("option:selected").attr("id"));
    let Descriptions = null;

    // Trying to add admin additional info
    const adminUsername = $("#admin-username").text();
    if (adminUsername != "") {
        StepByID = parseInt($("#admin-userid").text());
        Descriptions = "Ошибку игнорировал: " + adminUsername;
    }

    // Constructin data for the request
    const insertData = {
        PCBID: PCBID,
        LOTID: LOTID,
        StepID: StepID,
        TestResultID: TestResultID,
        StepByID: StepByID,
        LineID: LineID,
        Descriptions: Descriptions,
        ErrorCodeID: ErrorCodeID,
    }

    // Sending request
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/InsertOperLog`,
        data: insertData,
        dataType: "json",
        success: function (response) {
            console.log(response);
        }
    });

    // Resetting hidden of used buttons
    $(".apply-fail-scan-container").addClass("hidden");
    $(".apply-scan-buttons").removeClass("hidden");

    // Updating test results
    const contractLotID = $("#contract-lots").children("option:selected").attr("id");
    const stepID = $("#step-info span").attr("id");
    const currentLineID = $("#fas-lines").children("option:selected").attr("id");
    getTestResultsCounter(contractLotID, stepID, currentLineID)
}

function updateErrorCodes() {
    // Constructing error codes
    const modelID = $("#model-info span").attr("id");

    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetErrorCodes`,
        data: { ModelID: modelID },
        dataType: "json",
        success: function (response) {
            // Checking request status
            if (response.status != 'ok') { return; }

            // Checking user existance
            if (response.success == "true") {
                // Getting container
                const container = $("#fail-scan-error-codes");
                
                // Clearing container
                container.html("");

                // Filling error codes
                response.responseData.forEach(code => {
                    // Constructing HTML
                    const html = `<option id="${code.error_code_id}">[${code.error_code}] ${code.description}</option>`

                    // Adding options to the selection
                    container.append(html);
                });
            }

            // Showing error message
            else {
                
            }
        }
    });
}

$(document).ready(function () {
    // On back scan pressed
    $("#back-scan").click(function (e) { 
        e.preventDefault();
        
        // Redirecting to the last page
        if (!$(".apply-fail-scan-container").hasClass("hidden")) {
            $(".apply-fail-scan-container").addClass("hidden");
            $(".apply-scan-buttons").removeClass("hidden");
        } else {
            toggleContainer("work");
        }

        // Clearing serial number
        const serialNumber = $(".work-container #serial-number");
        serialNumber.val("");
        serialNumber.focus();

        // Clearing error message
        $("#serial-number-label").text("");

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());
    });

    // On pass scan pressed
    $("#pass-scan").click(function (e) { 
        e.preventDefault();

        // Inserting scan data
        applyScan(2, null);

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());

        // Clearing serial number
        const serialNumber = $(".work-container #serial-number");
        serialNumber.val("");
        serialNumber.focus();

        // Clearing error message
        $("#serial-number-label").text("");

        // Redirecting to main
        toggleContainer("work");
    });

    // On fail scan pressed
    $("#fail-scan").click(function (e) { 
        e.preventDefault();

        // Hiding buttons, showing apply fail menu
        $(".apply-fail-scan-container").removeClass("hidden");
        $(".apply-scan-buttons").addClass("hidden");

        // Updating error code
        updateErrorCodes();

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());
    });

    // On fail scan apply
    $("#apply-fail-scan").click(function (e) { 
        e.preventDefault();

        // Getting choosen error code id
        const errorCodeId = parseInt($("#fail-scan-error-codes").children("option:selected").attr("id"));

        // Inserting scan data
        applyScan(3, errorCodeId);

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());

        // Clearing serial number
        const serialNumber = $(".work-container #serial-number");
        serialNumber.val("");
        serialNumber.focus();
        
        // Clearing error message
        $("#serial-number-label").text("");

        // Redirecting to main
        toggleContainer("work");
    });
});