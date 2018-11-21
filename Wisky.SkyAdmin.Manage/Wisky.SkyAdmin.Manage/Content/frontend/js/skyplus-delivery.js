
HMS.Delivery = function () {
    var echo = 0, MAX_VALUE = 500, MIN_VALUE = 1;
    var storeId, storeName;

    var init = function () {
        $(document).ready(function () {
            // Run some stuffs
            storeId = $('#hiddenStoreId').val();
            storeName = $('#hiddenStoreName').val();
            brandId = $('#hiddenBrandId').val();
            window.OnlineOrder = new DeliveryOrder(storeId, window.urls.Delivery_Create);
            renderServiceItem();
            loadAllCustomer();

            /*
             * author: TrungNDT
             * method: [EVENT] plus to current quantity
             */
            $(document).on('click', '[data-spinner="plus"]', function (e) {
                var item = $(e.currentTarget).parents('.service-item'),
                    input = item.find('.input-quantity'),
                    currentValue = $(input).val();
                currentValue++;
                $(input).val(currentValue);
            });

            /*
             * author: TrungNDT
             * method: [EVENT] (Single item) Decrease item's quantity
             */
            $(document).on('click', '.btn-minus', function () {
                var input = $(event.toElement).closest('.input-group').children('.input-quantity');
                var label = $(event.toElement).closest('.ace-spinner').closest('li').find('.label');
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
                    title: '<h5>Xác nhận<h5>',
                    message: '<div style="font-size:15px">Bạn có muốn xóa sản phẩm này không?</div>',
                    buttons: {
                        'close': {
                            'label': 'Đóng',
                            'className': 'btn-sm btn-danger',
                            'callback': function () {
                                bootbox.hideAll();
                            }
                        },
                        'ok': {
                            'label': '<i class="fa fa-ok"></i> Đồng ý',
                            'className': 'btn-sm btn-primary',
                            'callback': function () {
                                window.OnlineOrder.removeOrderDetail(orderId);
                                $('#orderItemDatatable > tbody [data-role=order-detail][data-id="' + orderId + '"]').remove();
                                //$('#total-Item').html('Tổng cộng : ' + window.OnlineOrder.getTotal().toMoney(0, ',', '.'));
                                $('#total-Item').html('Tổng cộng : ' + toMoney(window.OnlineOrder.getTotal(), ',', ''));
                            }
                        }

                    }
                });
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('#btn-order').on('click', function () {
                window.OnlineOrder.assignStore($('.nearby-stores li.active input[type=hidden]').val());
                window.OnlineOrder.assignBrand($('#hiddenBrandId').val());
                if (window.OnlineOrder.Order.CustomerID == 0) {
                    window.OnlineOrder.addNewCustomer(
                        $('#cust-name').val(),
                        $('#cust-address').val(),
                        $('#cust-phone').val(),
                        $('#cust-email').val(),
                        $('#notes').val()
                    );
                }
                var validObj = window.OnlineOrder.isValidate();
                window.OnlineOrder.addNote($('#notes').val());
                window.OnlineOrder.addWard($('#ward').val());
                window.OnlineOrder.addDistrict($('#district').val());
                window.OnlineOrder.addProvince($('#province').val());

                if (validObj.valid) {
                    window.OnlineOrder.submit(function () {
                        ShowMessage('Thêm hóa đơn thành công!', 2);
                        window.location.reload();
                    }, function (e) {
                        ShowMessage('Thêm hóa đơn thất bại!', 1);
                    });
                } else {
                    ShowMessage(validObj.msg, 1);
                }
            });

            /*
             * author: AnDND
             * method: [EVENT] 
             */
            $('#btn-charge').on('click', function () {
                var url = "/" + brandId + "/MembershipCard/" + storeId + "/MembershipCard/" + "CreateOrderBuyMembershipCardAsync";
                window.OnlineOrder.assignStore($('#hiddenStoreId').val());
                window.OnlineOrder.Order.DeliveryAddress = " ";
                window.OnlineOrder.assignBrand($('#hiddenBrandId').val());
                window.OnlineOrder.addMembershipCardCode($('#search-card-number').val());
                if (window.OnlineOrder.Order.CustomerID == 0) {
                    window.OnlineOrder.addCustomerWithId(
                        $('#cust-id').val(),
                        $('#cust-name').val(),
                        $('#cust-address').val(),
                        $('#cust-phone').val(),
                        $('#cust-email').val(),
                        $('#notes').val()
                    );
                }
                var validObj = window.OnlineOrder.isValidate();
                window.OnlineOrder.addNote($('#notes').val());
                if (validObj.valid) {
                    window.OnlineOrder.submit(function () {
                        ShowMessage('Thêm hóa đơn thành công!', 2);
                        clearAllFieldChargeCard();
                        window.OnlineOrder = new DeliveryOrder(storeId, url);

                        //window.location.reload();
                    }, function (e) {
                        ShowMessage('Thêm hóa đơn thất bại!', 1);
                    });
                } else {
                    ShowMessage(validObj.msg, 1);
                }
            });
            $('#rechargeTab').on('click', function () {
                var url = "/" + brandId + "/MembershipCard/" + storeId + "/MembershipCard/" + "CreateOrderBuyMembershipCardAsync";
                window.OnlineOrder.setSubmitUrl(url);
            });
            

            $('#btn-createOrder').on('click', function () {
                window.OnlineOrder.assignStore($('#hiddenStoreId').val());
                window.OnlineOrder.Order.DeliveryAddress = " ";
                window.OnlineOrder.assignBrand($('#hiddenBrandId').val());
                if (window.OnlineOrder.Order.CustomerID == 0) {
                    window.OnlineOrder.addCustomerWithId(
                        $('#cust-id').val(),
                        $('#cust-name').val(),
                        $('#cust-address').val(),
                        $('#cust-phone').val(),
                        $('#cust-email').val(),
                        $('#notes').val()
                    );
                }
                var validObj = window.OnlineOrder.isValidate();
                window.OnlineOrder.addNote($('#notes').val());
                if (validObj.valid) {
                    //window.OnlineOrder.submit(function () {
                    //    ShowMessage('Thêm hóa đơn thành công!', 2);
                    //    //window.location.reload();
                    //}, function (e) {
                    //    ShowMessage('Thêm hóa đơn thất bại!', 1);
                    //});
                    var url = "/" + brandId + "/MembershipCard/" + storeId + "/MembershipCard/" + "CreateOrderBuyMembershipCard"
                    $.ajax({
                        url: url,
                        type: "POST",
                        data: JSON.stringify(this.Order),
                        dataType: "json",
                        contentType: "application/json",
                        success: function (e) {
                            if (e.success) {
                                successCallback();
                                ShowMessage('Thêm hóa đơn thành công!', 2);
                                window.location.reload();
                            } else {
                                failCallback(e.msg);
                                ShowMessage('Thêm hóa đơn thất bại!', 1);
                            }
                        },
                        fail: function () {
                            //failCallback("Error is  occur, please try again!");
                            failCallback("Có lỗi xảy ra, vui lòng thử lại sau");
                            ShowMessage('Thêm hóa đơn thất bại!', 1);
                        }
                    });
                } else {
                    ShowMessage(validObj.msg, 1);
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $(document).on('change', '[data-filter="category"]', function () {
                renderServiceItem();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $(document).on('keyup', '[data-filter="name"]', function () {

                HMS.General.inputDelay(function () {
                    renderServiceItem();
                }, 1000);
            });



            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $(document).on('click', '[data-role="select-storeId"]', function (e) {
                // Remove current active
                $('.nearby-stores li.active').removeClass('active');
                // Get <li> parent
                $(this).parent().parent().parent().addClass('active');
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('#cust-address').on('focusout', function () {
                $('#txt-delivery-address').val($(this).val());
                window.OnlineOrder.Order.DeliveryAddress = $(this).val();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('#btn-search-address').click(function () {
                var address = $('#txt-delivery-address').val();
                window.OnlineOrder.Order.DeliveryAddress = address;
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('.btn-minus').on('click', function () {
                var input = $(event.toElement).closest('.input-group').children('.input-quantity');
                var label = $(event.toElement).closest('.ace-spinner').closest('li').find('.label');
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
             * method: [EVENT] 
             */
            $('body').on('blur', '.input-quantity', function () {
                var input = $(event.srcElement);
                var label = $(event.srcElement).closest('.ace-spinner').closest('li').find('.label');
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
             * method: [EVENT] Submit ordering a 'service item'
             */
            $(document).on('click', '[data-action="order-item"]', function () {
                var $item = $(this).parents('.service-item'),
                    data = $item.data('ServiceItem');
                data.quantity = parseInt($item.find('.input-quantity').val());


                //check redudant
                var allOrderDetails = window.OnlineOrder.Order.OrderDetails;
                for (var i = 0; i < allOrderDetails.length; i++) {
                    if (allOrderDetails[i]['ProductID'] == data.id) {
                        allOrderDetails[i]['Quantity'] += data.quantity;
                        var totalAmount = data.price * allOrderDetails[i]['Quantity'],
                            finalAmount = totalAmount * (100 - data.discount) / 100;
                        allOrderDetails[i]['TotalAmount'] = totalAmount;
                        allOrderDetails[i]['FinalAmount'] = finalAmount;
                        //$('#orderItemDatatable tbody tr[data-id=' + data.id + '] td[data-role=quantity]').html("");
                        //$('#orderItemDatatable tbody tr[data-id=' + data.id + '] td[data-role=total]').html("");
                        $('#orderItemDatatable tbody tr[data-id=' + allOrderDetails[i]['Id'] + '] td[data-role=quantity]').html(allOrderDetails[i]['Quantity']);
                        $('#orderItemDatatable tbody tr[data-id=' + allOrderDetails[i]['Id'] + '] td[data-role=total]').html(finalAmount);

                        $('#total-Item').html('Tổng cộng : ' + toMoney(window.OnlineOrder.getTotal(), ',', ''));
                        return;
                    }
                }
                var orderId = window.OnlineOrder.addOrderDetail(data);
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

                // Add new item as row into dataTable
                //$('#total-Item').html('Tổng cộng : ' + window.OnlineOrder.getTotal().toMoney(0, ',', '.'));
                $('#total-Item').html('Tổng cộng : ' + toMoney(window.OnlineOrder.getTotal(), ',', ''));
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('#btnIsNewUser').on('click', function () {
                var status = ($(this).data('status') == 'new');
                window.OnlineOrder.Order.CustomerID = 0;
                $('#cust-name').prop('disabled', status);
                $('#cust-phone').prop('disabled', status);
                $('#cust-address').prop('disabled', status);
                $('#cust-email').prop('disabled', status);
                $('#customers-list').prop('disabled', !status);
                $(this).data('status', status ? 'exist' : 'new');
                if (status) {
                    window.OnlineOrder.addNewCustomer('', '', '', '', '');
                    window.OnlineOrder.Order.CustomerID = $('#cust-id').val();
                }
            });
        });
    }


    var delay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();
    var loadAllCustomer = function () {
        $.ajax({
            url: window.urls.Delivery_LoadAllCustomer,
            type: 'POST',
            //success: function (data) {
            //    if (data.success) {
            //        window['customers-list'] = data.data;
            //        $.fn.select2.amd.require(['select2/compat/matcher'], function (oldMatcher) {
            //            $('#customers-list').select2({
            //                templateResult: formatSelect2,
            //                templateSelection: formatSelectedSelect2,
            //                data: data.data,
            //                matcher: oldMatcher(filterFunction),
            //                allowClear: true,
            //            });
            //        });
            //    }
            //}
            success: function (data) {
                if (data.success) {
                    window['customers-list'] = data.data;
                    $('#customers-list').select2({
                        formatResult: formatSelect2,
                        formatSelection: formatSelectedSelect2,
                        data: getListCustomer,
                        matcher: filterFunction,
                        allowClear: true,
                    });
                }
            }
        });
    }

    function renderServiceItem() {
        var filter_category = $('[data-filter="category"]').val(),
            filter_name = $('[data-filter="name"]').val();

        $.ajax({
            url: window.urls.Delivery_LoadItemByCategory,
            type: 'POST',
            data: {
                echo: ++echo,
                cateId: filter_category,
                'pattern': filter_name,
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
                    $('[data-role=service-container]').html(nodes);
                    $('.scroll-div').niceScroll();
                }
            }
        });
    }

    function filterFunction(term, text, ele) {
        return term === '' || ele.phone.indexOf(term) > -1 || ele.text.toLowerCase().indexOf(term.toLowerCase()) > -1;
        //return term === '' || ele.text.toLowerCase().indexOf(term.toLowerCase()) > -1;
    }

    //function loadCustomerOrder() {
    //    $.ajax({
    //        url: '/DeliveryManager/Delivery/OrderCustomer',
    //        type: 'POST',
    //        data: {
    //            customerId: $('#customers-list').val()
    //        },
    //        success: function (data) {
    //            $('#customer-order').html(data);
    //        },
    //        error: function () {
    //            //alert('dm');
    //        },
    //    });
    //}

    function getListCustomer() {
        //console.log("abc");
        var term = '';
        if (event.target.tagName === 'INPUT') {
            term = $(event.target).val().toLowerCase();
        }
        //if (term.length < 2) return false;
        //delay(function () {
        //    alert("Really?");
        //}, 5000);
        var count = 0;
        var tmp = window['customers-list'].filter(function (ele) {
            if (ele.text != null) {
                var text = ele.text.toLowerCase(),
                    phone = ele.phone;
                if (text == undefined || phone == undefined)
                    return false;
                else {
                    return term === '' || phone.indexOf(term) > -1 || text.indexOf(term) > -1;
                }
            }
        });
        return {
            text: 'id',
            results: tmp.slice(0, 100)
        };
    }



    function formatSelect2(state) {
        //var markup =
        //    '<div class="row">' +
        //    '<h5 style="margin-left: 7px; padding-left:5px;">' + state.text + '</h5>' +
        //    '<div class="col-xs-7" style="white-space: nowrap"><strong>SĐT: </strong>' + state.phone + '</div>' +
        //    '</div>';
        //markup += '</div>';
        var markup = state.text + " - " + state.phone;
        return markup;
    }
    function formatSelectedSelect2(state) {
        loadCustomer(state.id);
        //loadCustomerOrder();
        //InitDatatable();
        //RefreshTable();
        return state.text;
    }
    function loadCustomer(id) {
        $.ajax({
            url: window.urls.Delivery_GetCustomerDetail,
            type: 'POST',
            data: {
                'id': id
            },
            success: function (data) {
                $('#cust-name').val(data.data.name);
                $('#cust-phone').val(data.data.phone);
                $('#cust-address').val(data.data.address);
                $('#txt-delivery-address').val(data.data.address);
                window.OnlineOrder.addExistedCustomer(id, $('#notes').val());
            },
            error: function (e) {
                ShowMessage('Không thể xem chi tiết do lỗi server!', 1);
            }
        });
    }

    return {
        init: init
    }
}();
