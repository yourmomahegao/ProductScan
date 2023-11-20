$(document).ready(function () {
    // On back scan pressed
    $("#back-scan").click(function (e) { 
        e.preventDefault();
        
        // Redirecting to the last page
        toggleContainer("work");

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());
    });

    // On pass scan pressed
    $("#pass-scan").click(function (e) { 
        e.preventDefault();

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());
    });

    // On fail scan pressed
    $("#fail-scan").click(function (e) { 
        e.preventDefault();

        // Starting logout timer
        updateLogoutTimer(getLogoutTime());
    });
});