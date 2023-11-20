function loadLastStepInfoFromPCBID(PCBID) {
    $.ajax({
        type: "GET",
        url: "/Home/GetLastStepInfoFromPCBID",
        data: { PCBID: PCBID },
        dataType: "json",
        success: function (response) {
            // Checking request status
            if (response.status != 'ok') { return; }

            // Checking user existance
            if (response.success == "true") {
                console.log(response);
                
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
                $("#badge-error-label").text(response.description)
            }
        }
    });
}

$(document).ready(function () {
    // Hotkey connect
    $(document).keypress(function(e) {
        // Toggling scan info on "I" press
        if(e.which == 105 && !$(".apply-scan-container").hasClass("hidden")) {
            $(".apply-scan-info").toggleClass("hidden");
        }
    });
});