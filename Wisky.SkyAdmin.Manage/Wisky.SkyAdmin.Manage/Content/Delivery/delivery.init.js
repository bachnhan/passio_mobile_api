var echo = 0;
var MAX_VALUE = 500;
var MIN_VALUE = 1;

HMS.Delivery = function () {
    var init = function () {
        $(document).ready(function () {
            var storeId = $('#hiddenStoreId').val();
            var storeName = $('#hiddenStoreName').val();
            window.OnlineOrder = new DeliveryOrder(window["storeId"], '/DeliveryManager/' + storeId + '/' + storeName + '/Delivery/CreateOnlineOrder');
            /*
             * author: TrungNDT
             * method: [EVENT] plus to current quantity
             */
            $(document).on('click', '[data-spinner="plus"]', function (e) {
                var item = $(e.currentTarget).parents('.service-item'),
                    input = item.find(".input-quantity"),
                    currentValue = $(input).val();
                currentValue++;
                $(input).val(currentValue);
            });

            /*
             * author: TrungNDT
             * method: [EVENT] (Single item) Decrease item's quantity
             */
            $(document).on('click', '.btn-minus', function () {
                var input = $(event.toElement).closest(".input-group").children(".input-quantity");
                var label = $(event.toElement).closest(".ace-spinner").closest("li").find(".label");
                var currentValue = $(input).val();
                if (currentValue <= MIN_VALUE) {
                    return;
                }
                currentValue--;
                $(input).val(currentValue);
                $(label).html(currentValue);
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Remove selected order row
             */
            $(document).on('click', '[data-action="delete-ordered-item"]', function (e) {
                var orderId = parseInt($(this).parents('tr').attr('data-id'));
                bootbox.dialog({
                    title: "<h5>Xác nhận<h5>",
                    message: "<div style='font-size:15px'>Bạn có muốn xóa sản phẩm này không?</div>",
                    buttons: {

                        "close": {
                            "label": "Đóng",
                            "className": "btn-sm btn-danger",
                            "callback": function () {
                                bootbox.hideAll();
                            }
                        },
                        "ok": {
                            "label": "<i class='fa fa-ok'></i> Đồng ý",
                            "className": "btn-sm btn-success",
                            "callback": function () {
                                window.OnlineOrder.removeOrderDetail(orderId);
                                $("#orderItemDatatable > tbody [data-role=order-detail][data-id=" + orderId + "]").remove();
                                $("#total-Item").html("Tổng cộng : " + window.OnlineOrder.getTotal().toMoney(0, ",", "."));
                            }
                        }
                    }
                });
            });

            $("#btn-order").on("click", function () {
                window.OnlineOrder.assignStore($(".nearby-stores li.active input[type=hidden]").val());
                if (window.OnlineOrder.Order.CustomerID == 0) {
                    window.OnlineOrder.addNewCustomer(
                        $("#cust-name").val(),
                        $("#cust-address").val(),
                        $("#cust-phone").val(),
                        $("#cust-email").val());
                }
                var validObj = window.OnlineOrder.isValidate();
                if (validObj.valid) {
                    window.OnlineOrder.submit(function () {
                        ShowMessage("Thêm hóa đơn thành công!", 2);
                        window.location.reload();
                    }, function (e) {
                        ShowMessage("Thêm hóa đơn thất bại!", 1);
                    });
                } else {
                    ShowMessage(validObj.msg, 1);
                }
            });

            $(document).on('change', '[data-role="category-filter"]', function () {
                var categoryType = $('[data-role="category-filter"]').val();
                var name = $('[data-role="name-filter"]').val();
                loadItemByCategory(categoryType, name);
            });

            $(document).on('keyup', '[data-role="name-filter"]', function () {
                var categoryType = $('[data-role="category-filter"]').val();
                var name = $('[data-role="name-filter"]').val();
                loadItemByCategory(categoryType, name);
            });

            $(document).on("click", "[data-role=\"select-storeId\"]", function (e) {
                // Remove current active
                $(".nearby-stores li.active").removeClass("active");
                // Get <li> parent
                $(this).parent().parent().parent().addClass("active");
            });

            $("#cust-address").on("focusout", function () {
                $("#txt-delivery-address").val($(this).val());
                window.OnlineOrder.Order.DeliveryAddress = $(this).val();
            });

            $("#btn-search-address").click(function () {
                var address = $("#txt-delivery-address").val();
                window.OnlineOrder.Order.DeliveryAddress = address;
            });

            $(".btn-minus").on("click", function () {
                var input = $(event.toElement).closest(".input-group").children(".input-quantity");
                var label = $(event.toElement).closest(".ace-spinner").closest("li").find(".label");
                var currentValue = $(input).val();
                if (currentValue <= MIN_VALUE) {
                    return;
                }
                currentValue--;
                $(input).val(currentValue);
                $(label).html(currentValue);
            });

            $("body").on("blur", ".input-quantity", function () {
                var input = $(event.srcElement);
                var label = $(event.srcElement).closest(".ace-spinner").closest("li").find(".label");
                var value = $(input).val();
                if (!isNaN(value) && value <= MAX_VALUE && value >= MIN_VALUE) {
                    //valid
                    $(label).html(value);
                    return;
                }

                $(input).val($(label).html());
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Submit ordering a "service item"
             */
            $(document).on('click', '[data-action="order-item"]', function () {
                var $item = $(this).parents('.service-item'),
                    data = $item.data('ServiceItem');
                data.quantity = parseInt($item.find('.input-quantity').val());

                var orderId = window.OnlineOrder.addOrderDetail(data);

                // Add new item as row into dataTable
                var $tmpItemRow = $($('#tmpServiceItemRow').html().trim());
                $.each(data, function (i, e) {
                    var $key = $tmpItemRow.find('[data-role="' + i + '"]');
                    if ($key != undefined) {
                        $key.html(e);
                    }
                });
                $tmpItemRow.attr('data-id', orderId);
                $tmpItemRow.attr('data-role', 'order-detail');
                $tmpItemRow.find('[data-role="total"]').html(toMoney(data.price * data.quantity));
                $('#orderItemDatatable > tbody').append($tmpItemRow);
                $('#total-Item').html('Tổng cộng : ' + window.OnlineOrder.getTotal().toMoney(0, ',', '.'));
            });


            $("[data-role=\"category-filter\"]").on("change", function () {
                loadItemByCategory($(this).val(), $("[data-role=\"name-filter\"]").val());
            });

            $("[data-role=\"name-filter\"]").on("keyup", function () {
                loadItemByCategory($("[data-role=\"category-filter\"]").val(), $(this).val());
            });

            $("#btnIsNewUser").on("click", function () {
                var status = ($(this).data("status") == "new");
                $("#cust-name").prop("disabled", status);
                $("#cust-phone").prop("disabled", status);
                $("#cust-address").prop("disabled", status);
                $("#cust-email").prop("disabled", status);
                $("#customers-list").prop("disabled", !status);
                $(this).data("status", status ? "exist" : "new");
                if (status) {
                    window.OnlineOrder.addNewCustomer("", "", "", "");
                }
            });
        });
    }

    return {
        init: init
    }
}();

$(document).ready(function () {
    window.OnlineOrder = new DeliveryOrder(window["storeId"], "/DeliveryManager/Delivery/CreateOnlineOrder");

    $.ajax({
        url: "/CRM/Customer/LoadAllCustomer",
        type: "POST",
        success: function (data) {
            if (data.success) {
                //console.log(data.data);
                //window["customers-list"] = data.data;
                $("#customers-list").select2({
                    data: data.data,
                    allowClear: true,
                });
            }
        }
    });

    $(".scroll-div").niceScroll();

    loadItemByCategory(-1, "");

});




function loadItemByCategory(cateId, pattern) {
    $.ajax({
        url: "/DeliveryManager/Delivery/LoadItemByCategory",
        type: "POST",
        data: {
            echo: ++echo,
            cateId: cateId,
            'patterm': pattern,
            storeID: window["storeId"]
        },
        cache: true,
        success: function (result) {
            if (result.echo >= echo) {
                var nodes = [];
                $.each(result.products, function (i, e) {
                    var node = $($('#tmpServiceItem').html().trim());
                    node.children().data('ServiceItem', e);
                    node.find('[data-role="name"]').html(e.name);
                    node.find('[data-role="price"]').html(e.price);
                    nodes.push(node);
                });
                $("[data-role=service-container]").html(nodes);

            }
        }
    });
}

function filterFunction(term, text, ele) {
    //console.log(term);
    return term === "" || ele.phone.indexOf(term) > -1 || ele.text.toLowerCase().indexOf(term.toLowerCase()) > -1;
}

function loadCustomerOrder() {
    $.ajax({
        url: "/DeliveryManager/Delivery/OrderCustomer",
        type: "POST",
        data: {
            customerId: $("#customers-list").val()
        },
        success: function (data) {
            $("#customer-order").html(data);
        },
        error: function () {
            alert("dm");
        },
    });
}

function getListCustomer() {
    var term = "";
    if (event.target.tagName === "INPUT") {
        term = $(event.target).val();
    }
    var tmp = window["customers-list"].filter(function (ele) {
        return term === "" || ele.phone.indexOf(term) > -1 || ele.text.toLowerCase().indexOf(term.toLowerCase()) > -1;
    });
    return {
        text: "id",
        results: tmp
    };
}

function formatSelectedSelect2(state) {
    loadCustomer(state.id);
    loadCustomerOrder();
    //InitDatatable();
    //RefreshTable();
    return state.text;
}

function formatSelect2(state) {
    var markup =
        "<div class=\"row\">" +
        "<h5 style=\"margin-left: 7px\">" + state.text + "</h5>" +
        "<div class=\"col-xs-7\" style=\"white-space: nowrap\"><strong>SĐT: </strong>" + state.phone + "</div>" +
        "</div>";
    markup += "</div>";
    return markup;
}

function loadCustomer(id) {
    $.ajax({
        url: "/CRM/Customer/GetCustomerDetailJson",
        type: "POST",
        data: {
            'id': id
        },
        success: function (data) {
            //console.log("abc");
            $("#cust-name").val(data.data.name);
            $("#cust-phone").val(data.data.phone);
            $("#cust-address").val(data.data.address);
            $("#txt-delivery-address").val(data.data.address);
            window.OnlineOrder.addExistedCustomer(id);
        },
        error: function (e) {
            ShowMessage("Không thể xem chi tiết do lỗi server!", 1);
        }
    });
}

function deleteOrderDetail(orderId) {


}
