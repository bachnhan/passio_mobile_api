HMS.DualScreen = function () {
    var init = function () {
        $(document).ready(function () {
            /*
             * author: TrungNDT
             * method: [EVENT] Remove all tabs except pinned when close modal
             */
            $(document).on('hidden.bs.modal', '.modal', function () {
                if ($(this).find('.modal-dialog').data('role') == 'dual-screen') {
                    var pinnedTab = [
                        //'tab-payment',
                        'tab-order'
                    ];
                    $.each($('#tabContent .tab-pane'), function (i, e) {
                        var tabId = $(e).attr('id');
                        if (pinnedTab.indexOf(tabId) < 0) {
                            $(e).remove();
                            $('#tabHandler').find('[href="#' + tabId + '"]').parent().remove();
                        }
                    });
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Update total price when something added to service table
             */
            $(document).on('service.change', '#table-item-service', function () {
                //console.log('service.change');
                sumTotalServiceCost();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Click to change room
             */
            $(document).on('click', '[data-model="changeRoom"]', function (e) {
                var me = $(this);
                var currentRoomId = $('#roomId').val();
                var newRoomId = me.data('roomid');
                var roomName = me.data('roomname');
                bootbox.dialog({
                    title: 'Xác nhận',
                    message: "<h6>Bạn có chắc đổi qua <span style='color:blue;font-weight:bold'>" + roomName + "</span> này không?</h6>",
                    buttons:
                    {
                        "ok":
                        {
                            "label": "<i class='icon-ok'></i> Đồng ý!",
                            "className": "btn-sm btn-success",
                            "callback": function () {
                                changeRoom(newRoomId);
                                //window["current-room"] = roomId;
                                //bootbox.hideAll();
                                //var room = $('.room[data-id=' + newRoomId + ']');
                                //room.attr('data-rentId', rentId);
                                //location.reload();

                                var targetId = me.attr('data-roomid');
                                $('.room[data-id=' + newRoomId + ']').attr('data-state', 1);
                                $('.room[data-id=' + targetId + ']').attr('data-state', 2);
                                $('.room[data-id=' + targetId + ']').attr('data-rentid', $('#rentId').val());
                                $('.room[data-id=' + currentRoomId + ']').attr('data-rentid', $('#rentId').val(""));
                                $('#modalRentDetails').modal('hide');
                            }
                        },
                        "close":
                        {
                            "label": "<i class='icon-remove'></i> Đóng",
                            "className": "btn-sm btn-danger",
                            "callback": function () {
                                bootbox.hideAll();
                            }
                        }
                    }
                });

            });
        });
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var loadTabOrder = function () {
        getSubScreenTab('order', '/DualScreen/Tab/TabOrder', {}, function () {
            window.HMS.ServiceItemHandler.renderServiceItem();
        });
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var loadTabCustomer = function (rentId) {
        var params = {
            rentId: rentId
        };
        getSubScreenTab('customer', '/DualScreen/Tab/TabCustomer', params);
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var loadTabPayment = function (rentId) {
        var params = {
            rentId: rentId
        };
        getSubScreenTab('payment', '/DualScreen/Tab/TabPayment', params, function() {
            HMS.Payment.checkPaymentTableDisplaying();
        });
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var loadTabRentFee = function (rentId) {
        var params = {
            rentId: rentId
        };
        getSubScreenTab('rentfee', '/DualScreen/Tab/TabRentFee', params, function () {
            //HMS.rentDetail.setupBaseDateTimePicker();
            HMS.RentFeeSection.getGeneralValues();
            HMS.RentFeeSection.loadFeeDetail();
            //HMS.RentFeeSection.renderProcessBar();
        });
    }

    /*
     * author: SinhCV
     * method: 
     */
    var loadTabChangeRoom = function () {
        getSubScreenTab('changeRoom', '/DualScreen/' + $('#hiddStoreId').val() + '/' + $('#hiddStoreName').val() + '/Tab/TabChangeRoom', {});
    };

    /*
     * author: SinhCV
     * method: Change Room
     */
    var changeRoom = function (newRoomId) {
        var rentId = $("#rentId").val();
        var roomId = $("#roomId").val();
        //console.log("DM2");
        $.ajax({
            url: "/RoomManager/Rent/ChangeRoom",
            type: "GET",
            data: { rentId: rentId, roomId: roomId, newRoomId: newRoomId },
            success: function (result) {
                if (result == "1") {
                    $('#modal').modal('hide');
                    updateRoomStatus(roomId);
                    updateRoomStatus(newRoomId);
                    ShowMessage('Chuyển phòng thành công', 2);
                }
                else {
                    ShowMessage("Vui lòng kiểm tra trạng thái phòng được chuyển đã sẵn sàng!", 3);
                }
            }
        });
    };

    /*
     * author: SinhCV
     * method: [EVENT] 
     */
    var updateRoomStatus = function (roomId) {
        $.ajax({
            url: '/RoomManager/Room/GetRoomState',
            data: {
                roomId: roomId
            },
            success: function (data) {
                if (data.success) {
                    $('.room[data-id="' + roomId + '"]').attr('data-state', data.data);
                }
            }
        });
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var renderTabHandlers = function () {
        var $tabHandler = $('#tabHandler').empty();
        var tabData = [
            {
                name: 'Thực đơn',
                href: '#tab-order'
            },
            {
                name: 'Khách hàng',
                href: '#tab-customer'
            }, {
                name: 'Tiền phòng',
                href: '#tab-rentfee'
            },
            {
                name: 'Chuyển phòng',
                href: '#tab-changeRoom'
            }
        ];
        $.each(tabData, function (i, e) {
            // Render tab handler
            $tabHandler.append($('<li/>', {
                html: $('<a/>', {
                    'data-toggle': 'tab',
                    'href': e.href,
                    'html': e.name
                })
            }));
        });
    };

    /*
     * author: TrungNDT
     * method: Init DualScreen into main page
     */
    var loadDualScreen = function (modalID) {
        $(modalID).addClass('modal-fullscreen');
        $.ajax({
            url: '/DualScreen/Screen/GetDualScreen',
            type: 'html',
            success: function (result) {
                $(modalID).html(result);
                loadTabOrder();
            }
        });
    };

    /*
     * author: TrungNDT
     * method: [SUPPORT] 
     */
    var loadTableItemService = function (content) {
        $('#table-item-service').html(content);
        $('#table-item-service').trigger('service.change');
    };

    /*
     * author: TrungNDT
     * method: [SUPPORT] Calculate total service cost
     */
    var sumTotalServiceCost = function () {
        var $txtRoomAmount = $('#txtRoomAmount'),
            $txtDualDiscount = $('#txtDualDiscount'),
            $txtDualTotal = $('#txtDualTotal'),
            $txtDualPayment = $('#txtDualPayment'),
            sum = 0;
        //console.log('Test: ' + $('#txtRoomAmount').val());
        // Sum all items in service table
        $.each($('#table-item-service [data-role="totalPrice"]'), function (i, e) {
            sum += moneyToNumber($(e).html());
        });

        // Plus room cost
        sum += moneyToNumber($txtRoomAmount.val());

        $txtDualTotal.val(toMoney(sum));
        $('#hiddenRemaining').val(sum)

        // Display payment (Total - Discount)
        var totalPayment = moneyToNumber($txtDualTotal.val())
            - moneyToNumber($txtDualDiscount.val());
        $txtDualPayment.val(toMoney(totalPayment));
        //console.log('sumTotalServiceCost');
        //console.log($txtRoomAmount.val());
    }

    /*
     * author: TrungNDT
     * method: [SUPPORT] Active first tab
     */
    var activeFirstTab = function () {
        $('#tabHandler a:first').tab('show'); // Select first tab
    };

    /*
     * author: TrungNDT
     * method: [SUPPORT] Get tab constant
     */
    var getTabsConst = function (tag) {
        return {
            'order': {
                name: 'Thực đơn',
                selector: '#tab-order'
            },
            'customer': {
                name: 'Khách hàng',
                selector: '#tab-customer'
            },
            'rentfee': {
                name: 'Tiền phòng',
                selector: '#tab-rentfee'
            },
            'payment': {
                name: 'Thanh toán',
                selector: '#tab-payment'
            },
            'changeRoom': {
                name: 'Chuyển phòng',
                selector: '#tab-changeRoom'
            }
        }[tag];
    };

    /*
     * author: TrungNDT
     * method: [SUPPORT] Set Screen title
     * params: [String] title: modal title
     */
    var setScreenTitle = function (title) {
        $('[data-role="dual-screen"] .modal-title').html(title); // Select first tab
    };

    /*
     * author: TrungNDT
     * method: Clear tab content
     */
    var clearTabContent = function () {
        $('#tabContent').empty();
    };

    /*
     * author: TrungNDT
     * method: [SUPPORT] Load given tab and append to 'right' screen
     * params: [String] tag: Tab name
     * params: [String] action: Server's function name (ex: '/DualScreen/Screen/GetDualScreen')
     * params: [Function] callback: Callback function
     */
    var getSubScreenTab = function (tag, action, params, callback) {
        params = params || {};
        var selectedTab = getTabsConst(tag);
        if ($('#tabContent').has(selectedTab.selector).length == 0) {
            //console.log('getSubScreenTab: ' + tag);
            //$(selectedTab.selector).remove();
            // Render tab content
            $.ajax({
                url: action,
                type: 'html',
                data: params,
                async: false,
                success: function (result) {
                    var $tabContent = $('#tabContent');
                    $tabContent.append(result);
                    if (callback != undefined) callback();
                },
                error: function (e) {
                    //console.log(e);
                }
            });
        }
    };

    return {
        init: init,
        loadDualScreen: loadDualScreen,
        activeFirstTab: activeFirstTab,
        loadTabOrder: loadTabOrder,
        loadTabCustomer: loadTabCustomer,
        loadTabPayment: loadTabPayment,
        loadTableItemService: loadTableItemService,
        loadTabChangeRoom: loadTabChangeRoom,
        getSubScreenTab: getSubScreenTab,
        renderTabHandlers: renderTabHandlers,
        clearTabContent: clearTabContent,
        loadTabRentFee: loadTabRentFee,
        sumTotalServiceCost: sumTotalServiceCost
    };
}();

/*
 * author: TrungNDT
 * method: Service Item Handler
 *          
 */
HMS.ServiceItemHandler = function () {
    var init = function () {
        $(document).ready(function () {
            /*
             * author: TrungNDT
             * method: setup tagsinput & spinner for extra modal & single modal.
             */
            $('#modalExtra, #modalSingle').on('shown.bs.modal', function () {
                // Setup tagsinput
                $('[data-type="tagsinput"]').tagsinput();
                var $hiddenComment = $('#hiddenComment');
                if ($hiddenComment.length > 0) {
                    $('[data-type="tagsinput"]').tagsinput('add', $hiddenComment.val());
                }
                //setupQuantitySpinner();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] (Single item) Decrease item's quantity
             */
            $('.modal-order').on('click', '.btn-minus', function () {
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
             * method: [EVENT] handle event when click on service item based on their type
             *      - Single/Combo: plus to current quantity
             *      - Extra/General: open popup modal to choose extra/general
             */
            $('.modal-order').on('click', '[data-spinner="plus"]', function (e) {
                var me = $(e.currentTarget);
                var item = me.parents('.service-item');
                // If product is extra or general, show extra popup
                var getExtra = function (productId) {
                    $.ajax({
                        url: '/DualScreen/ExtraModal/GetExtra',
                        type: 'GET',
                        data: { productId: productId },
                        dataType: 'html',
                        success: function (result) {
                            $('#modalExtra').html(result);
                            $('#modalExtra').modal('show');
                        }
                    });
                };
                var getGeneral = function (productId) {
                    $.ajax({
                        url: '/DualScreen/ExtraModal/GetGeneral',
                        type: 'GET',
                        data: { productId: productId },
                        dataType: 'html',
                        success: function (result) {
                            $('#modalExtra').html(result);
                            $('#modalExtra').modal('show');
                        }
                    });
                }; // If product is single, plus 1 to current quantity
                var countSingleProduct = function () {
                    //var label = $(event.toElement).closest('li').find('.label');
                    var input = item.find('.input-quantity');

                    var currentValue = $(input).val();

                    if (currentValue >= MAX_VALUE) {
                        return;
                    }
                    currentValue++;
                    $(input).val(currentValue);
                };
                switch (item.data('type')) {
                    case 'single':
                        countSingleProduct();
                        break;
                    case 'extra':
                        getExtra(item.data('serviceItem').id);
                        break;
                    case 'general':
                        getGeneral(item.data('serviceItem').id);
                        break;
                    case 'combo':
                        countSingleProduct();
                        break;
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Submit ordering amount of single serviceItem
             */
            $('#modalExtra, .modal-order').on('click', '[data-action="order-item"]', function (e) {
                var me = $(e.currentTarget);
                var type = me.data('type');
                //var orderType = parseInt($('[data-role='order-container']').data('type'));

                var tempOrderItem = {};

                // Case: order a single/combo item
                var orderSingleItem = function () {
                    var item = me.parents('.service-item');
                    var quantityPlace = item.find('.input-quantity');
                    var quantity = parseInt(quantityPlace.val()),
                        unitPrice = item.data('serviceItem').price;
                    var rentId = $('#rentId').val();
                    var sumPrice = quantity * unitPrice;

                    // Save info into tempOrderItem variable
                    tempOrderItem = {
                        name: item.find('.title label').html(),
                        unitPrice: toMoney(unitPrice),
                        quantity: quantity,
                        totalPrice: toMoney(sumPrice),
                        hasExtra: 'False',
                        type: item.data('type'),
                    };
                    $.ajax({
                        url: '/RoomManager/Service/AddItem',
                        type: 'GET',
                        data: {
                            rentId: rentId,
                            productId: item.data('serviceItem').id,
                            quantity: quantity
                        },
                        success: function (result) {
                            //console.log(result);
                            if (result != null) {
                                $('#rentId').val(result['rentId']);
                                quantityPlace.val(1);
                                ShowMessage(item.find('.title').html() + ' : ' + quantity, 2);
                                renderTemporaryOrderItem(tempOrderItem, result['orderId']);
                                $('#modalSingle').modal('hide');
                            }
                        }
                    });
                }; // CASE: Edit extra/general item
                var orderPopupItem = function () {
                    // Declare these variables for saving to database
                    var extraData = [];
                    var rentId = $('#rentId').val();
                    var modalExtra = $('#modalExtra');

                    var productId = parseInt(modalExtra.find('#hiddenProductId').val()),
                        productName = modalExtra.find('#hiddenProductName').val(),
                        productQuantity = parseInt(modalExtra.find('#numProductQuantity').val()),
                        unitPrice = parseInt(modalExtra.find('#hiddenProductPrice').val()),
                        comment = modalExtra.find('#inputComment').val();

                    // Declare tempOrderItem variable for rendering temporary order record
                    tempOrderItem = {
                        name: productName,
                        unitPrice: toMoney(unitPrice),
                        quantity: productQuantity,
                        totalPrice: toMoney(calculateOrderItemPrice()),
                        hasExtra: 'True',
                        type: modalExtra.find('#hiddenType').val(),
                        children: [
                            {
                                name: '1 x ' + modalExtra.find('#hiddenProductName').val(),
                                unitPrice: toMoney(unitPrice)
                            }
                        ]
                    };
                    $.each($('[data-type="extra-group"] .extra-count[data-quantity]'), function (i, e) {
                        var extraId = parseInt($(e).data('id')),
                            name = $(e).parents('.extra-item').find('.extra-name').html(),
                            unitPrice = parseInt($(e).data('price')),
                            quantity = parseInt($(e).data('quantity'));
                        extraData.push({
                            'ProductExtraId': extraId,
                            'Price': unitPrice,
                            'Quantity': quantity
                        });
                        if (quantity > 0) {
                            tempOrderItem['children'].push({
                                name: quantity + ' x ' + name,
                                unitPrice: toMoney(unitPrice),
                            });
                        }
                    });

                    $.ajax({
                        url: '/RoomManager/Service/AddItemWithExtra',
                        type: 'POST',
                        data: {
                            rentId: rentId,
                            productId: productId,
                            quantity: productQuantity,
                            extraDetails: extraData,
                            comment: comment
                        },
                        success: function (result) {
                            if (result != null) {
                                ShowMessage(productName + ' : ' + productQuantity, 2);
                                $('#rentId').val(result['rentId']);
                                renderTemporaryOrderItem(tempOrderItem, result['orderId']);
                                $('#modalExtra').modal('hide');
                            }
                        }
                    });
                };
                switch (type) {
                    case 'spinner':
                        orderSingleItem();
                        break;
                    case 'popup':
                        orderPopupItem();
                        break;
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Submit editing an existing order
             */
            $('#modalExtra, #modalSingle').on('click', '[data-action="order-edit-item"]', function (e) {
                var extraData = [];
                $.each($('[data-type="extra-group"] .extra-count[data-quantity]'), function (i, e) {
                    extraData.push({
                        'ProductExtraId': parseInt($(e).data('id')),
                        'Price': ($(e).data('price')),
                        'Quantity': parseInt($(e).data('quantity'))
                    });
                });
                var me = $(e.currentTarget),
                    type = me.data('type'),
                    tempOrderItem;

                var getUpdateData = function (modal) {
                    return {
                        orderId: parseInt(modal.find('#hiddenOrderId').val()),
                        quantity: parseInt(modal.find('#numProductQuantity').val()),
                        comment: modal.find('#inputComment').val()
                    };
                };
                var updateSingleItem = function (data) {
                    tempOrderItem = data;
                    tempOrderItem['totalPrice'] = toMoney(calculateOrderItemPrice());
                    $.ajax({
                        url: '/RoomManager/Service/UpdateItem',
                        type: 'POST',
                        data: {
                            orderId: data.orderId,
                            quantity: data.quantity,
                            comment: data.comment
                        },
                        success: function (result) {
                            if (result != null) {
                                ShowMessage('Cập nhật thành công!');
                                HMS.Room.loadItemService();
                                $('#modalSingle').modal('hide');
                            }
                        }
                    });
                };
                var updateExtraItem = function (data) {
                    tempOrderItem = data;
                    tempOrderItem['totalPrice'] = toMoney(calculateOrderItemPrice());
                    $.ajax({
                        url: '/RoomManager/Service/UpdateItemWithExtra',
                        type: 'POST',
                        data: {
                            orderId: data.orderId,
                            quantity: data.quantity,
                            comment: data.comment,
                            extraDetails: extraData
                        },
                        success: function (result) {
                            if (result != null) {
                                ShowMessage('Cập nhật thành công!');
                                HMS.Room.loadItemService();
                                $('#modalExtra').modal('hide');
                            }
                        }
                    });
                };
                switch (type) {
                    case 'single':
                        updateSingleItem(getUpdateData($('#modalSingle')));
                        break;
                    case 'extra':
                        updateExtraItem(getUpdateData($('#modalExtra')));
                        break;
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Change backdrop z-index when multiple modal is shown
             */
            $('.modal-overlap, .bootbox').on('shown.bs.modal', function () {
                $(this).css('z-index', 1060);
                $('.modal-backdrop').last().css('z-index', 1059);
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Update id, price, extra items when change general item
             */
            $('#modalExtra').on('change', 'input[name="general-item-control"]', function (e) {
                selectGeneralItem();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Change current product quantity
             */
            $('#modalExtra, #modalSingle').on('change', 'input[data-action="change-product-quantity"]', function (e) {
                calculateOrderItemPrice();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Plus 1 to current extra object
             */
            $('#modalExtra').on('click', '[data-action="plus-extra"]', function (e) {
                event.stopPropagation();
                event.preventDefault();
                var me = $(e.currentTarget),
                    hiddenRemainingExtra = $('#hiddenRemainingExtra');
                var currentQuantity = parseInt(me.parent().data('quantity') || '0'),
                    maxThisExtra = parseInt(me.parent().data('max')),
                    remainingExtra = parseInt(hiddenRemainingExtra.val() || 5);

                if (remainingExtra > 0 && currentQuantity < maxThisExtra) {
                    currentQuantity++;
                    hiddenRemainingExtra.val(--remainingExtra);
                    $('#lblRemainingExtra').html(remainingExtra);
                } else {
                }

                //currentQuantity++;
                me.parent().data('quantity', currentQuantity);
                if (currentQuantity > 0) me.html(currentQuantity);
                calculateOrderItemPrice();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Minus 1 to current extra object
             */
            $('#modalExtra').on('click', '[data-action="minus-extra"]', function (e) {
                event.stopPropagation();
                event.preventDefault();
                var me = $(e.currentTarget),
                    hiddenRemainingExtra = $('#hiddenRemainingExtra');
                var currentQuantity = parseInt(me.parent().data('quantity') || '0'),
                    remainingExtra = parseInt(hiddenRemainingExtra.val());

                if (remainingExtra < 5) {
                    hiddenRemainingExtra.val(++remainingExtra);
                    $('#lblRemainingExtra').html(remainingExtra);
                }
                if (currentQuantity > 0) {
                    currentQuantity--;
                }

                me.parent().data('quantity', currentQuantity);
                if (currentQuantity == 0) {
                    me.siblings('.extra-quantity').empty();
                } else {
                    me.siblings('.extra-quantity').html(currentQuantity);
                }
                calculateOrderItemPrice();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Open edit modal to edit ordered item. 
             *          Choosing modal to open based on type of order.
             */
            $('.modal-order').on('click', '[data-action="open-edit-order-modal"]', function (e) {
                var me = $(e.currentTarget);
                var orderId = parseInt(me.data('orderid'));
                var getUpdatePopup = function () {
                    $.ajax({
                        url: '/DualScreen/ExtraModal/UpdateOrderExtra',
                        type: 'GET',
                        data: { orderId: orderId },
                        dataType: 'html',
                        success: function (result) {
                            $('#modalExtra').html(result);
                            $('#modalExtra').modal('show').one('shown.bs.modal', function () {
                                calculateOrderItemPrice();
                            });
                        }
                    });
                };
                var getUpdateSingle = function () {
                    $.ajax({
                        url: '/DualScreen/ExtraModal/UpdateOrderSingle',
                        type: 'GET',
                        data: { orderId: orderId },
                        dataType: 'html',
                        success: function (result) {
                            $('#modalSingle').html(result);
                            $('#modalSingle').modal('show').one('shown.bs.modal', function () {
                                calculateOrderItemPrice();
                            });
                        }
                    });
                };
                if (me.data('hasExtra') == 'True') {
                    getUpdatePopup();
                } else {
                    switch (me.data('type').toLowerCase()) {
                        case 'combo':
                            getUpdateSingle();
                            break;
                        case 'general':
                            getUpdatePopup();
                            break;
                        case 'single':
                            getUpdateSingle();
                            break;
                    }
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Click on red 'x' button at an ordered item to remove it
             */
            $('.modal-order').on('click', '[data-action="remove-single-order"]', function (e) {
                var me = $(e.currentTarget),
                    orderRow = me.parents('.main-order-row');

                var id = me.data('id');

                bootbox.confirm('Bạn có muốn xóa đơn đặt hàng này?', function (result) {
                    if (result) {
                        orderRow.nextUntil('.main-order-row').detach();
                        orderRow.remove();
                        $('#table-item-service').trigger('service.change');
                        $.ajax({
                            url: '/RoomManager/Service/DeleteOrderDetail',
                            type: 'GET',
                            data: { orderDetailId: id },
                            success: function (result) {
                                if (result.success) {
                                    ShowMessage("Xóa item thành công", 2)
                                } else {
                                    ShowMessage("Xóa item không thành công", 1)
                                }
                                //HMS.deliveryManagement.loadItemServiceDelivery();
                            }
                        });
                    }
                });
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Click on red 'x' button at very top to remove all orders
             */
            $('.modal-order').on('click', '[data-action="remove-all-orders"]', function (e) {
                bootbox.confirm('Bạn có chắc muốn xóa tất cả đơn đặt hàng?', function (result) {
                    if (result) {
                        //$.ajax({
                        //    url: '/DeliveryManager/Delivery/ClearAllOrder',
                        //    type: 'GET',
                        //    success: function (result) {
                        //        HMS.deliveryManagement.loadItemServiceDelivery();
                        //    }
                        //});
                    }
                });

            });

            /*
             * author: TrungNDT
             * method: [EVENT] Filter service item by category
             */
            $('.modal-order').on('change', '[data-filter="category"]', function () {
                renderServiceItem();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Filter service item by nam
             */
            $('.modal-order').on('keyup', '[data-filter="name"]', function () {
                renderServiceItem();
            });

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

            $(document).on('change', '#cbbChangeMenuCategory', function () {
                var catId = $('#cbbChangeMenuCategory').val();
                filterFoodByCategory(catId);
            });

        });
    }; /*
     * author: TrungNDT
     * method: 
     */
    var refreshServiceTable = function () {
        var oTable = $('#serviceItemDatatable').dataTable();
        oTable._fnPageChange(0);
        oTable._fnAjaxUpdate();
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var updateOrderFee = function (amount) {
        var orderFee = parseInt($('#hdfOrderFee').val());
        orderFee += parseInt(amount);
        $('#hdfOrderFee').val(orderFee);
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var updateServiceItem = function (id, serviceItem, quantity, discount) {
        $('#serviceDetailModal').modal('show');
        $('#serviceID').val(id);
        $('#serviceItem').text(serviceItem);
        $('#quantity').val(quantity);
        $('#discount').val(discount);
    };

    /*
     * author: TrungNDT
     * method: Make product quantity control become spinner
     * param: {number} value in textbox
     */
    var setupQuantitySpinner = function (value) {
        $('#numProductQuantity').ace_spinner({
            value: value || 1,
            min: 0,
            max: 99,
            step: 1,
            on_sides: true,
            icon_up: 'ace-icon fa fa-plus smaller-75',
            icon_down: 'ace-icon fa fa-minus smaller-75',
            btn_up_class: 'btn-success',
            btn_down_class: 'btn-danger'
        });
    };

    /*
     * author: TrungNDT
     * method: If img src is not found, set it to default img
     */
    var resetDefaultImage = function (img) {
        $(img).attr('src', '/Content/images/Default_product_img.jpg');
    };
    var echo = 0;

    /*
     * author: TrungNDT
     * method: Load service items based on category id, typing them by type
               (single/combo/extra/general)
     */
    var renderServiceItem = function () {
        var $tabOrder = $('#tab-order'),
            filter_category = $tabOrder.find('[data-filter="category"]').val(),
            filter_name = $tabOrder.find('[data-filter="name"]').val(),
            storeId = $('#hiddenStoreId').val();

        var getFooterPanel = function () {
            return $($('#tmpServiceItemFooter').html().trim()).clone();
        };
        var getTouchSpinner = function () {
            return $($('#tmpTouchSpinner').html().trim()).clone();
        };
        $.ajax({
            url: '/RoomManager/Service/LoadItemByCategory',
            type: 'POST',
            data: {
                echo: ++echo,
                cateId: filter_category,
                patterm: filter_name,
                storeID: storeId
            },
            cache: true,
            success: function (result) {
                if (result.echo >= echo) {
                    var nodes = [];
                    $.each(result.products, function (i, e) {
                        var node = $($('#tmpServiceItem').html().trim());
                        node.children().data('service-item', e);
                        //node.find('img').attr('src', e.image);
                        node.find('[data-role="name"]').html(e.name);
                        node.find('[data-role="price"]').html(e.price);
                        //console.log(e);
                        // Setup footer for item based on their type
                        //console.log(e.name + ' - ' + e.hasExtra + ' - ' + e.type);
                        if (e['hasExtra']) {
                            node.children().addClass('item-extra');
                            node.children().data('type', 'extra');
                            node.children().append(getFooterPanel());
                        } else {
                            switch (e['type']) {
                                case 'Combo':
                                    node.children().addClass('item-combo');
                                    node.children().data('type', 'combo');
                                    node.children().append(getTouchSpinner(e));
                                    break;
                                case 'General':
                                    node.children().addClass('item-general');
                                    node.children().data('type', 'general');
                                    node.children().append(getFooterPanel());
                                    break;
                                case 'Single':
                                    node.children().addClass('item-single');
                                    node.children().data('type', 'single');
                                    node.children().append(getTouchSpinner(e));
                                    break;
                            }
                        }
                        nodes.push(node);
                    });
                    $('#service-container').html(nodes);
                }
            }
        });
    };

    /*
     * author: TrungNDT
     * method: (In modalExtra/modalGeneral) Calculate total price when:
     *      - Add/Remove some extra product
     *      - Change product quantity
     *      - Change product (general)
     */
    var calculateOrderItemPrice = function () {
        // Get Product Price & Quantity
        var productPrice = parseInt($('#hiddenProductPrice').val());
        var productQuantity = $('#numProductQuantity').val();
        // Get extra prices
        var totalExtraPrice = 0;
        var extraPrice, extraQuantity;
        $.each($('.extra-count[data-quantity]'), function (i, e) {
            extraPrice = parseInt($(e).data('price'));
            extraQuantity = parseInt($(e).data('quantity'));
            totalExtraPrice += (extraPrice * extraQuantity);
        });
        var totalPrice = (productPrice + totalExtraPrice) * productQuantity;
        var displayStr = toMoney(totalPrice);
        $('#lblTotalPrice').html(displayStr);
        return totalPrice;
    };

    /*
     * author: TrungNDT
     * method: On General popup, when 
     */
    var selectGeneralItem = function () {
        var selectedItem = $('[name="general-item-control"]:checked');
        // Update price & ID on hidden fields
        $('#hiddenProductId').val(selectedItem.data('id'));
        $('#hiddenProductPrice').val(selectedItem.data('price'));
        // Load extra items
        if (selectedItem.data('hasExtra')) {
            $.ajax({
                url: '/DualScreen/ExtraModal/GetExtraOfGeneral',
                type: 'GET',
                data: { productId: parseInt(selectedItem.data('id')) },
                dataType: 'html',
                success: function (result) {
                    $('#groupExtraOfGeneral').html(result);
                }
            });
        } else {
            $('#groupExtraOfGeneral').empty();
        }
        calculateOrderItemPrice();
    };

    /*
     * author: TrungNDT
     * method: When new order is submitted, add new record to ItemService list
     * param: {JSON} orderItem
     */
    var renderTemporaryOrderItem = function (tempOrderItem, orderId) {
        var $templateOrderParent = $($('#templateMainOrderRow').html().trim()),
            $templateOrderChildren = $($('#templateSubOrderRow').html().trim()),
            $tableItem = $('#table-item-service tbody');

        // Add parent record
        var parentRecord = $templateOrderParent.clone(),
            parentColumn;
        $.each(tempOrderItem, function (i, e) {
            parentColumn = parentRecord.find('[data-role="' + i + '"]');
            if (parentColumn.length > 0) {
                parentColumn.html(e);
            }
        });
        $tableItem.prepend(parentRecord);

        // Add child record
        var currentIndex = parentRecord.index(),
            childRecord;
        if (tempOrderItem.hasOwnProperty('children')) {
            $.each(tempOrderItem['children'], function (i, e) {
                childRecord = $templateOrderChildren.clone();
                $.each(e, function (j, k) {
                    childRecord.find('[data-role="' + j + '"]').html(k);
                });
                $tableItem.children().eq(currentIndex++).after(childRecord);
            });
        }

        // For button
        var $btnEdit = parentRecord.find('[data-action="open-edit-order-modal"]');
        $btnEdit.data('hasExtra', tempOrderItem['hasExtra']);
        $btnEdit.data('type', tempOrderItem['type']);
        $btnEdit.data('orderid', orderId);

        var $btnRemove = parentRecord.find('[data-action="remove-single-order"]');
        $btnRemove.data('id', orderId);

        $('#table-item-service').trigger('service.change');
    };
    //var updateTemporaryOrderItem = function (tempOrderItem, parentRecord) {
    //    parentRecord.find('[data-role="quantity"]').html(tempOrderItem['quantity']);
    //    parentRecord.find('[data-role="totalPrice"]').html(tempOrderItem['totalPrice']);
    //    $('#table-item-service').trigger('service.change');
    //};
    var MAX_VALUE = 500;
    var MIN_VALUE = 1;


    return {
        init: init,
        updateServiceItem: updateServiceItem,
        resetDefaultImage: resetDefaultImage,
        renderServiceItem: renderServiceItem,
        selectGeneralItem: selectGeneralItem,
        calculateOrderItemPrice: calculateOrderItemPrice,
        setupQuantitySpinner: setupQuantitySpinner
    };
}();

/*
 * author: TrungNDT
 * method: Payment
 *          
 */
HMS.Payment = function () {
    var init = function () {
        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Add a payment method row
         */
        $('.modal-order').on('click', '[data-action="add-payment-item"]>li>a', function (e) {
            addNewPaymentItem($(e.currentTarget).data('type'));
            checkPaymentTableDisplaying();
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Active selected row & highlight it
         */
        $('.modal-order').on('focus', '.payment-row [contenteditable="true"]', function (e) {
            $('.payment-row.active').removeClass('active');
            $(e.currentTarget).parent().addClass('active');
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Prevent Enter key
         */
        $('.modal-order').on('keypress', '.payment-row [contenteditable="true"]', function (e) {
            if (e.which == 13) {
                e.preventDefault();
                $(e.currentTarget).blur();
            }
            return;
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Put number value into tender html when focus
         */
        $('.modal-order').on('focus', '.payment-row [data-role="tender"]', function (e) {
            var me = $(e.currentTarget);
            me.html(me.data('value'));
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Validate tender value and run calculating all payment methods
         */
        $('.modal-order').on('focusout', '.payment-row [data-role="tender"]', function (e) {
            var me = $(e.currentTarget),
                htmlValue = me.html();
            if (!isNaN(htmlValue)) {
                me.data('value', parseInt(htmlValue));
                me.html(toMoney(htmlValue));
                calculatePaymentItems();
            } else {
                alert('Tender is not a number');
                me.empty().focus();
            }
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Remove payment method item
         */
        $('.modal-order').on('click', '.payment-row [data-action="remove-payment-item"]', function (e) {
            var $paymentRow = $(e.currentTarget).parents('.payment-row');
            if ($paymentRow.hasClass('saved')) {
                bootbox.confirm('Bạn có muốn xóa thanh toán này?', function (result) {
                    if (result) {
                        $paymentRow.remove();
                    }
                });
            } else {
                $paymentRow.remove();
            }

            if ($paymentRow.data('type') == 'atm') {
                $('#tab-payment [data-type="atm"]').removeAttr('disabled');
            }
            calculatePaymentItems();
            checkPaymentTableDisplaying();
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Payment] Save all payment items (also change frontend status)
         */
        $('.modal-order').on('click', '[data-action="save-all-payment-items"]', function (e) {
            var $paymentRow = $('#tablePayment tbody .payment-row');
            $paymentRow.addClass('saved').filter('.active').removeClass('active');
            $paymentRow.find('[contenteditable=true]').attr('contenteditable', 'false');
        });

        /*
         * author: CuongHH
         * method: [EVENT][Payment] checkout payment items
         */
        $('.modal-order').on('click', '[data-action="payment-items"]', function (e) {

            var table = [];
            $('#tablePayment > tbody > tr').each(function () {
                var rowData = [];
                $(this).children('td').each(function (i, value) {
                    if (i == 0) var el = $(this).data('type');
                    else var el = $(this).text();
                    rowData.push(el);
                });
                table.push(rowData);
            });

            var dt = {
                rentId: $('#rentId').val(),
                total: $('#txtDualTotal').val(),
                discount: $('#txtDualDiscount').val(),
                orderStatus: $('#orderStatus').val(),
                dataPayment: table
            };

            $.ajax({
                url: "/RoomManager/" + $('#hiddenStoreId').val() + "/" + $('#hiddenStoreName').val() + "/Rent/RoomCheckOut",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(dt),
                success: function (data) {
                    if (data.success) window.location.replace("Index");
                }
            });
        });

    };

    /*
     * author: TrungNDT
     * method: [Payment] add new payment item
     * params: {String} type: cash/atm/transfer/membership/voucher
     */
    var addNewPaymentItem = function (type) {
        var $paymentRow = $($('#paymentRow').html().trim()),
            remainingValue = parseInt($('#hiddenRemaining').val()),
            icons = {
                cash: 'money',
                atm: 'credit-card',
                transfer: 'exchange',
                membership: 'users',
                voucher: 'ticket'
            };

        $paymentRow.data('type', type);
        $paymentRow.find('[data-role="icon"] .fa').addClass('fa-' + icons[type]);
        $paymentRow.find('[data-role="icon"]').attr('data-type', type);
        $paymentRow.find('[data-role="due"]').html(toMoney(remainingValue)).data('value', remainingValue);
        $paymentRow.find('[data-role="tender"]').html(toMoney(0)).data('value', 0);
        $paymentRow.find('[data-role="change"]').html(toMoney(0)).data('value', 0);

        switch (type) {
            case 'atm':
                $('#tab-payment [data-type="atm"]').attr('disabled', 'disabled');
                break;
            case 'membership':
                $paymentRow.find('[data-role="description"]').attr('data-ph', 'Nhập mã membership...');
                break;
            case 'voucher':
                //$paymentRow.find('[data-role="tender"]').attr('contenteditable', 'false');
                $paymentRow.find('[data-role="description"]').attr('data-ph', 'Nhập mã voucher...');
                break;
        }
        $('#tablePayment tbody').append($paymentRow);
        $paymentRow.find('[data-role="tender"]').focus();
    };

    /*
     * author: TrungNDT
     * method: [Payment] calculate all payment items
     */
    var calculatePaymentItems = function () {
        var $totalRemaining = $('#hiddenRemaining'),
            totalDiscount = 0;
        $.each($('.payment-row'), function (i, e) {
            var $due = $(e).find('[data-role="due"]'),
                $tender = $(e).find('[data-role="tender"]'),
                $change = $(e).find('[data-role="change"]'),
                nextDueValue,
                initValue;

            // Init value for DUE record
            if ($(e).is(':first-child')) {
                initValue = moneyToNumber($('#txtDualTotal').val());
            } else {
                initValue = parseInt($totalRemaining.val());
            }
            $due
                .html(toMoney(initValue))
                .data('value', initValue);

            var intDue = parseInt($due.data('value')),
                intTender = parseInt($tender.data('value'));

            // Calculate CHANGE
            var changeValue, previousChange = 0;
            if (intDue < intTender) {
                changeValue = intTender - intDue;
                nextDueValue = 0;
            } else {
                changeValue = 0;
                nextDueValue = intDue - intTender;
            }
            if (!$(e).is(':first-child')) {
                previousChange = parseInt($(e).prev().find('[data-role="change"]').data('value'));
            }
            $change
                .html(toMoney(changeValue + previousChange))
                .data('value', changeValue + previousChange);

            // Record remaining value
            $totalRemaining.val(nextDueValue);

            // **Sum total paying and total discount
            if ($(e).data('type') == 'voucher') {
                totalDiscount += intTender;
            }
        });
        $('#totalPaymentCost').html(toMoney($('#hiddenRemaining').val()));

        // Display total discount and total paying into left form
        $('#txtDualDiscount').val(toMoney(totalDiscount));
        $('#txtDualPayment').val(toMoney(moneyToNumber($('#txtDualTotal').val()) - totalDiscount));
    };

    /*
     * author: TrungNDT
     * method: [Payment] If there is no payment item in payment table, table will be hidden.
     *                  And vice versa
     */
    var checkPaymentTableDisplaying = function () {
        var $table = $('#tablePayment'),
            $panel = $('#pnlTablePayment'),
            $msg = $('#msgTablePayment');
        //console.log('checkPaymentTableDisplaying');
        // Case 1: nothing exist
        if ($table.find('.payment-row').length == 0) {
            //console.log('nothing exist');
            $panel.hide();
            $msg.show();
        }
        else { // Case 2: payment row found
            $panel.show();
            $msg.hide();
        }
    }

    return {
        init: init,
        checkPaymentTableDisplaying: checkPaymentTableDisplaying
    };
}();

HMS.RentFeeSection = function () {
    var fee = 0, rentId, storeId, storeName;

    var init = function () {
        $(document).ready(function () {
            /*
             * author: TrungNDT
             * method: [Rent] Submit base start & end date, init first blank stacked item
             */
            $('.modal-order').on('click', '[data-action="send-rent-time"]', function () {
                $('#stackedContainer').data('StackedList', []);
                renderStackedTime();
            });

            /*
             * author: TrungNDT
             * method: [Rent] Open add/edit stacked item based on type (blank or not)
             */
            $('.modal-order').on('click', '.stacked-time', function (e) {
                var me = $(e.currentTarget),
                    $modal = $('#modalStackedTimeDetails'),
                    rentId = $('#rentID').val(),
                    orderId = $('#hiddenOrderId').val(),
                    startStackedDate = me.attr('data-start'),
                    endStackedDate = me.attr('data-end'),
                    data;
                if (me.hasClass('blank')) {
                    data = {
                        url: 'AddMoneyRoom',
                        param: { rentId: rentId },
                        title: 'Thêm thời gian thuê'
                    };
                } else {
                    data = {
                        url: 'UpdateMoneyRoom',
                        param: { orderId: orderId },
                        title: 'Chỉnh sửa thời gian thuê'
                    };
                }
                $.ajax({
                    url: '/RoomManager/Service/' + data['url'],
                    //type: 'GET',
                    dataType: 'HTML',
                    data: data['param'],
                    success: function (result) {
                        $modal.html(result);
                        $modal.find('.modal-title').html(data['title']);
                        $modal.find('[name="RentId"]').val($('#rentId').val());
                        setupStackedDateTimePicker(startStackedDate, endStackedDate);
                        $modal.modal('show');
                    }
                });

            });

            /*
             * author: TrungNDT
             * method: [Rent] 
             */
            $('.modal-order').on('dp.change', '#startBaseDate, #endBaseDate', function (e) {

            });

            /*
             * author: TrungNDT
             * method: [Rent] Add/edit stacked item based on their type (blank or not)
             */
            $('#modalStackedTimeDetails').on('click', '[data-type="submit"]', function (e) {
                var me = $(e.currentTarget),
                    mode = me.attr('data-mode');
                $('#form' + mode + 'MoneyRoom').submit();
                $('#modalStackedTimeDetails').modal('hide');
                //    $container = $('#stackedContainer'),
                //    startStackedDate = $('#startStackedDate input').val(),
                //    endStackedDate = $('#endStackedDate input').val(),
                //    //stackedList = JSON.parse($container.attr('data-stackedList'));
                //    stackedList = $container.data('StackedList') || [];

                //stackedList.push({
                //    _start: startStackedDate,
                //    end: endStackedDate
                //});

                ////$container.attr('data-stackedList', JSON.stringify(stackedList));
                //$container.data('StackedList', stackedList);
                //renderStackedTime();
                //$('#modalStackedTimeDetails').modal('hide');
                //return true;
            });

            /*
             * author: TrungNDT
             * method: [Rent] 
             */
            $('.modal-order').on('click', '[data-action="approve-fee"]', function () {
                approveFeeDetail();
            });

            $('.modal-order').on('keyup', 'fee-input', function () {
                updateFeeTotal();
            });

            $('.modal-order').on('click', '[href="#tab-payment"]', function () {
                $('#tabHandler .active').removeClass('active');
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('.modal-order').on('click', '[data-role="toggle-editable"]', function (e) {
                var $selectedRow = $(this).closest('[data-role="fee-row"]'),
                    mode = $(this).attr('data-type');
                toggleEditableMode($selectedRow, (mode == 'enable'));
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('.modal-order').on('change', '[data-role="category"]', function (e) {
                var value = $(this).val();
                var $selectedRow = $(this).closest('[data-role="fee-row"]');
                var orderFeeId = $selectedRow.attr('data-orderfeeid');
                var priceGroup = $selectedRow.attr('data-pricegroup');
                var rentType = $selectedRow.attr('data-rentType');
                var hour = $selectedRow.attr('data-hours');
                var cal = $selectedRow.find('[data-role="calculated-price"]');
                var cus = $selectedRow.find('[data-role="custom-price"]');
                $.ajax({
                    url: '/RoomManager/Rent/ChangePriceMode',
                    type: 'GET',
                    dataType: 'JSON',
                    data: {
                        orderFeeId: orderFeeId,
                        hour: hour,
                        rentType: value,
                        priceGroupId: priceGroup
                    },
                    success: function (result) {
                        $selectedRow.attr('data-pricegroup', result.priceGroupId);
                        $selectedRow.attr('data-rentType', result.rentMode);
                        $selectedRow.attr('data-priceCal', result.fee);
                        $selectedRow.attr('data-priceEt', result.fee);
                        cal.html(result.fee);
                        cus.html(result.fee);
                        cus.val(result.fee);
                    }
                });

            });
            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('.modal-order').on('change', '[data-role="level"]', function (e) {
                var value = $(this).val();
                var $selectedRow = $(this).closest('[data-role="fee-row"]');
                var orderFeeId = $selectedRow.attr('data-orderfeeid');
                var priceGroup = $selectedRow.attr('data-pricegroup');
                var rentType = $selectedRow.attr('data-rentType');
                var hour = $selectedRow.attr('data-hours');
                var cal = $selectedRow.find('[data-role="calculated-price"]');
                var cus = $selectedRow.find('[data-role="custom-price"]');
                $.ajax({
                    url: '/RoomManager/Rent/ChangePriceMode',
                    type: 'GET',
                    dataType: 'JSON',
                    data: {
                        orderFeeId: orderFeeId,
                        hour: hour,
                        rentType: rentType,
                        priceGroupId: value
                    },
                    success: function (result) {
                        $selectedRow.attr('data-pricegroup', result.priceGroupId);
                        $selectedRow.attr('data-rentType', result.rentMode);
                        $selectedRow.attr('data-priceCal', result.fee);
                        $selectedRow.attr('data-priceEt', result.fee);
                        cal.html(result.fee);
                        cus.html(result.fee);
                        cus.val(result.fee);
                    }
                });
            });
        });
    };

    //Sinh Change Price Room
    var changeRentMode = function (orderFeeId, hour, rentType, priceGroupId) {
        $.ajax({
            url: '/RoomManager/Rent/ChangePriceMode',
            type: 'GET',
            dataType: 'JSON',
            data: { rentId: rentId },
            success: function (result) {

            }
        });
    }



    /*
     * author: TrungNDT
     * method: [Rent] setup datetimepicker for start/end BASE date 
     */
    var setupBaseDateTimePicker = function () {
        var $start = $('#startBaseDate'),
            $end = $('#endBaseDate');

        $('#startBaseDate, #endBaseDate').datetimepicker({
            format: 'YYYY/MM/DD HH:mm'
        });
        $start.on('dp.change', function (e) {
            $end.data('DateTimePicker').minDate(e.date);
            loadStackedTimeline();
        });
        $end.on('dp.change', function (e) {
            $start.data('DateTimePicker').maxDate(e.date);
            loadStackedTimeline();
        });
    };

    /*
     * author: TrungNDT
     * method: [Rent] set value for start/end BASE date.
     *          Also set min/max constraint for them.
     * params: {String} start: start date (YYYY/MM/DD HH:mm)
     * params: {String} end: end date (YYYY/MM/DD HH:mm)
     */
    var setValueForBaseDateTimePicker = function (start, end) {
        $('#startBaseDate').data('DateTimePicker').date(start).maxDate(start);
        $('#endBaseDate').data('DateTimePicker').date(end).minDate(end);
    };

    /*
     * author: TrungNDT
     * method: [Rent] setup datetimepicker & value for start/end STACKED date 
     *          Also set min/max constraint for them
     * params: {String} start: start date (YYYY/MM/DD HH:mm)
     * params: {String} end: end date (YYYY/MM/DD HH:mm)
     */
    var setupStackedDateTimePicker = function (start, end) {
        $('#startStackedDate, #endStackedDate').datetimepicker({
            format: 'YYYY/MM/DD HH:mm',
            minDate: start,
            maxDate: end
        });
        $('#startStackedDate').data('DateTimePicker').date(start);
        $('#endStackedDate').data('DateTimePicker').date(end);

        //$('#startStackedDate').data('DateTimePicker').setDate(start);
        //$('#endStackedDate').data('DateTimePicker').setDate(end);
        //$('#startStackedDate, #endStackedDate').data('DateTimePicker').setMinDate(start);
        //$('#startStackedDate, #endStackedDate').data('DateTimePicker').setMaxDate(end);
    };

    /*
     * author: TrungNDT
     * method: [Rent][INIT] Load all room fee from DB
     */
    var loadStackedTimeline = function () {
        var $container = $('#stackedContainer'),
            rentId = $('#rentId').val();
        if (rentId) {
            $.ajax({
                url: '/RoomManager/Service/LoadRoomFee',
                //type: 'GET',
                //dataType: 'JSON',
                data: { rentId: rentId },
                success: function (result) {
                    var data = [];
                    $.each(result['orderFee'], function (i, e) {
                        data.push({
                            _start: e['FromDate'],
                            end: e['ToDate']
                        });
                    });
                    $container.data('StackedList', data);
                    $('#hiddenOrderId').val(result['orderFee'][0]['OrderId']);
                    setValueForBaseDateTimePicker(result['startDate'], result['endDate']);
                    renderStackedTime();
                }
            });
        } else {
            alert('Rentid not found!');
            return;
        }
    };

    /*
     * author: TrungNDT
     * method: [Rent] Render stacked time items into progress bar (as HTML)
     */
    var renderStackedTime = function () {
        var startBaseDate = $('#startBaseDate input').val(),
            endBaseDate = $('#endBaseDate input').val(),
            stackedList = $('#stackedContainer').data('StackedList') || [];
        var data = stackedList.slice(0);
        // Add very start/end point
        data.push({
            _start: '1000/01/01',
            end: startBaseDate
        });
        data.push({
            _start: endBaseDate,
            end: '9999/01/01'
        });

        // Sort by start time
        data.sort(function (a, b) {
            return new Date(a['_start']) - new Date(b['_start']);
        });

        // Checkoverlap
        var blankTime;
        for (var i = 0, l = data.length; i < l - 1; i++) {
            var currEnd = data[i]['end'],
                nextStart = data[i + 1]['_start'];
            if ((new Date(currEnd)) >= (new Date(nextStart))) {
                data[i + 1]['_start'] = currEnd;
            } else {
                blankTime = {
                    isBlank: true,
                    _start: currEnd,
                    end: nextStart
                };
                data.push(blankTime);
            }
        }

        // Sort again
        data.sort(function (a, b) {
            return new Date(a['_start']) - new Date(b['_start']);
        });

        var getItem = function (length, data) {
            return $('<div/>', {
                'class': 'stacked-time progress-bar ' + (data[i]['isBlank'] ? 'blank' : 'tooltip-success stacked'),
                'style': 'width: ' + length + '%',
                'data-start': data[i]['_start'],
                'data-end': data[i]['end'],
                'data-toggle': 'tooltip',
                'data-placement': 'bottom',
                'title': data[i]['isBlank'] ? 'Thời gian trống' : (data[i]['_start'] + ' - ' + data[i]['end'])
            });
        }; // Throw to progress bar
        var total = (new Date(endBaseDate) - new Date(startBaseDate)),
            value,
            $bar = $('.stacked-container').empty();

        for (var i = 1, l = data.length; i < l - 1; i++) {
            value = (new Date(data[i]['end']) - new Date(data[i]['_start'])) / total * 100;
            $bar.append(getItem(value, data));
        }
        $('.stacked-time').tooltip();
    };

    /*
     * author: TrungNDT
     * method: When new order is submitted, add new record to ItemService list
     * param: {JSON} orderItem
     */
    var renderTemporaryOrderItem = function (tempOrderItem, orderId) {
        var $templateOrderParent = $($('#templateMainOrderRow').html().trim()),
            $tableItem = $('#table-item-service tbody');

        // Add parent record
        var parentRecord = $templateOrderParent.clone(),
            parentColumn;
        $.each(tempOrderItem, function (i, e) {
            parentColumn = parentRecord.find('[data-role="' + i + '"]');
            if (parentColumn.length > 0) {
                parentColumn.html(e);
            }
        });
        $tableItem.prepend(parentRecord);

        // For button
        var $btnEdit = parentRecord.find('[data-action="open-edit-order-modal"]');
        $btnEdit.data('hasExtra', tempOrderItem['hasExtra']);
        $btnEdit.data('type', tempOrderItem['type']);
        $btnEdit.data('orderId', orderId);
        $('#table-item-service').trigger('service.change');
    };

    /*
     * author: TrungNDT
     * method: 
     */
    var getGeneralValues = function () {
        rentId = parseInt($('#rentId').val());
        storeId = $('#hiddenStoreId').val(),
        storeName = $('#hiddenStoreName').val();
    }

    var toggleEditableMode = function ($selectedRow, isEnable) {
        var orderFeeId = $selectedRow.attr('data-orderfeeid');
        var priceGroup = $selectedRow.attr('data-pricegroup');
        var rentType = $selectedRow.attr('data-rentType');
        var hour = $selectedRow.attr('data-hours');
        var cal = $selectedRow.find('[data-role="calculated-price"]');
        var cus = $selectedRow.find('[data-role="custom-price"]');

        var $btnApproveFee = $('#btnApproveFee'),
            $btnEnable = $selectedRow.find('[data-type="enable"]'),
            $btnSaving = $selectedRow.find('[data-type="saving"]'),
            $input = $selectedRow.find('.form-control'),

            $inputCategories = $selectedRow.find('[data-role="category"]'),
            $inputPriceLevel = $selectedRow.find('[data-role="level"]'),
            $inputCalculatedPrice = $selectedRow.find('[data-role="calculated-price"]'),
            $inputCustomPrice = $selectedRow.find('[data-role="custom-price"]');

        // Case 1: Enable editable mode
        if (isEnable) {
            // Check displaying: show/hide, enable/disable
            $btnEnable.hide();
            $btnSaving.show();
            $input.removeProp('disabled');
            $btnApproveFee.prop('disabled', 'disabled');
        }
            // Case 2: Done editing and save
        else {
            // Check displaying: show/hide, enable/disable
            $btnEnable.show();
            $btnSaving.hide();
            $input.prop('disabled', 'disabled');
            $btnApproveFee.removeProp('disabled');

            // Get prices & collect data
            var calculatedPrice = parseInt($inputCalculatedPrice.val()),
                customPrice = parseInt($inputCustomPrice.val()),
                finalPrice = customPrice == 0 ? calculatedPrice : customPrice;
            var feeData = {
                type: $inputCategories.val(),
                level: $inputCategories.val(),
                price: finalPrice
            }
            $.ajax({
                url: '/RoomManager/Rent/SaveOrderFeeItem',
                type: 'GET',
                dataType: 'JSON',
                data: {
                    orderFeeId: orderFeeId,
                    rentType: rentType,
                    priceGroupId: priceGroup,
                    fee: customPrice
                },
                success: function (result) {
                    $selectedRow.attr('data-pricegroup', result.priceGroupId);
                    $selectedRow.attr('data-rentType', result.rentMode);
                    $selectedRow.attr('data-priceCal', result.fee);
                    $selectedRow.attr('data-priceEt', result.fee);
                    cal.html(result.fee);
                    cus.html(result.fee);
                    cus.val(result.fee);
                }
            });
        }
    }

    //function loadFeeDetail() {
    //    var count = 1;
    //    fee = 0;
    //    window.dt = $('#fee-detail-table').DataTable({
    //        'ajax': {
    //            'url': '/RoomManager/' + storeId + '/' + storeName + '/Rent/GetFeeDetail',
    //            'data': { 'rentId': rentId }
    //        },
    //        'paging': false,
    //        'bSort': false,
    //        'bFilter': false,
    //        'columns': [
    //            {
    //                'data': function () {
    //                    return count++;
    //                }
    //            },
    //            {
    //                'data': function (data) {
    //                    if (data.IsEarlyPrice) {
    //                        return 'Sớm giờ';
    //                    }
    //                    if (data.IsLatePrice) {
    //                        return 'Trễ giờ';
    //                    }
    //                    if (data.IsDaysPrice) {
    //                        return 'Ngày';
    //                    }
    //                    if (data.IsNightPrice) {
    //                        return 'Đêm';
    //                    }
    //                    if (data.IsFirstHourPrice) {
    //                        return 'Giờ đầu';
    //                    }
    //                    if (data.IsSecondHourPrice) {
    //                        return 'Giờ thứ 2';
    //                    }
    //                    if (data.IsThirdHourPrice) {
    //                        return 'Giờ thứ 3';
    //                    }
    //                    if (data.IsHourPrice) {
    //                        return 'Giờ tiếp theo';
    //                    }
    //                }
    //            },
    //            {
    //                'data': 'StartTime'
    //            },
    //            {
    //                'data': 'EndTime'
    //            },
    //            {
    //                'data': 'Fee'
    //            },
    //            {
    //                'data': function (data) {
    //                    fee = fee + data.Fee;
    //                    $('#sumFee').html(fee);
    //                    return '<input type="text" class="form-control fee-input" style="width:100px" value="' + data.Fee + '"/>';
    //                }
    //            }
    //        ],
    //        'oLanguage': {
    //            'sInfo': '',
    //            'sZeroRecords': 'Không có dữ liệu phù hợp',
    //            'sEmptyTable': 'Không có dữ liệu',
    //            'sInfoFiltered': ' - lọc ra từ _MAX_ dòng',
    //            'sLengthMenu': 'Hiển thị _MENU_ dòng',
    //            'sProcessing': 'Đang xử lý...'
    //        },
    //        'bAutoWidth': false
    //    });
    //    setTimeout(function () { approveFeeDetail(); }, 1000);
    //}

    function loadFeeDetail() {
        $.ajax({
            url: '/RoomManager/' + storeId + '/' + storeName + '/Rent/GetFeeDetail',
            type: 'GET',
            dataType: 'html',
            data: { 'rentId': rentId },
            cache: true,
            success: function (data) {
                $('#loadFee-detail-table').html(data);
                var table = [];
                var total = 0;
                $('#fee-detail-table > tbody > tr').each(function () {
                    var rowData = [];
                    $(this).children('td').each(function (i, value) {
                        if (i == 6)
                        {
                            var el = parseInt($(this).find('input').val());
                            total += el;
                        }
                        else
                            var el = $(this).text();
                        rowData.push(el);
                    });
                    table.push(rowData);
                });
                //console.log('#txtRoomAmount ' + toMoney(total));
                $('#txtRoomAmount').val(toMoney(total));
            }
        });
    }

    function updateFeeTotal() {
        var feeTotal = 0;
        $.each($('.fee-input'), function (i, e) {
            var tmp = $(e).val();
            if (tmp == '') {
                tmp = 0;
                $(e).val(0);
            }
            feeTotal = feeTotal + parseInt(tmp);
            $(e).parent().prev().html(toMoney(tmp));
        });
        $('#sumFee').html(feeTotal);
    }

    function approveFeeDetail() {
        //  var storeId = $('#hiddenStoreId').val();
        // var storeName = $('#hiddenStoreName').val();
        var table = [];
        $('#fee-detail-table > tbody > tr').each(function () {
            var rowData = [];
            $(this).children('td').each(function (i, value) {
                if (i == 6) var el = $(this).find('input').val();
                else
                    var el = $(this).text();
                rowData.push(el);
            });
            table.push(rowData);
        });

        var dataToExport = {
            feeDetail: table,
            rentId: rentId
        };

        $.ajax({
            url: '/RoomManager/' + storeId + '/' + storeName + '/Rent/UpdateFeeDetail',
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(dataToExport),
            cache: true,
            success: function (data) {
                if (data.success) {
                    updateFeeTotal();
                    $('#txtRoomAmount').val(toMoney(data.price));
                    $('#table-item-service').trigger('service.change');
                }
            }
        });
    }

    function renderItem(length, data) {
        return $('<div/>', {
            'class': 'stacked-time progress-bar ' + (data.Hours < 0 ? 'tooltip-error' : 'tooltip-success stacked'),
            'style': 'width: ' + length + '%',
            'data-start': data.StartTime,
            'data-end': data.EndTime,
            'data-toggle': 'tooltip',
            'data-placement': 'bottom',
            'title': data.StartTime + ' - ' + data.EndTime + ' : ' + data.Fee + ' VNĐ'
        });
    }

    //Sinh comment
    //function renderProcessBar() {
    //    var bar = $('.stacked-container').empty();
    //    $.ajax({
    //        url: '/RoomManager/' + storeId + '/' + storeName + '/Rent/GetFeeDetail',
    //        type: 'POST',
    //        data: { rentId: rentId },
    //        success: function (data) {
    //            for (var i = 0; i < data.data.length; i++) {
    //                //console.log('for loop');
    //                var length;
    //                if (data.data[i].Hours < 0) length = data.data[i].Hours * -1 / data.data[i].TotalHours * 100;
    //                else length = data.data[i].Hours / data.data[i].TotalHours * 100;
    //                //console.log(length);
    //                bar.append(renderItem(length, data.data[i]));
    //            }
    //        }
    //    });
    //}

    var totalOrderDetail = function (rentId) {
        $.ajax({
            url: "/RentManager/Rent/GetTotalOrderDetail",
            type: "GET",
            data: { rentId: rentId },
            cache: true,
            async: true,
            success: function (result) {
                $("#txtDualTotal").val(result.TotalPrice);
            }
        });
    }

    return {
        init: init,
        getGeneralValues: getGeneralValues,
        loadFeeDetail: loadFeeDetail,
        //renderProcessBar: renderProcessBar,
        approveFeeDetail: approveFeeDetail
    };
}();