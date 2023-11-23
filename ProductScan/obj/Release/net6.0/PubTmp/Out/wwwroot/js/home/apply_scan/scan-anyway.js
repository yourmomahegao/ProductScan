// Showing scan anyway notification
function showScanAnywayNotification() {
    $(".scan-anyway-container").removeClass("hidden");
}

// Hiding scan anyway notification
function hideScanAnywayNotification() {
    $(".scan-anyway-container").addClass("hidden");
}

// Function to get admin user
function checkUserAdminStatus(RFID) {
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetAdminUser`,
        data: { RFID: RFID },
        dataType: "json",
        success: function (response) {
            // Checking request status
            if (response.status != 'ok') { return; }

            // Checking user existance
            if (response.success == "true") {
                // Saving variables
                const userID = response.userid;
                const username = response.username;

                // Hiding badge splashscreen
                hideSplashScreen("admin_badge");

                // Toggling container anyway
                toggleContainer("apply_scan");

                // Uploading content
                const PCBID = parseInt($(".apply-scan-info #pcbid-info span").text());
                loadLastStepInfoFromPCBID(PCBID);

                // Hiding error message
                $("#admin-badge-error-label").text("")

                // Making admin badge value empty
                $(".admin-badge-splashscreen #admin-badge-number").val("")

                // Saving admin password
                $("#admin-username").text(username)
                $("#admin-userid").text(userID)
            }

            // Showing error message
            else {
                $("#admin-badge-error-label").text(response.description)
            }
        }
    });
}



$(document).ready(function () {
    // Binding hide button
    $(".scan-anyway-container #scan-anyway-deny-button").click(function(e) {
        e.preventDefault();

        // Hiding scan-anyway notification
        hideScanAnywayNotification();
    });

    // Binding scan-anyway button
    $(".scan-anyway-container #scan-anyway-button").click(function(e) {
        e.preventDefault();
        
        // Toggling admin-badge container
        showSplashScreen("admin_badge");

        // Hiding scan-anyway notification
        hideScanAnywayNotification();
    });

    // On admin badge scanned
    $(".admin-badge-splashscreen #admin-badge-number").on('keyup', function (e) {
        if (e == undefined) {
            return;
        }

        if (e.key === 'Enter' || e.keyCode === 13) {
            // Checking admin badge
            checkUserAdminStatus($("#admin-badge-number").val());
        }
    });
});