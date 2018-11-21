HMS.Cost = function () {

    var loadOverViewCost = function (inputUrl) {
        $.ajax({
            url: inputUrl,
            dataType: "HTML",
            data: {
                'startTime': $('#startDate').val(),
                'endTime': $('#endDate').val(),
                'status': status
            },
            beforeSend: function () {

            },
            success: function (result) {
                $('#infoIndex').html(result);
            }
        });
    }

    return {
        loadOverViewCost: loadOverViewCost
    }
}();

