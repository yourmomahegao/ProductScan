// Showing scan anyway notification
function showScanDuplicateNotification() {
    $(".scan-duplicate-container").removeClass("hidden");
}

// Hiding scan anyway notification
function hideScanDuplicateNotification() {
    $(".scan-duplicate-container").addClass("hidden");
}

$(document).ready(function () {
    // Binding hide button
    $(".scan-duplicate-container #scan-duplicate-deny-button").click(function(e) {
        e.preventDefault();

        // Hiding scan-anyway notification
        hideScanDuplicateNotification();
    });

    // Binding scan-anyway button
    $(".scan-duplicate-container #scan-duplicate-button").click(function(e) {
        e.preventDefault();
        
        // Showing login dialog
        showLoginDialog();

        // Hiding scan-anyway notification
        hideScanDuplicateNotification();
    });
});