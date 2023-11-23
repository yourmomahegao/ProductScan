function updateModelInfo(CustomerID, ContractLotID) {
    // Getting model info
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetModel/`,
        data: {CustomerID: CustomerID, ContractLotID: ContractLotID},
        dataType: "json",
        success: function (response) {
            if (response.status == 'ok') {
                // Updating model info
                const modelInfoSpan = $("#model-info span");
                modelInfoSpan.text(response.model_name);
                modelInfoSpan.attr("id", response.model_id);

                // Updating fas number
                const fasNumberFormat2 = $("#fas-number-format2 span");
                fasNumberFormat2.attr("value", response.fas_number_format_2);
                
                // Constructing FAS number format
                const fasNumberFormatList = response.fas_number_format_2.split(";")
                let fasNumberFormatListFixed = new Array();

                fasNumberFormatList.forEach(element => {
                    fasNumberFormatListFixed.push(element.substring(0, (element.length - 1) - 6));
                });

                // Constructing new string
                const fasNumberFormatFixed = fasNumberFormatListFixed.join(" ").toString()

                // Setting value
                fasNumberFormat2.text(fasNumberFormatFixed);
            }
        }
    });
}

$(document).ready(function () {
    // On contract lot changed
    $("#contract-lots").on("change", function() {
        const CustomerID = $("#customers").children("option:selected").attr("id");
        const ContractLotID = $("#contract-lots").children("option:selected").attr("id");

        // Updating model info
        updateModelInfo(CustomerID, ContractLotID);
    });
});