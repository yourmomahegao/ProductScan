function getLastOperationsFromOperLog(PCBID) {
    $.ajax({
        type: "GET",
        url: `${SERVER_ROOT}/GetLastOperationsFromOperLog`,
        data: { Amount: 10, PCBID: PCBID },
        dataType: "json",
        success: function (response) {
            console.log(response);

            // Checking request status
            if (response.status != 'ok') { return; }

            // Getting container
            const container = $("#last-logs-info");

            // Clearing container
            container.html("");

            // Checking user existance
            if (response.success == "true") {
                // Table start info
                const startHtml = `
                <tr id="-1">
                    <th id="pcbid-label">PCBID</th>
                    <th id="step-name-label">Имя шага</th>
                    <th id="result-label">Результат</th>
                    <th id="username-label">Пользователь</th>
                    <th id="line-name-label">Линия</th>
                    <th id="step-date-label">Дата</th>
                </tr>
                `;

                // Appending start HTML
                container.append(startHtml);

                response.responseData.forEach(log => {
                    // Constructing HTML
                    const html = `
                    <tr id="${log.id}">
                        <th id="pcbid">${log.pcbid}</th>
                        <th id="step-name">${log.stepName}</th>
                        <th id="result">${log.result}</th>
                        <th id="username">${log.userName}</th>
                        <th id="line-name">${log.lineName}</th>
                        <th id="step-date">${log.stepDate}</th>
                    </tr>
                    `;     

                    // Appending to HTML
                    container.append(html);
                });
            }

            // Showing error message
            else {
                
            }
        }
    });
}

$(document).ready(function () {

});