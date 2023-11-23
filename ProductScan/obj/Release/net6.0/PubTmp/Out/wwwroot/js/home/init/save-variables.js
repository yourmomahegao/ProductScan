function saveFasLine() {
  // Saving last used fas line
  const currentLineID = $("#fas-lines").children("option:selected").attr("id");
  localStorage.setItem("last_fas_line_id", currentLineID);
}

function saveStepScan() {
  // Saving last used fas line
  const currentStepID = $("#steps-scan").children("option:selected").attr("id");
  localStorage.setItem("last_step_scan_id", currentStepID);
}

function saveRFIDSave() {
  // Saving RFDI save option
  const saveRFIDStatus = $("#save-rfid-check").is(':checked');
  localStorage.setItem("save_rfid", saveRFIDStatus);
}

function saveLogoutTime() {
  // Saving logout time
  const logoutTime = $("#logout-time").val();

  if (logoutTime != "") {
    localStorage.setItem("logout_time", (parseInt(logoutTime) * 1000).toString());
  } else {
    localStorage.setItem("logout_time", 0);
  }
}

function loadFasLine() {
  // Loading last used fas line
  const item = localStorage.getItem("last_fas_line_id");
  $(`#fas-lines option[id=${item}]`).prop("selected", true);

  // Checking is loading is right
  if (item == null) {
    return false;
  }

  // Return true if evething ok
  return true;
}

function loadStepScan() {
  // Loading last used step scan
  const item = localStorage.getItem("last_step_scan_id");
  $(`#steps-scan option[id=${item}]`).prop("selected", true);

  // Checking is loading is right
  if (item == null) {
    return false;
  }

  // Return true if evething ok
  return true;
}

function loadRFIDSave() {
  // Loading RFID save option
  const item = localStorage.getItem("save_rfid");

  // Checking is loading is right
  if (item == null) {
    return false;
  }

  // Return true if evething ok
  return true;
}

function loadLogoutTime() {
  // Loading RFID save option
  const item = localStorage.getItem("logout_time");

  // Checking is loading is right
  if (item == null) {
    return false;
  }

  // Return true if evething ok
  return true;
}

function getLogoutTime() {
    // Gettimg logout time
    const logoutTime = localStorage.getItem("logout_time");
    const defaultLogoutTime = 300000;

    // Returning value
    if (logoutTime == null || logoutTime == "" || logoutTime == NaN || logoutTime == "NaN") {
      return defaultLogoutTime;
    } else {
      return parseInt(logoutTime);
    }
}

$(document).ready(function () {
  // On save RFID value changed
  $("#save-rfid-check").change("checked", function() {
    // Getting checkbox value
    const isChecked = $(this).is(':checked');

    // Showing or hiding logout time option
    if (isChecked) {
      $("#logout-time").removeClass("hidden");
    } else {
      $("#logout-time").addClass("hidden");
    }
  });

  // Saving initialize config
  $("#save-init-config").click(function () {
    saveFasLine();
    saveStepScan();
    saveRFIDSave();
    saveLogoutTime();

    // Showing start container
    toggleContainer("start");

    // Updating start info
    updateStartInfo();
  });
});

$(window).on("load", function () {
  // Loading some variables
  try {
    const status_fas = loadFasLine();
    const status_step = loadStepScan();
    const save_rfid = loadRFIDSave();
    const logout_time = loadLogoutTime();

    // Showing container depends on status
    if (status_fas && status_step && save_rfid && logout_time) {
      toggleContainer("start");
    } else {
      toggleContainer("init");
    }
  } catch {
    // Showing init container
    toggleContainer("init");
  }

  // Updating start info
  updateStartInfo();
});
