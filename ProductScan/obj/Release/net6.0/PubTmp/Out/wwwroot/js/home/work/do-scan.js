
function getScanResult(serialNumber, fasNumberFormatString, contractLotID, stepID) {
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetScanResult`,
        data: {serialNumber: serialNumber, fasNumberFormatString: fasNumberFormatString, contractLotID: contractLotID, stepID: stepID },
        dataType: "json",
        success: function (response) {
            // Making button available
            $("#do-scan").attr("disabled", false);

            // Checking request status
            if (response.status != 'ok') { return; }

            // Saving PCBID
            $(".apply-scan-info #pcbid-info span").text(response.pcbid);

            if (response.statusCode == "STEP_CONFIRM" || response.statusCode == "FIRST_STEP") {
                $("#apply-scan-label").html(response.description);
                toggleContainer("apply_scan");

                // Showing login dialog
                showLoginDialog();

                // Resetting serial number error label
                $(".work-container #serial-number-label").text("");
            }

            // On serial number check fail
            if (response.success == "false") {
                if (response.pcbid != undefined) {
                    // Showing error label
                    const scanAnywayError = $(".scan-anyway-container #scan-anyway-label #error");
                    scanAnywayError.text(response.description);

                    // Showing scan anyway
                    showScanAnywayNotification();
                    
                    // Updating last operlogs
                    getLastOperationsFromOperLog(response.pcbid);
                }

                // Showing error message
                $(".work-container #serial-number-label").text(response.description)              
            }

            // On succesful request
            else if (response.success == "true") {
                // Saving PCBID
                $(".apply-scan-info #pcbid-info span").text(response.pcbid);

                // Uploading content
                loadLastStepInfoFromPCBID(response.pcbid);

                // Updating last operlogs
                getLastOperationsFromOperLog(response.pcbid);
            }
        },

        error: function (response) {
            // Showing error message
            $(".work-container #serial-number-label").text("Ошибка сканирования")
        }
    });
}

$(document).ready(function () {
    $("#serial-number").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            if ($("#do-scan").attr("disabled")) { return; }

            $("#do-scan").trigger("click");
        }
    });

    $("#do-scan").click(function() {
        // Getting variables
        const serialNumber = $("#serial-number").val()
        const fasNumberFormatString = $("#fas-number-format2 span").attr("value");
        const contractLotID = $("#contract-lots").children("option:selected").attr("id");
        const stepID = $("#step-info span").attr("id");

        // Getting scan result
        getScanResult(serialNumber, fasNumberFormatString, contractLotID, stepID);

        // Making button unavailable
        $(this).attr("disabled", true);

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());
    })
})