var containers = null;
var splashscreens = null;

// Togging container by it's name
function toggleContainer(name) {
    // Cheking container
    if (containers == null) { return; }

    // Disabling every container
    Object.entries(containers).forEach(container => {
        container[1].addClass("hidden")
    });

    // Showing our container
    const container = containers[name]
    container.removeClass("hidden")
}

// Showing given splashscreen
function showSplashScreen(name) {
    // Cheking container
    if (splashscreens == null) { return; }

    // Showing our splashscreen container
    const splashscreen = splashscreens[name]
    splashscreen.removeClass("hidden")
}

// Hiding given splashscreen
function hideSplashScreen(name) {
    // Cheking container
    if (splashscreens == null) { return; }

    // Showing our splashscreen container
    const splashscreen = splashscreens[name]
    splashscreen.addClass("hidden")
}


// Will deny starting of the scan
function denyStartScan(thisButton) {
    // Adding animation to the button
    thisButton.addClass("denied");
    thisButton.text("Поля не определены")

    // Removing deny class
    setTimeout(function() {
        thisButton.removeClass("denied");
        thisButton.text("Перейти к сканированию")
    }, 2000, thisButton)
}

$(document).ready(function () {
    // Initializing containers
    containers = {
        'init': $(".init-container"),
        'start': $(".start-container"),
        'work': $(".work-container"),
        'apply_scan': $(".apply-scan-container"),
    }

    // Initializing containers
    splashscreens = {
        'badge': $(".badge-splashscreen"),
        'admin_badge': $(".admin-badge-splashscreen"),
    }

    // Start scan redirect
    $(".start-container #start-scan").click(function (e) { 
        e.preventDefault();

        // Checking is all configured
        if ($(".start-info #line-info span").text() == "Не определено") { denyStartScan($(this)); return; }
        else if ($(".start-info #step-info span").text() == "Не определено") { denyStartScan($(this)); return; }
        else if ($(".start-info #model-info span").attr("id") == undefined) { denyStartScan($(this)); return; }

        toggleContainer("work");

        // Updating test results
        const contractLotID = $("#contract-lots").children("option:selected").attr("id");
        const stepID = $("#step-info span").attr("id");
        const currentLineID = $("#fas-lines").children("option:selected").attr("id");
        getTestResultsCounter(contractLotID, stepID, currentLineID)
    });
});