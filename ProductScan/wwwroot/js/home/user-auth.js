
var logoutTimer;

// Getting user by RFID and saving variables
function login(RFID) {
    $.ajax({
        type: "GET",
        url: "/Home/GetUserByRFID",
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

                // Applying them to the HTML page
                $(".current-user").attr("id", userID);
                $(".current-user").text(username);

                // Hiding badge splashscreen
                hideSplashScreen("badge");

                // Hiding error message
                $("#badge-error-label").text("")

                // Starting logout timer
                updateLogoutTimer(getLogoutTime());
            }

            // Showing error message
            else {
                $("#badge-error-label").text(response.description)
            }
        }
    });
}

// Will logout person
function logout() {
    toggleContainer("work");
    showSplashScreen("badge");
    $("#badge-number").val("");
    $("#badge-number").focus();
}

// Will update timer, for current session
function updateLogoutTimer(time) {
    // If time is undefined
    if (time == 0) {
        return;
    }

    // Checking existance
    if (logoutTimer) {
        clearTimeout(logoutTimer)
    }

    // Starting logout timer
    logoutTimer = setTimeout(function () { logout() }, time)
}

$(document).ready(function () {
    // On badge scanned
    $("#badge-number").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            // Checking RFID
            login($(this).val());
        }
    });
});