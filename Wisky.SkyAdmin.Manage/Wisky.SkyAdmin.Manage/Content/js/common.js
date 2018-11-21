
$(document).ready(function () {

    $("#CheckInDate").datepicker({
        showOtherMonths: true,
        selectOtherMonths: true,
        changeMonth: true,
        changeYear: true,
        dateFormat: 'dd-mm-yy',
        showHour: false,
        showMinute: false,
        showButtonPanel: true

    });
    ////_RentSubDetail
    $("#paymentDetailModal").modal('hide');

    $("#addpPaymentDetailModal").modal('hide');


    $("#btnModalCloses").click(function () {
        $("#paymentDetailModal").modal('hide');
    });

    $("#btnAddModalCloses").click(function () {
        $("#addpPaymentDetailModal").modal('hide');
    });

    $("#btnAdd").click(function () {
        var rentId = $("#rentId").val();
        document.forms["addPayment"]["rentId"].value = rentId;
        var formData = new FormData($("#addPayment")[0]);
        $.ajax({
            url: '/Rent/AddDetailPayment/',
            type: 'POST',
            data: formData,
            async: false,
            success: function (result) {
                if (result == "1") {
                    var text = "Thêm dịch vụ thành công.";
                    ShowMessage(text, 2);

                    var orderFee = parseInt($("#orderFee").val());
                    orderFee += 1000;
                    $("#orderFee").val(orderFee);

                    RentSubDetail_RefreshTable();

                    $("#addpPaymentDetailModal").modal('hide');
                } else {
                    var text = "Đã xảy ra lỗi.";
                    ShowMessage(text, 1);
                }
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });

    $("#btnUpdate").click(function () {
        var formData = new FormData($("#updatePayment")[0]);
        $.ajax({
            url: '/Rent/UpdatePayment/',
            type: 'POST',
            data: formData,
            async: false,
            success: function (result) {
                if (result == "1") {
                    var text = "Cập nhật dịch vụ thành công.";
                    ShowMessage(text, 2);

                    var orderFee = parseInt($("#orderFee").val());
                    orderFee += 1000;
                    $("#orderFee").val(orderFee);

                    RentSubDetail_RefreshTable();

                    $("#paymentDetailModal").modal('hide');
                } else {
                    var text = "Đã xảy ra lỗi.";
                    ShowMessage(text, 1);
                }
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });

    ////_RentCustomer
    $("#serviceDetailModal").modal('hide');

    //leaderName
    $("#leaderName").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/RoomManager/Rent/AutoCompleteBookerName",
                type: "POST",
                dataType: "json",
                data: { term: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        var date = new Date(parseInt(item.Time.substr(6)));
                        return {
                            label: item.BookerName + " - " + date.getHours() + "h:" + date.getMinutes() + "p:" + date.getSeconds() + "s " + date.getDate() + "/" + (date.getMonth() + 1) + "/" + date.getFullYear().toString().substring(2, 4),
                            value: item.BookerName + " - " + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds() + " " + date.getDate() + "/" + (date.getMonth() + 1) + "/" + date.getFullYear().toString().substring(2, 4)
                        };
                    }));

                }

            });
        },
        change: function (event, ui) {
            if (ui.item == null || ui.item == undefined) {
                $("#leaderName").val("");
                ShowMessage('Bạn phải chọn 1 kết quả từ mục đề xuất', 1);
                $('#checkHaveLeader').click();

            } else {
            }
        },
        minLength: 0
    }).bind('focus', function () { $(this).autocomplete("search"); });

    //CMND
    $("#personId").autocomplete({
        source: function (request, response) {
            var leader = $('#leaderName').val();
            var address = $('#address').val();
            var customerName = $('#customerName').val();
            $.ajax({
                url: "/RoomManager/Rent/AutoCompleteCmnd",
                type: "POST",
                dataType: "json",
                data: { term: request.term, leader: leader, address: address, customerName: customerName },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.StayerIDC,
                            value: item.StayerIDC
                        };
                    }));

                }

            });
        },
        minLength: 0
    }).bind('focus', function () { $(this).autocomplete("search"); });

    //Tên khách hàng
    $("#customerName").autocomplete({
        source: function (request, response) {
            var leader = $('#leaderName').val();
            var address = $('#address').val();

            $.ajax({
                url: "/RoomManager/Rent/AutoCompleteCustomerName",
                type: "POST",
                dataType: "json",
                data: { term: request.term, leader: leader, address: address },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.StayerName,
                            value: item.StayerName
                        };
                    }));

                }

            });
        },
        minLength: 0
    }).bind('focus', function () { $(this).autocomplete("search"); });

    //Quê quán
    $("#address").autocomplete({
        source: function (request, response) {
            var leader = $('#leaderName').val();
            var customerName = $('#customerName').val();
            $.ajax({
                url: "/RoomManager/Rent/AutoCompleteAddress",
                type: "POST",
                dataType: "json",
                data: { term: request.term, leader: leader, customerName: customerName },
                success: function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.StayerAddress,
                            value: item.StayerAddress
                        };
                    }));

                }

            });
        },
        minLength: 0
    }).bind('focus', function () { $(this).autocomplete("search"); });

    ////_RentService2
    $("#serviceDetailModal").modal('hide');

    $("#RentService_btnUpdate").click(function () {
        var formData = new FormData($("#updateService")[0]);
        $.ajax({
            url: '/Service/updateServiceItem/',
            type: 'POST',
            data: formData,
            async: false,
            success: function (result) {
                if (result == "1") {
                    var text = "Cập nhật dịch vụ thành công.";
                    ShowMessage(text, 2);

                    var orderFee = parseInt($("#orderFee").val());
                    orderFee += 1000;
                    $("#orderFee").val(orderFee);

                    RefreshTable();

                    $("#serviceDetailModal").modal('hide');
                } else {
                    var text = "Đã xảy ra lỗi.";
                    ShowMessage(text, 1);
                }
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });

    ////_RoomFee

    ////CheckoutByGroup
    $('table th input:checkbox').on('click', function () {
        var that = this;
        $(this).closest('table').find('tr > td:first-child input:checkbox')
        .each(function () {
            this.checked = that.checked;
            $(this).closest('tr').toggleClass('selected');
        });

    });

    ////RentBooking
    $('.priceField').each(function () {
        $(this).qtip({
            content: {
                text: function (event, api) {
                    $.ajax({
                        url: "/RoomManager/Room/GetPriceToolTip/?priceName=" + $('.priceField').find('option:selected').text() // Use href attribute as URL
                    })
                    .then(function (content) {
                        // Set the tooltip content upon successful retrieval
                        api.set('content.text', content);
                    }, function (xhr, status, error) {
                        // Upon failure... set the tooltip content to error
                        api.set('content.text', status + ': ' + error);
                    });

                    return 'Đang load...'; // Set some initial text
                }
            }
        });
    });

    $(".priceField").change(function () {
        $(this).qtip({
            content: {
                text: function (event, api) {
                    $.ajax({
                        url: "/RoomManager/Room/GetPriceToolTip/?priceName=" + $('.priceField').find('option:selected').text() // Use href attribute as URL
                    })
                    .then(function (content) {
                        // Set the tooltip content upon successful retrieval
                        api.set('content.text', content);
                    }, function (xhr, status, error) {
                        // Upon failure... set the tooltip content to error
                        api.set('content.text', status + ': ' + error);
                    });

                    return 'Đang load...'; // Set some initial text
                }

            }
        });
    });


    $(document).on('shown.bs.tab', '#myTab', function (e) {
        var text = $(e.currentTarget).find('.active');
        if (text == 'advance') {
            //console.log($('#roomTab .selectpicker'));
            $('#roomTab .selectpicker').selectpicker();

        } 
    });
});
////_RentSubDetail
function updatePaymentItem(id, paymentItem, paymentMoney) {
    $("#paymentDetailModal").modal('show');
    $("#paymentID").val(id);
    $("#paymentItem").text(paymentItem);
    $("#paymentMoney").val(paymentMoney);
    $("#paymentNote").val($("[data-type='" + id + "']").val());
}

function addPaymentItem(paymentItem, paymentMoney, paymentNote) {
    $("#addpPaymentDetailModal").modal('show');
    $("#addPaymentItem").text(paymentItem);
    $("#addPaymentMoney").val(paymentMoney);
    $("#addPaymentNote").val(paymentNote);
}



////_RentService2
function removeServiceItem(orderId) {
    bootbox.dialog({
        title: 'Xác nhận',
        message: "<h6>Bạn muốn xóa dịch vụ?</h6>",
        buttons:
        {
            "ok":
             {
                 "label": "<i class='icon-remove'></i> Đồng ý!",
                 "className": "btn-sm btn-success",
                 "callback": function () {
                     $.ajax({
                         url: "/RoomManager/Service/RemoveItem",
                         type: "GET",
                         data: { orderID: orderId },
                         cache: true,
                         success: function (result) {
                             if (result != null) {
                                 var fee = parseInt(result);
                                 updateOrderFee(fee);

                                 RefreshTable();
                             };
                             bootbox.hideAll();
                         }
                     });
                 }
             },
            "close":
             {
                 "label": "<i class='icon-ok'></i> Đóng",
                 "className": "btn-sm btn-danger",
                 "callback": function () {
                     bootbox.hideAll();
                 }
             }
        }
    });
};

////_RoomFee
function editInline(form, text) {
    $(form).show();
    $(text).hide();
}

function cancel(text, form) {
    $(text).show();
    $(form).hide();
}

function showhide(div) {
    $(div).toggle(300);
    $(div).find('.form-inline').hide();
    $(div).find('h4').show();
}

////CheckoutByGroup
function checkOutByGroup() {
    $("#checkOutModal").modal("show");
    var totalString = $("#total-payment").html();
    $("#TotalPayment").val(totalString);
}

function ConfirmCheckoutByGroup() {
    var totalString = $("#total-payment").html().replace(/\,/g, "");

    if (isNaN(totalString)) {
        return;
    }

    var total = parseInt(totalString);

    var payment = $("#Payment").val();
    if (isNaN(payment)) {
        ShowMessage("Số tiền thanh toán trước phải là số", 3);
        return;
    }

    var ids = "";
    var firstTime = true;
    $("table td input:checkbox:checked").each(function () {
        var rent = $(this).closest("tr").data('rent');
        if (firstTime) {
            firstTime = false;
            ids = rent;
        } else {
            ids += "-" + rent;
        }
    });

    $.ajax({
        url: "/RoomManager/Rent/ComfirmCheckoutByGroup",
        type: "GET",
        data: {
            ids: ids,
            total: total,
            payment: payment
        },
        success: function (result) {
            ShowMessage("Thanh toán " + result + " phòng thành công", 2);
            $("#checkOutModal").modal("hide");
            var bookingId = $("#BookingId").val();
            GetRentRooms(bookingId);
        }
    });
}

function GetRentRooms(id) {
    $.ajax({
        url: "/RoomManager/Rent/GetRentRoomsByBookingId",
        type: "GET",
        data: {
            id: id
        },
        success: function (result) {
            $("#rent-rooms").html(result);
        }
    });
}

$(".modal").on("hidden.bs.modal", function(){
    $(".modal").html("");
});