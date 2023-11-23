$(document).ready(function () {
    setInterval(function() {
        // Updating test results
        const contractLotID = $("#contract-lots").children("option:selected").attr("id");
        const stepID = $("#step-info span").attr("id");
        const currentLineID = $("#fas-lines").children("option:selected").attr("id");
        getTestResultsCounter(contractLotID, stepID, currentLineID)
    }, 60000)
});