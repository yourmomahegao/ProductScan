function loadLastStepInfoFromPCBID(PCBID) {
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetLastStepInfoFromPCBID`,
        data: { PCBID: PCBID },
        dataType: "json",
        success: function (response) {
            // Checking request status
            if (response.status != 'ok') { return; }

            // Checking user existance
            if (response.success == "true") {
                // Saving variables
                const Content = response.responseData.content;
                const StepName = response.responseData.step_name;
                const Result = response.responseData.result;
                const UserName = response.responseData.username;
                const StepDate = response.responseData.step_date;
                let ErrorCode = response.responseData.error_code;

                // Fixing error code
                if (ErrorCode == "") {
                    ErrorCode = "Не задано";
                }

                // Uploading variables
                $("#content-info span").text(Content);
                $("#step-name-info span").text(StepName);
                $("#result-info span").text(Result);
                $("#username-info span").text(UserName);
                $("#step-date-info span").text(StepDate);
                $("#error-code-info span").text(ErrorCode);
            }

            // Showing error message
            else {
                // Uploading variables
                $("#content-info span").text("Не определено");
                $("#step-name-info span").text("Не определено");
                $("#result-info span").text("Не определено");
                $("#username-info span").text("Не определено");
                $("#step-date-info span").text("Не определено");
                $("#error-code-info span").text("Не определено");

                $("#badge-error-label").text(response.description)
            }
        }
    });
}

$(document).ready(function () {
    // Hotkey connect
    $(document).keyup(function(e) {
        // Toggling scan info on "I" press
        if(e.keyCode == 73 && !$(".apply-scan-container").hasClass("hidden")) {
            $(".apply-scan-info").toggleClass("hidden");
        }

        // Toggling last logs on "L" press
        if(e.keyCode == 76 && !$(".apply-scan-container").hasClass("hidden")) {
            $(".last-logs-container").toggleClass("hidden");
        }
    });
});