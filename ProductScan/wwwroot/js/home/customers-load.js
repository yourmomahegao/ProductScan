function getContractLotsFromCustomerID(customerID) {
  const contractLots = $("#contract-lots");

  $.ajax({
    type: "GET",
    url: "Home/GetContractLotsFromCustomerID/",
    data: { CustomerID: customerID },
    dataType: "json",
    success: function (response) {
      // Adding contract lots
      if (response.status == "ok") {
        // Getting info from response
        const idList = response.id_list;
        const specList = response.spec_list;
        const idAmount = idList.length;

        // Clearing lots
        contractLots.html("");

        // Appending options to the contract lots
        for (var i = 0; i < idAmount; i++) {
          const html = `
          <option id=${idList[i]}>[${idList[i]}] ${specList[i]}</option>
          `;

          // Appending options
          contractLots.append(html);
        }
      }
    },
  });
}

$(document).ready(function () {
  const customers = $("#customers");

  // Load on start
  getContractLotsFromCustomerID(
    parseInt(customers.children("option:selected").attr("id"))
  );

  // Changing contract lots on customer change
  customers.on("change", function () {
    getContractLotsFromCustomerID(
      parseInt($(this).children("option:selected").attr("id"))
    );
  });
});
