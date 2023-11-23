function getTestResultsCounter(LOTID, StepID, LineID) {
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetTestResultsCounter`,
        data: { LOTID: LOTID, StepID: StepID, LineID: LineID },
        dataType: "json",
        success: function (response) {
            // Checking request status
            if (response.status != 'ok') { return; }

            // Checking user existance
            if (response.success == "true") {
                // Saving variables
                const PassAmount = response.responseData.pass_amount;
                const FailAmount = response.responseData.fail_amount;
                const DuplicatesAmount = response.responseData.duplicates_amount;

                // Uploading variables
                $("#pass-counter-info #value").text(PassAmount);
                $("#fail-counter-info #value").text(FailAmount);
                $("#duplicates-counter-info #value").text(DuplicatesAmount);
            }

            // Showing error message
            else {
                
            }
        }
    });
}

$(document).ready(function () {
    // Hotkey connect
    $(document).keyup(function(e) {
        // Toggling scan info on "I" press
        if(e.keyCode == 73 && !$(".work-container").hasClass("hidden")) {
            $(".test-results-counter-container").toggleClass("hidden");
        }
    });
});