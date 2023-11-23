

// Updating start info
function updateStartInfo() {
    // Line info set
    $(".page .start-container .start-info #line-info span").text($("#fas-lines").children("option:selected").val())
    $(".page .start-container .start-info #line-info span").attr("id", $("#fas-lines").children("option:selected").attr("id"))

    $(".page .start-container .start-info #step-info span").text($("#steps-scan").children("option:selected").val())
    $(".page .start-container .start-info #step-info span").attr("id", $("#steps-scan").children("option:selected").attr("id"))
}