var HMS = window.HMS || {};

// Module for General
HMS.General = function () {
    var init = function () {
        $(document).ready(function () {
            //loadStoreList();//--PhuongLHK: change layout . Do not need load all store at begining
            bindingAjaxLoader();
            //renderStoreNavigation();
            setSystemDislaying();
            avoidNullUrl();

            /*
             * author: TrungNDT
             * method: [EVENT] Active when change storeId in StoreDropdown, 
             *          save selected storeId into Session[storeId]
             */
            $(document).on('change', '.transparent-select', function (e) {
                var list = $(e.currentTarget);
                $.ajax({
                    'type': 'POST',
                    'url': '/Home/SelectStore',
                    'data': { 'storeId': list.val() },
                    'dataType': 'json',
                    success: function (result) {
                        window.location.href = '/' + result.storeId + '/' + result.storeName + '/Home/Index';
                        //window.location.href = JSON.parse(result)['Url'];
                    }
                });
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Active/Deactivate Shortcut btn
             */
            $(document).on('click', '.sidebar-shortcuts-large .btn', function (e) {
                var btn = $(e.currentTarget);
                var currBtn = $('.sidebar-shortcuts-large .btn.active');
                if (btn != currBtn) {
                    currBtn.removeClass('active');
                    btn.addClass('active');
                }

            });

            /*
             * author: TrungNDT
             * method: [EVENT] Save selected menu item to session
             */
            $(document).on('click', '.side-menu .panel-collapse .nav > li', function (e) {
                //sessionStorage.setItem('currentTab', $('.sidebar-shortcuts-large .btn.active').attr('id'));
                sessionStorage.setItem('currentMenu', $(e.currentTarget).attr('id'));
            });

            /*
             * author: TrungNDT
             * method: [EVENT] When loading gif is on screen, press ESC to turn it off
             */
            $(document).on('keydown', function (e) {
                var $loader = $('.loading-gif');
                if (e.which == 27 && $loader.is(':visible'))
                    $loader.fadeOut();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Expand/collapse sidebar
             */
            $('.sidebar-toggle').click(function () {
                $('#sidebar').toggleClass('sidebar-collapse');
                $('.main-content').toggleClass('expand');
                $(this).find('.fa').toggleClass('fa-angle-right');
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $('.sidebar .panel-group').on('show.bs.collapse', function () {
                var $currPanel = $('.sidebar .panel-collapse.in');
                $currPanel.collapse('hide');
                $('[href="#' + $currPanel.attr('id') + '"]').addClass('collapsed');
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Activate nicescroll when sidebar is collapse in
             */
            $('.sidebar .panel-group').on('shown.bs.collapse', function () {
                //$('#sidebar').niceScroll();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $(document).on('show.bs.collapse', '#navigationPanel', function () {
                if ($('#navigationPanel .card').is(':empty')) {
                    renderStoreNavigation();
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] When StoreNav is shown: activate nicescroll & disabled body scroll
             */
            $(document).on('shown.bs.collapse', '#navigationPanel', function () {
                $('[href="#navigationPanel"] .fa').addClass('fa-caret-up');
                $(this).niceScroll({
                    cursoropacitymax: .5
                });
                $('body').addClass('modal-open');
            });

            /*
             * author: TrungNDT
             * method: [EVENT] When StoreNav is hidden: enable body scroll
             */
            $(document).on('hidden.bs.collapse', '#navigationPanel', function () {
                $('body').removeClass('modal-open');
                $('[href="#navigationPanel"] .fa').removeClass('fa-caret-up');
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Click outside navigationPanel to close it
             */
            //$(document).on('click', ':not(#navigationPanel)', function () {
            //    var $nav = $('#navigationPanel');
            //    if (!$(event.target).closest('#navigationPanel').length) {
            //        if ($nav.is(":visible")) {
            //            $nav.collapse('hide');
            //        }
            //    }
            //});

            /*
             * author: TrungNDT
             * method: [EVENT][TABLET] Click inside sidebar to open it
             */
            $(document).on('click', '#sidebar.sidebar-collapse', function () {
                if ($(window).width() < 1224) {
                    $(this).removeClass('sidebar-collapse').addClass('sidebar-expand');
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT][TABLET] Click outside sidebar to close it
             */
            //$(document).on('click', ':not(#sidebar)', function () {
            //    if (!$(event.target).closest('#sidebar').length && $(window).width() < 1224) {
            //        $('#sidebar').removeClass('sidebar-expand').addClass('sidebar-collapse');
            //    }
            //});

        });

        $(window).load(function () {
            setSelectedMenu();
        });
    }

    /*
     * author: TrungNDT
     * method: [INIT] Auto show loading-gif when ajax function is called
     */
    var bindingAjaxLoader = function () {
        $(document).bind('ajaxStart', function () {
            $('.loading-gif').fadeIn();
        }).bind('ajaxStop', function () {
            $('.loading-gif').fadeOut();
        });
    }

    /*
     * author: TrungNDT
     * method: [INIT] Load storeId list and append to top menu
     */
    var loadStoreList = function () {

        var url = window.location.href;
        $.ajax({
            'type': 'GET',
            'url': '/Home/LoadStoreList',
            'data': { 'url': url },
            'dataType': 'html',
            success: function (result) {
                $('#storeListContainer').append(result);
                var id = $('#hiddenStoreId').val();
                $('.selectpicker').val(id);

                selectCurrentStore();
                $('.selectpicker').selectpicker('val', id);
            }
        });
    }

    /*
     * author: TrungNDT
     * method: [INIT] Set selected for current Store (from Session)
     */
    var selectCurrentStore = function () {
        var currStore = $('#hiddenCurrentStore').val();
        if (currStore != undefined) {
            $('.selectpicker').selectpicker('val', currStore);
        }
    }

    /*
     * author: TrungNDT
     * method: 
     */
    var setSelectedMenu = function () {
        var pathname = window.location.pathname.replace('%20', ' ');

        if (pathname != '/') {
            var menu = $('.side-menu [href="' + pathname + '"]');
            if (menu.length > 0) {
                menu = $(menu[0]);
                //var tabContent = menu.parent().parent().parent().parent();
                //var tabHeader = $('.sidebar-shortcuts [href="#' + tabContent.attr('id') + '"]');
                var $tabContent = menu.parents('.panel-collapse.collapse');

                // Deactivate old tab & menu
                //$('.sidebar-shortcuts-large .btn').removeClass('active');
                //$('.side-menu .tab-pane').removeClass('in active');
                //$('.side-menu .panel-collapse .nav > li').removeClass('active');

                // Active new tab & menu
                //tabHeader.addClass('active');
                //tabContent.addClass('in active');

                menu.parent().addClass('active');
                $tabContent.removeClass('collapse').addClass('in');
                $tabContent.prev().children().addClass('active');
                $('[href="#' + $tabContent.attr('id') + '"]').removeClass('collapsed');
            }
        }
    }

    /*
     * author: TrungNDT
     * method: Apply expanding mode fore some specific pages: 
     *          Sidebar & store list will be hidden
     */
    var activateExpandingMode = function () {
        $('#sidebar').addClass('collapse');
        //$('#storeListContainer').hide();
    }

    /*
     * author: TrungNDT
     * method: [INIT] Render store list navigation (in top nav)
     */
    var renderStoreNavigation = function () {
        $.ajax({
            url: '/Home/RenderStoreNavigation',
            dataType: 'html',
            success: function (result) {
                $('#navigationPanel .card').append(result);
                //$('[href="#navigationPanel"]').show();
            }
        });
    }

    /*
     * author: TrungNDT
     * method: [INIT] This function do following things: 
     *                 - Display current store name
     *                 - Check displaying status of NavButton (Store / Manage)
     *                 - Check displaying status of siderbar
     */
    var setSystemDislaying = function () {
        var storeId = $('#hiddenStoreId').val(),
            storeName = $('#hiddenStoreName').val();

        // Display current store name
        $('#txtCurrentStore').html(storeId == 0
            ? 'Tổng quan hệ thống'
            : 'Cửa hàng <strong>' + storeName + '</strong>');

        // Check displaying status of NavButton (Store / Manage)
        var $btnStore = $('#btnNavStore'),
            $btnExit = $('#btnNavExit'),
            $btnNavigation = $('[href="#navigationPanel"]'),
            url = window.location.pathname,
            isStorePage = (url.split('/').indexOf('RoomManager') > 0);
        $btnStore.css('display', isStorePage ? 'none' : 'block');
        $btnExit.css('display', isStorePage ? 'block' : 'none');
        $btnNavigation.css('display', isStorePage ? 'none' : 'block');

        // Check displaying status of siderbar:
        // If device is tablet (< 1224px), 'siderbar-collapse' class will be added to #siderbar
        if ($(window).width() < 1224) {
            $('#sidebar').addClass('sidebar-collapse');
        }
    }

    /*
     * author: TrungNDT
     * method: [INIT] 
     */
    var avoidNullUrl = function () {
        var storeId = $('#hiddenStoreId').val(),
            storeName = $('#hiddenStoreName').val();
    }

    /*
     * author: TrungNDT
     * method: Return default options for dataTables
     */
    var getDataTableDefaultOptions = function () {
        return {
            "dom": '<"top"f>rt<"bottom"ilp><"clear">',
            "oLanguage": {
                "sSearch": "Tên Danh mục:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý..."
            }
        }
    }

    /*
     * author: TrungNDT
     * method: Refresh given dataTable
     * params: {String} tableId
     */
    var refreshTable = function (tableId) {
        var options = getDataTableDefaultOptions(),
            oTable = $(tableId).dataTable(options);
        //oTable._fnPageChange(0);
        //oTable._fnAjaxUpdate();
    }

    /*
     * author: TrungNDT
     * method: Trigger event (often "Search") when a routine of keys is presses
     */
    var inputDelay = (function () {
        var timer = 0;
        return function (callback) {
            clearTimeout(timer);
            timer = setTimeout(callback, 500);
        };
    })();

    return {
        init: init,
        activateExpandingMode: activateExpandingMode,
        getDataTableDefaultOptions: getDataTableDefaultOptions,
        refreshTable: refreshTable,
        inputDelay: inputDelay
    }
}();

HMS.bookingForm = function () {
    $(document).ready(function () {
        $(document).on('click', '[data-role="new-customer"]', function () {
            var inputSearch = $('#product-item-list'),
                inputName = $('#cust-name'),
                inputAddress = $('#cust-address'),
                inputPhone = $('#cust-phone'),
                inputEmail = $('#cust-email'),
                me = $(this);

            if (me.hasClass('active')) {
                inputSearch.attr('disabled', 'disabled');
                inputName.removeAttr('disabled');
                inputAddress.removeAttr('disabled');
                inputPhone.removeAttr('disabled');
                inputEmail.removeAttr('disabled');
                me.removeClass('btn-success')
                    .addClass('btn-primary')
                    .html('<i class="left-icon fa fa-user"></i>KH Mới');
            } else {
                inputSearch.removeAttr('disabled');
                inputName.attr('disabled', 'disabled');
                inputAddress.attr('disabled', 'disabled');
                inputPhone.attr('disabled', 'disabled');
                inputEmail.attr('disabled', 'disabled');
                me.removeClass('btn-primary')
                    .addClass('btn-success')
                    .html('<i class="left-icon fa fa-plus"></i>Thêm KH')
            }
        });
    });
};

HMS.booking = function () {
    $(document).ready(function () {
        /*
         * author: TrungNDT
         * method: [EVENT] Toggle assigning guest list base on which room is selected,
         *          in order to assign guest into rooms
         */
        $(document).on('click', '[data-role="assignGuest"]', function () {
            var me = $(this);
            var guestList = $('#pnlGuestList');
            var tableAssignGuest = $('#tblAssignGuest');

            // Get current active button
            var currentActibeBtn = tableAssignGuest.find('[data-role="assignGuest"].active');
            if (me.attr('data-target') != currentActibeBtn.attr('data-target')) {
                currentActibeBtn.removeClass('active');
                me.addClass('active');
                guestList.fadeOut(50, function () {
                    showGuestList();
                });
            } else {
                if (me.hasClass('active')) {
                    me.removeClass('active');
                    guestList.fadeOut();
                }
                else {
                    me.addClass('active');
                    showGuestList()
                }
            }

            // Get active row and show GuestList at that row
            function showGuestList() {
                var selectedRow = $($('[data-role="assignGuest"].active').attr('data-target'));
                selectedRow.find('.guest-container').append(guestList);
                guestList.fadeIn();
            }
        });

        /*
         * author: TrungNDT
         * method: [EVENT] When Assign Guest Panel is shown, click on each guest name
         *                  to add that guest into selected row
         */
        $(document).on('click', '.guest-item', function () {
            var selectedRow = $($('[data-role="assignGuest"].active').attr('data-target'));
            selectedRow.find('.drag-zone').append($(this));
        });

        /*
         * author: TrungNDT
         * method: [EVENT] Filter guest by name 
         */
        $(document).on('keyup', '#txtFilterGuest', function () {
            var text = $(this).val();
            $('.guest-list .guest-item span:not(:Contains(' + text + '))').hide();
            $('.guest-list .guest-item span:Contains(' + text + ')').show();
        });

    });
};

/*
* author: SinhCV
* method: Rent/RentDetail
*          
*/
HMS.rentDetail = function () {
    var init = function () {
        $(document).ready(function () {

            $(document).keydown(function (e) {
                switch (e.keyCode) {
                    case 112: //f1
                        event.preventDefault();
                        updateRentInfo();
                        break;
                    case 113: //f2 - Thanh toán
                        CheckOut(true);
                        break;
                }
                $("#InvoiceID").change(function () {
                    $(this).closest('.form-group').addClass("has-success");
                });
                $("#BikeID").change(function () {
                    $(this).closest('.form-group').addClass("has-success");
                });
                //$("#CheckInDate").change(function () {
                //    $(this).closest('.form-group').addClass("has-success");
                //});

                //$("#CheckOutDate").change(function () {
                //    $(this).closest('.form-group').addClass("has-success");
                //});
                $("#PriceGroupID").change(function () {
                    $(this).closest('.form-group').addClass("has-success");
                });
                $("#Notes").change(function () {
                    $(this).closest('.form-group').addClass("has-success");
                });
            });

            $("#isFixedPrice").click(function () {
                var isFixedPrice = $(this).is(':checked');
                if (isFixedPrice) {
                    $("#rentFee").prop('readonly', false);
                } else {
                    $("#rentFee").prop('readonly', true);
                    var rentType = $("#hdfRentType").val();
                    updateRentFee(rentType);
                }

            });

            $("#rentFee").change(function () {
                updateEntireFee();
            });

            $('.priceField').each(function () {
                $(this).qtip({
                    content: {
                        text: function (event, api) {
                            $.ajax({
                                url: "/RoomManager/Room/GetPriceToolTip/?priceName=" + $('.priceField').find('option:selected').text() // Use href attribute as URL
                            }).then(function (content) {
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
                            }).then(function (content) {
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
        });
    }

    /*
     * author: ???
     * method: ???
     */
    var loadItemService = function () {
        var rentID = $('#rentId').val();
        $.ajax({
            url: "/RoomManager/Service/LoadItemService",
            type: "GET",
            data: { rentId: rentID },
            cache: true,
            async: true,
            success: function (result) {
                $("#table-item-service").html(result);
            }
        });
    }

    /*
     * author: ???
     * method: ???
     */
    var updateRentInfo = function () {
        var formData = new FormData($("#form")[0]);
        $.ajax({
            url: "/RoomManager/Rent/UpdateRentInfo",
            type: 'POST',
            data: formData,
            async: false,
            success: function (result) {
                if (result == "1") {
                    var text = "Cập nhật dịch vụ thành công.";
                    ShowMessage(text, 2);
                } else {
                    var text = "Đã xảy ra lỗi.";
                    ShowMessage(text, 1);
                }
            },
            cache: false,
            contentType: false,
            processData: false
        });
    };

    /*
     * author: ???
     * method: ???
     */
    var updateEntireFee = function () {
        if ($("#rentFee").val() == undefined) {
            return;
        }

        var rentFee = parseInt($("#rentFee").val().replace(/,/g, ""));
        var orderFee = ($("#hdfOrderFee").val() != null) ? parseInt($("#hdfOrderFee").val()) : parseInt($("#orderFee").val().replace(/,/g, "")); //Lay tu hidden service form

        var addionFee = ($("#hdfTotalAddition").val() != null) ? parseInt($("#hdfTotalAddition").val()) : parseInt($("#addionFee").val().replace(/,/g, ""));
        var discountFee = ($("#hdfTotalDiscount").val() != null) ? parseInt($("#hdfTotalDiscount").val()) : parseInt($("#discountFee").val().replace(/,/g, ""));
        var totalFee = rentFee + orderFee + addionFee - discountFee;

        var totalPayment = ($("#hdfTotalPayment").val() != null) ? parseInt($("#hdfTotalPayment").val()) : parseInt($("#totalPayment").val().replace(/,/g, ""));

        var remainFee = totalFee - totalPayment;

        $("#orderFee").val(addCommas(orderFee));
        $("#rentFee").val(addCommas(rentFee));
        $("#addionFee").val(addCommas(addionFee));
        $("#discountFee").val(addCommas(discountFee));
        $("#totalFee").val(addCommas(totalFee));
        $("#totalPayment").val(addCommas(totalPayment));
        $("#remainFee").val(addCommas(remainFee));
    }

    /*
     * author: ???
     * method: Cap nhat lai gia tri cua Fee thue phong 
     */
    var updateRentFee = function (rentType) {

        var isFixedPrice = $("#isFixedPrice").is(':checked');
        //change focus
        $(event.toElement).closest(".btn-group").find(".btn").removeClass("active");
        $(event.toElement).addClass('active');
        $("#hdfRentType").val(rentType);

        if (!isFixedPrice) {

            var rentId = $("#rentId").val();
            var checkOutDate = ConvertDateVNToUS($('#CheckOutDate').val()); //Do co che tu sinh ID cua MVC 
            var checkInDate = ConvertDateVNToUS($('#CheckInDate').val());
            var priceGroup = $('#PriceGroupID').val();

            //Thay doi phi khi: gio vao thay doi, gio ra thay doi , hinh thuc thue thay doi
            //InvoiceID, checkInDate, Bike, Note duoc bat bang Onchange, chi con hdfRentType
            var oldRentType = $("#hdfRentType").val();
            $.ajax({
                url: "/Rent/CalculateRentFee",
                type: "GET",
                data: { rentID: rentId, rentType: rentType, checkInDate: checkInDate, checkOutDate: checkOutDate, priceGroup: priceGroup },
                cache: true,
                success: function (result) {
                    $('#rentFeeDetail').attr("data-content", result.description);
                    $("#rentFee").val(addCommas(result.rentFee));
                    updateEntireFee(result.rentFee, rentType);
                    changeStatusUpdateButton(false);
                }
            });
        }
    }

    /*
     * author: ???
     * method: ???
     */
    var getRoomFeeTab = function () {
        var rentID = $('#rentId').val();
        var check = $("#serviceTab").val();
        if (check == null) {

            $.ajax({
                url: "/RoomManager/Rent/GetRoomFee",
                type: "GET",
                data: { rentId: rentID },
                cache: true,
                async: true,
                success: function (result) {
                    $("#rent-detail").html(result);

                }
            });
        }
    };

    /*
     * author: ???
     * method: ???
     */
    var getServiceTab2 = function () {
        var rentID = $('#rentId').val();
        var check = $("#serviceTab").val();

        if (check == null) {
            //$("#tab9").html('<img src="../../Content/images/loading1.gif"/>');

            $.ajax({
                url: "/RoomManager/Rent/GetRentService",
                type: "GET",
                data: { rentId: rentID },
                cache: true,
                async: true,
                success: function (result) {
                    $("#rent-detail").html(result);

                }
            });
        }
    };

    /*
     * author: ???
     * method: ???
     */
    var getCustomerTab = function () {
        var rentID = $('#rentId').val();

        var check = $("#customerTab").val();
        if (check == null) {
            //$("#tab2").html('<img src="../../Content/images/loading1.gif"/>');

            $.ajax({
                url: "/RoomManager/Rent/GetRentCustomers",
                type: "GET",
                cache: true,
                data: { rentId: rentID },
                success: function (result) {
                    $("#rent-detail").html(result);

                    //$("#tab2").append("<div class='clearfix'></div>");
                }
            });
        }
    }

    /*
     * author: ???
     * method: ???
     */
    var getPaymentTab = function () {
        var rentID = $('#rentId').val();

        var check = $("#prepaidTab").val();
        if (check == null) {
            //$("#tab7").html('<img src="../../Content/images/loading1.gif"/>');

            $.ajax({
                url: "/RoomManager/Rent/GetRentPayments",
                type: "GET",
                cache: true,
                data: { rentId: rentID },
                success: function (result) {
                    $("#tab7").html(result);
                    $("#tab7").append("<div class='clearfix'></div>");

                }
            });
        }
    }

    /*
     * author: ???
     * method: ???
     */
    var getAdvanceTab = function () {

        $.ajax({
            url: "/RoomManager/Rent/GetChangeRoomView",
            type: "GET",
            cache: true,
            success: function (result) {
                $("#tab5").html(result);
                $('#roomTab .selectpicker').selectpicker();

            }
        });
    }

    return {
        init: init,
        getRoomFeeTab: getRoomFeeTab,
        getServiceTab2: getServiceTab2,
        getCustomerTab: getCustomerTab,
        getPaymentTab: getPaymentTab,
        getAdvanceTab: getAdvanceTab,
        loadItemService: loadItemService
    }
}();

/*
 * author: SinhCV
 * method: Delivey Management
 *          
 */
HMS.deliveryManagement = function () {

    var init = function () {
        $(document).ready(function () {
            /*
             * author: ???
             * method: [EVENT] Show delivery detail
             */
            $(document).on('click', '[data-role=detail]', function () {
                currentId = $(this).attr('data-id');
                //$.ajax({
                //    url: '/RoomManager/Service/LoadItemService',
                //    type: 'GET',
                //    data: {
                //        rentId: currentId
                //    },
                //    dataType: 'html',
                //    success: function (result) {
                //        $('#Item-order').html(result);
                //    }
                //});
                $('#order-detail-modal').modal('show');
                $('#order-detail-modal [data-role=customer-name]').html($(this).attr('data-customer-name'));
                $('#order-detail-modal [data-role=delivery-address]').html($(this).attr('data-delivery-address'));
                $('#order-detail-modal [data-role=customer-phone]').html($(this).attr('data-customer-phone'));
                if ($(this).attr('data-status') == 5) {
                    $('#store-list').show();
                    $('#store').hide();
                } else {
                    $('#store-list').val($(this).attr('data-storeid'));
                    $('#store').hide();
                }
                $('#order-detail-modal [data-role=storeId-name]').html($(this).attr('data-storeId-name'));
                $('#order-detail-modal [data-role=order-time]').html($(this).attr('data-order-time'));
                var status = getDeliveryStatusText(parseInt($(this).attr('data-status')));
                //if ($(this).attr('data-status') == 9 || $(this).attr('data-status') == 0 || $(this).attr('data-status') == 4) {
                if ($(this).attr('data-status') == 5 || $(this).attr('data-status') == 0) {
                    $('#order-detail-submit').show();
                } else {
                    $('#order-detail-submit').hide();

                }
                var btn = $('<span/>', {
                    'class': 'badge badge-' + status.color,
                    'html': status.status
                });
                $('#order-detail-modal [data-role=status]').html(btn);
                $('#order-detail-modal').modal('show');
                orderDetailTable._fnPageChange(0);
                orderDetailTable._fnAjaxUpdate();
            });

            /*
             * author: ???
             * method: [EVENT] Search store by input's address
             */
            $('.modal-order').on('click', '#btn-search-address', function () {
                var resultMarker = null;
                var address = $("#txt-delivery-address").val();
                window.geocoder.geocode({ 'address': address }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        map.setCenter(results[0].geometry.location);
                        if (resultMarker != null) {
                            resultMarker.setMap(null);
                        }
                        resultMarker = new google.maps.Marker({
                            map: map,
                            position: results[0].geometry.location
                        });
                    } else {
                        alert("Geocode was not successful for the following reason: " + status);
                    }
                });
                searchClosestStore(address);
            });

            /*
             * author: ???
             * method: [EVENT] ???
             */
            $('.modal-order').on('focusout', '#cust-address', function () {
                $("#txt-delivery-address").val($(this).val());
            });

            /*
             * author: TrundNDT
             * method: [EVENT] Deactivate all footer tabs when payment tab is shown
             */
            $('.modal-order').on('click', 'a[href="#tab-payment"]', function () {
                $('#listDeliveryTabs li.active').removeClass('active');
            });

            $('#modalDelivery').on('hide.bs.modal', function () {
                $('#service-temp-placeholder').append($('#service-container'));
            });

            $('#modalDelivery').on('show.bs.modal', function () {
                $('#service-placeholder').append($('#service-container'));
            });

            /*
             * author: TrundNDT
             * method: [EVENT] 
             */
            $(document).on('click', '[data-action="open-create-delivery-modal"]', function (e) {
                $.ajax({
                    url: '/DeliveryManager/Delivery/CreateDelivery',
                    type: 'GET',
                    dataType: 'html',
                    success: function (result) {
                        $('#modalDelivery').html(result);
                        $('#modalDelivery').modal('show');
                    }
                });
            });

            /*
             * author: TrundNDT
             * method: [EVENT] 
             */
            $(document).on('click', '[data-action="open-edit-delivery-modal"]', function (e) {
                var me = $(e.currentTarget),
                    rentId = me.attr('data-rentId');
                $.ajax({
                    url: '/DeliveryManager/Delivery/EditDelivery',
                    type: 'GET',
                    data: { 'rentId': parseInt(rentId) },
                    dataType: 'html',
                    success: function (result) {
                        $('#modalDelivery').html(result);
                        $('#modalDelivery').modal('show');
                    }
                });

            });

        });
    }

    var initCustomerTab = function () {
        /*
         * author: TrundNDT
         * method: [EVENT][Customer] Clear customer form to add new one
         */
        $('.modal-order').on('click', '[data-action="empty-customer-form"]', function (e) {
            showCustomerDetailsForm('create');
            // Deactivate membership button
            $('#tab-payment [data-type=membership]').attr('disabled', 'disabled');
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Customer] Show selected customer's info into top form
         */
        $('.modal-order').on('click', '[data-action=show-customer-info]', function (e) {
            var me = $(e.currentTarget),
                $formCustomer = $('#formCustomerDetails');
            var data = JSON.parse(me.attr('data-customer'));
            // Active current row
            $('#tab-customer .table-selectable tr.active').removeClass('active');
            me.parents('tr').addClass('active');
            // Active membership when a customer is selected
            $('#tab-payment [data-type=membership]').removeAttr('disabled');
            // Show up the form
            showCustomerDetailsForm('view');
            $.each(data, function (i, e) {
                $formCustomer.find('input[name="' + i + '"]').val(e);
            });
        });

        /*
         * author: TrundNDT
         * method: [EVENT][Customer]
         */
        $('.modal-order').on('click', '[data-action="edit-customer-info"]', function (e) {
            var me = $(e.currentTarget);

            switch (me.attr('data-mode')) {
                case 'edit':
                    showCustomerDetailsForm('edit');
                    break;
                case 'save': // For update existing customer
                    // TO DO: Save to database
                    showCustomerDetailsForm('view');
                    break;
                case 'create': // For create new customer
                    // TO DO: Save to database
                    showCustomerDetailsForm('view');
                    break;
            }
        });


        /*
         * author: TrundNDT
         * method: [EVENT][Customer]
         */
        $('.modal-order').on('click', '#btnViewCustomerHistory, #btnViewCustomerInfo', function (e) {
            var me = $(e.currentTarget);
            me.toggle();
            me.siblings().toggle();
        });
    }

    /*
     * author: TrungNDT
     * method: [Customer] show customer form based on type of action
     * params: {String} action: create/view/edit
     */
    var showCustomerDetailsForm = function (action) {
        var $formCustomer = $('#formCustomerDetails'),
            $panelCustomer = $('#tab-customer > .panel-fullscreen'),
            $title = $('#customerTitle'),
            $btnSave = $('#btnSaveCustomer'),
            $btnHistory = $('#btnViewCustomerHistory'),
            $btnInfo = $('#btnViewCustomerInfo'),
            $icon = $btnSave.find('.fa');

        // Show up the form
        if (!$panelCustomer.hasClass('have-large-header')) {
            $panelCustomer.addClass('have-large-header')
            $formCustomer.fadeIn();
            setModeForButtonSaveCustomer('save');
            $btnSave.fadeIn();
        }

        switch (action) {
            case 'create':
                $icon.removeClass('fa-pencil').addClass('fa-floppy-o');
                $formCustomer.find('input').removeAttr('disabled').val('');
                $btnSave.attr('data-mode', 'create');
                $btnHistory.hide();
                $btnInfo.hide();
                $title.html('Thêm khách hàng mới');
                break;
            case 'view':
                $icon.removeClass('fa-floppy-o').addClass('fa-pencil');
                $formCustomer.find('input').attr('disabled', 'disabled');
                $btnSave.attr('data-mode', 'edit');
                $btnHistory.show();
                $btnInfo.hide();
                $title.html('Thông tin khách hàng');
                break;
            case 'edit':
                $icon.removeClass('fa-pencil').addClass('fa-floppy-o');
                $formCustomer.find('input').removeAttr('disabled');
                $btnSave.attr('data-mode', 'save');
                $btnHistory.show();
                $btnInfo.hide();
                $title.html('Chỉnh sửa thông tin khách hàng');
                break;
        }
    }

    /*
     * author: TrungNDT
     * method: [Customer] change mode for btnSaveCustomer
     * params: {String} mode: save/edit
     */
    var setModeForButtonSaveCustomer = function (mode) {
        var $btnSave = $('#btnSaveCustomer');
        switch (mode) {
            case 'save': $btnSave.find('.fa').removeClass('fa-pencil').addClass('fa-floppy-o');
                break;
            case 'edit': $btnSave.find('.fa').removeClass('fa-floppy-o').addClass('fa-pencil');
                break;
        }
        $btnSave.attr('data-mode', mode);
    }

    /*
     * author: TrungNDT
     * method: [Map] init Google map
     */
    var initializeGoogleMap = function () {
        var styles = [
            { featureType: "road.highway", stylers: [{ visibility: "off" }] },
            { featureType: "landscape", stylers: [{ visibility: "off" }] },
            { featureType: "transit", stylers: [{ visibility: "off" }] },
            { featureType: "poi", stylers: [{ visibility: "off" }] },
            { featureType: "poi.park", stylers: [{ visibility: "on" }] },
            { featureType: "poi.park", elementType: "labels", stylers: [{ visibility: "off" }] },
            { featureType: "poi.park", elementType: "geometry.fill", stylers: [{ color: "#d3d3d3" }, { visibility: "on" }] },
            { featureType: "poi.medical", stylers: [{ visibility: "off" }] },
            { featureType: "poi.medical", stylers: [{ visibility: "off" }] },
            { featureType: "road", elementType: "geometry.stroke", stylers: [{ color: "#cccccc" }] },
            { featureType: "water", elementType: "geometry.fill", stylers: [{ visibility: "on" }, { color: "#cecece" }] },
            { featureType: "road.local", elementType: "labels.text.fill", stylers: [{ visibility: "on" }, { color: "#808080" }] },
            { featureType: "administrative", elementType: "labels.text.fill", stylers: [{ visibility: "on" }, { color: "#808080" }] },
            { featureType: "road", elementType: "geometry.fill", stylers: [{ visibility: "on" }, { color: "#fdfdfd" }] },
            { featureType: "road", elementType: "labels.icon", stylers: [{ visibility: "off" }] },
            { featureType: "water", elementType: "labels", stylers: [{ visibility: "off" }] },
            { featureType: "poi", elementType: "geometry.fill", stylers: [{ color: "#d2d2d2" }] }];

        // Create a new StyledMapType object, passing it the array of styles,
        // as well as the name to be displayed on the map type control.
        var styledMap = new google.maps.StyledMapType(styles,
        { name: "Styled Map" });

        mapOptions = {
            center: new google.maps.LatLng(10.789886817455374, 106.6787300934875),
            zoom: 14,
            mapTypeControlOptions: {
                mapTypeIds: [google.maps.MapTypeId.ROADMAP, 'map_style']
            }
        };

        map = new google.maps.Map(document.getElementById("map-canvas"),
            mapOptions);
        map.mapTypes.set('map_style', styledMap);
        map.setMapTypeId('map_style');

        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            google.maps.event.trigger(map, 'resize');
        });

        window.map = map;
        window.geocoder = new google.maps.Geocoder();
        //addStoreLocations(); ??
    }

    /*
     * author: ???
     * method: [INIT] setup google auto-complete for delivery address textbox
     */
    var setupAutocompleteForDeliveryBox = function () {
        var autocomplete = new google.maps.places.Autocomplete(
            (document.getElementById('txt-delivery-address')),
            {
                types: ['geocode']
            });

        google.maps.event.addListener(autocomplete, 'place_changed', function () {
            var place = autocomplete.getPlace();
            $('#btn-search-address').trigger('click');
        });

        addStoreLocations();
    }

    /*
     * author: ???
     * method: ???
     */
    var settingCustomerFilter = function () {
        if (window.sessionStorage.CallCenterSetting == null) {
            $('#btn-setting').attr('data-status', 'setting');
        } else {
            window.CallCenterNotifier = new CallCenter({
                port: window.sessionStorage.CallCenterSetting,
                onComming: function (phoneNumber) {
                    var customer = window["customers-list"].filter(function (o, n) {
                        return o.phone === phoneNumber;
                    });
                    if (customer.length === 0) {
                        ShowMessage("Cuộc gọi từ số " + phoneNumber, 2);
                    } else {
                        ShowMessage(customer[0].name + " gọi từ số " + phoneNumber, 2);
                    }
                },
                missCall: function (phoneNumber) {
                    var customer = window["customers-list"].filter(function (o, n) {
                        return o.phone === phoneNumber;
                    });
                    if (customer.length === 0) {
                        ShowMessage("Cuộc gọi nhỡ từ số " + phoneNumber, 1);
                    } else {
                        ShowMessage("Cuôc gọi nhỡ từ \"" + customer[0].name + "\" - " + phoneNumber, 1);
                    }
                }
            });
            window.CallCenterNotifier.onConnect = function () {
                $("#btn-setting").attr('data-status', "connect");
            };
            window.CallCenterNotifier.onDisconnect = function () {
                $("#btn-setting").attr('data-status', "disconnect");
            };
            window.CallCenterNotifier.init();
        }
    }


    /*
     * author: ???
     * method: ???
     */
    var searchClosestStore = function (address) {
        if (window.storeInfos == null) {
            alert("Dữ liệu danh sách cửa hàng chưa được load xong, vui lòng thử lại sau.");
            return;
        }
        var origins = [];
        for (var index in window.storeInfos) {
            var store = window.storeInfos[index];

            origins.push(new google.maps.LatLng(store.Latitude, store.Longitude));
        }

        var service = new google.maps.DistanceMatrixService();
        service.getDistanceMatrix(
        {
            origins: origins,
            destinations: [address],
            travelMode: google.maps.TravelMode.DRIVING,
        }, onFindingClosestStoreFinished);
    }

    /*
     * author: ???
     * method: ???
     */
    var onFindingClosestStoreFinished = function (response, status) {
        if (status == "OK") {
            var minDistance = Number.POSITIVE_INFINITY;
            var minIndex = -1;

            var resultString = "";
            var rs = [];
            var rows = [];
            var i;
            for (i = 0; i < response.rows.length; i++) {
                var lowest = response.rows[i].elements[0];
                lowest.storeIndex = i;
                rows.push(lowest);
            }
            var count = rows.length;
            var checkOk = function (ele) {
                return ele.status == 'OK';
            };
            for (var index = 0; rows.length > 0; i++) {
                var result = rows[0];
                if (rows.filter(checkOk).length != 0) {
                    var l = result.status == 'OK' ? result.distance.value : -1;
                    for (var j in rows) {
                        var ele = rows[j];
                        if (ele.status == 'OK' && (result.status != 'OK' || ele.distance.value < result.distance.value)) {
                            result = ele;
                        }
                    }
                }
                rows = rows.filter(function (ele) {
                    return ele.storeIndex != result.storeIndex;
                });
                if (result.status != "OK") {
                    rs.push($('<li/>', {
                        'class': 'row',
                        'html': [
                            $('<lable/>', {
                                'class': 'col-md-4',
                                html: window.storeInfos[result.storeIndex].Name
                            }),
                            $('<label/>', {
                                'class': 'col-md-8',
                                html: 'Không tìm thấy đường đi.'
                            })
                        ]
                    }));
                    continue;
                }
                rs.push($('<li/>', {

                    'html': $('<div/>', {
                        'class': 'row',
                        'html': [
                            $('<input/>', {
                                'type': 'hidden',
                                'value': window.storeInfos[result.storeIndex].ID
                            }),
                            $('<label/>', {
                                'class': 'col-md-5',
                                html: window.storeInfos[result.storeIndex].Name
                            }),
                            $('<label/>', {
                                'class': 'col-md-4',
                                html: result.distance.text
                            }),
                            $('<div/>', {
                                'class': 'col-md-3 text-center',
                                html: $('<button/>', {
                                    'class': 'btn btn-primary btn-sm',
                                    'html': 'Chọn',
                                    'data-role': 'select-storeId',
                                })
                            })
                        ]
                    })
                }));

                if (result.distance.value < minDistance) {
                    minDistance = result.distance.value;
                    minIndex = index;
                }
            }
            $('ul.nearby-stores').html('');
            if (minDistance == Number.POSITIVE_INFINITY) {
                $('#nearest-storeId').html($('<li/>', {
                    html: 'Không tìm thấy đường đi từ bất kỳ cửa hàng nào đến địa chỉ trên.'
                }));
            } else {
                var store = window.storeInfos[minIndex];
                $('ul.nearby-stores').html(rs);
                $('ul.nearby-stores li:first-child').addClass('active');
            }
        } else {
            alert("Lỗi: " + status);
        }
    }

    /*
     * author: ???
     * method: ???
     */
    var addStoreLocations = function () {
        var listStore = new Array();
        var markerList = [];
        $.ajax({
            url: "/DeliveryManager/Delivery/GetStoreCoordinateList",
            type: "POST",
            success: function (data) {
                window.storeInfos = data;
                for (var i in markerList) {
                    markerList[i].setMap(null);
                }
                for (var index in data) {
                    var store = data[index];
                    listStore.push(store);
                    var img = "/Content/images/m_terminal.png";
                    var marker = new google.maps.Marker({
                        position: new google.maps.LatLng(store.Latitude, store.Longitude),
                        map: window.map,
                        title: store.Name,
                        icon: img
                    });
                    markerList.push(marker);
                }
            },
            error: function () {
                window.setTimeout(addStoreLocations, 500);
            },
        });
    }


    var getDeliveryStatusText = function (value) {
        var status = "";
        var color = "";
        switch (value) {
            case 0:
                status = "Đơn hàng mới";
                color = "info";
                break;
            case 1:
                status = "Đã assign cho cửa hàng";
                color = "default";
                break;
            case 2:
                status = "Đang xử lý";
                color = "primary";
                break;
            case 3:
                status = "Đang giao";
                color = "primary";
                break;
            case 4:
                status = "Đã hoàn thành";
                color = "success";
                break;
            case 5:
                status = "Hủy trước khi chế biến";
                color = "danger";
                break;
            case 6:
                status = "Hủy sau khi chế biến";
                color = "danger";
                break;
            case 7:
                status = "Không tìm thấy khách hàng";
                color = "warning";
                break;
            case 8:
                status = "Cửa hàng từ chối";
                color = "danger";
                break;
            case 9:
                status = "Đơn hàng mới (fakebook)";
                color = "info";
                break;
        }
        return { 'status': status, 'color': color };
    }


    return {
        init: init,
        initCustomerTab: initCustomerTab,
        //initPaymentTab: initPaymentTab,
        setupAutocompleteForDeliveryBox: setupAutocompleteForDeliveryBox,
        initializeGoogleMap: initializeGoogleMap,
        getDeliveryStatusText: getDeliveryStatusText
        //loadItemServiceDelivery: loadItemServiceDelivery
    }
}();

HMS.getRentCus = function () {
    /*
         * author: SinhCV
         * method: Rent/GetRentCustomers
         *          
         */
    var refreshRentCusTable = function () {
        var oTable = $("#tblCustomerDetail").dataTable();
        oTable._fnPageChange(0);
        oTable._fnAjaxUpdate();
    };

    var removeCustomer = function (customerId) {
        bootbox.dialog({
            title: 'Xác nhận',
            message: "<h6>Xóa khách hàng?</h6>",
            buttons:
            {
                "ok":
                {
                    "label": "<i class='icon-remove'></i> Đồng ý!",
                    "className": "btn-sm btn-success",
                    "callback": function () {
                        $.ajax({
                            url: "/RoomManager/Rent/RemoveRentCustomers",
                            type: "GET",
                            data: { customerId: customerId, rentID: $('#rentId').val() },
                            success: function (result) {

                                if (result == "1") {
                                    refreshRentCusTable();
                                    ShowMessage("Xóa thành công.", 2);
                                    $("#wrapCustomerTable").fadeOut(200, function () {
                                        $("#customerid-" + customerId).remove();
                                        $("#wrapCustomerTable").fadeIn(200, function () {
                                            $("#wrapCustomerTable").mCustomScrollbar("update");
                                        });
                                    });

                                } else {
                                    ShowMessage("Có lỗi xảy ra.", 1);
                                }
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

    var addRentCustomer = function () {
        event.preventDefault();
        var rentId = $("#rentId").val();
        var personId = $("#personId").val();
        var customerName = $("#customerName").val();
        var address = $("#addressCus").val();
        var phoneNumber = $("#phoneNumber").val();
        if (personId == "") {
            ShowMessage("Vui lòng nhập CMND", 3);
            return false;
        }
        if (customerName == "") {
            ShowMessage("Vui lòng nhập Tên khách hàng", 3);
            return false;
        }
        if (personId !== "" && customerName !== "") {
            $.ajax({
                url: '/RoomManager/Rent/AddRentCustomers',
                type: 'GET',
                data: {
                    rentId: rentId,
                    personId: personId,
                    customerName: customerName,
                    address: address,
                    phoneNumber: phoneNumber
                },
                success: function (result) {
                    if (result != null) {
                        $("#personId").val("");
                        $("#customerName").val("");
                        $("#addressCus").val("");
                        $("#phoneNumber").val("");
                        $("#customerList").prepend(result);
                        refreshRentCusTable();
                    };

                }
            });
        }
    };

    $('.slim-scroll').each(function () {
        var $this = $(this);
        $this.slimScroll({
            height: $this.attr('data-height') || 100,
            railVisible: true
        });
    });

    $(function ($) {
        $('#checkHaveLeader').on('click', function () {
            var inp = $("#leaderName").get(0);
            if (inp.hasAttribute('disabled')) {
                inp.removeAttribute('disabled');
            } else {
                inp.setAttribute('disabled', 'disabled');
                inp.value = "";
            }
        });
    });
    return {
        removeCustomer: removeCustomer,
        addRentCustomer: addRentCustomer
    }
};

HMS.getChangeRoom = function () {
    /*
    * author: SinhCV
    * method: Rent/GetChangeRoomView
    *          
    */
    function updateChangeFloorDisplay() {
        var targetFloorId = $('#floor-container-change').find('.item.active').attr('data-floor-id-change');
        $('[data-role="change-floor"]').next().find('.btn .filter-option').html('Tầng ' + targetFloorId);
    }

    $(window).load(function () {
        window["view-mode"] = 'floor';
        $('#floor-container-change').carousel('pause');
        window["total-floor"] = '(((IEnumerable<Hotel.Data.RoomFloor>)ViewBag.Floors).Count())';
        var floor = $('.carousel-inner .active').attr('data-floor-id-change');
        $('#main-container-change' + floor).mixItUp();
        window["current-floor"] = floor;
        //FilterRoom();
        setQtipForRoom();
        $('.selectpicker').selectpicker();
    });

    $(document).ready(function () {
        $('#floor-container-change').on('slid', function () {
            var floor = $('.carousel-inner .active').attr('data-floor-id-change');
            $('#main-container-change' + floor).mixItUp();
            $('#main-container-change' + window["current-floor"]).mixItUp('destroy');
            $('#main-container-change' + window["current-floor"] + ' .mix').css('display', 'none');
            window["current-floor"] = floor;
            $('#room-floor-cbo-change').val(floor);
        });

        $(document).on('click', '#room-item-change', function () {
            var state = $(this).attr('data-state-change');
            var roomName = $(this).find('.room-number').html();
            var newRoomId = $(this).attr('data-id-change');
            var rentId = $(this).attr('data-rent-change');
            bootbox.dialog({
                title: 'Xác nhận',
                message: "<h6>Bạn có chắc đổi qua <span style='color:blue;font-weight:bold'>" + roomName + "</span> này không?</h6>",
                buttons:
                {
                    "ok":
                    {
                        "label": "<i class='icon-remove'></i> Đồng ý!",
                        "className": "btn-sm btn-success",
                        "callback": function () {
                            ChangeRoom(newRoomId);
                            window["current-room"] = roomId;
                            bootbox.hideAll();
                            var room = $('.room[data-id=' + newRoomId + ']');
                            room.attr('data-rentId', rentId);
                            location.reload();
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

        });

        $(document).on('click', '#view-mode-btn-change', function () {
            $('#view-mode-btn-change.active').removeClass('active');
            $(this).addClass('active');
            var currentMode = window["view-mode-change"];
            var mode = $(this).attr('data-mode-change');
            if (currentMode == mode) {
                return;
            }
            window["view-mode-change"] = mode;
            if (mode == 'floor') {
                $('#main-container-change').mixItUp('destroy');
                $('#main-container-change .mix').css('display', 'none');
                $('[data-view-type-change=floor]').show(100);
                $('[data-view-type-change=grid]').hide(100);
                $('#main-container-change' + window["current-floor"]).mixItUp();
            } else {
                $('[data-view-type-change=floor]').hide(100);
                $('[data-view-type-change=grid]').show(100);
                $('#main-container-change').mixItUp();
                $('#main-container-change' + window["current-floor"]).mixItUp('destroy');
                $('#main-container-change' + window["current-floor"] + ' .mix').css('display', 'none');
            }
        });
        /*
         * author: TrungNDT
         * method: [EVENT] Change display floor name when change floor dropdown
         */
        $(document).on('change', '[data-role="change-floor"]', function (e) {
            var target = $(e.currentTarget);
            target.next().find('.btn .filter-option').html('Tầng ' + target.val());
        });

        /*
         * author: TrungNDT
         * method: [EVENT] Change display floor name when click next/prev button
         */
        $('#floor-container-change').on('slid.bs.carousel', function () {
            updateChangeFloorDisplay();
        });

    });
};

HMS.menu = function () {
    var init = function () {
        $(function () {
            renderMenuTree();
        });
    }

    /*
     * author: TrungNDT
     * method: Convert a flat list to a new list with tree structure
     * params: {List} flatMenu
     */
    var convertFlatListToTree = function (flatMenu, rootName) {
        var map = {}, // mapping parent id
            node,   // temp node
            tree = []; // result
        tree.push({
            text: rootName,
            nodes: []
        });
        for (var i = 0; i < flatMenu.length; i += 1) {
            node = flatMenu[i];
            node['text'] = flatMenu[i]['MenuText'];

            if ($('#filterRoles').val() == '-1' || $.inArray($('#filterRoles').val(), node['roleIDs']) > -1) {
                if (node.MenuLevel != 1) {
                    var parentNode = flatMenu[map[node['ParentMenuId']]];
                    if (parentNode != undefined) {
                        map[node['Id']] = i; // use map to look-up the parents

                        if (parentNode['nodes'] == undefined) {
                            parentNode['nodes'] = [];
                        }
                        parentNode['nodes'].push(node);
                    }
                } else {
                    map[node['Id']] = i; // use map to look-up the parents
                    tree[0]['nodes'].push(node);
                }
            }
        }
        return tree;
    }

    var convertToDropDownList = function (flatMenu, rootId, rootName) {
        var map = {}, // mapping parent id
            node,   // temp node
            list = []; // result
        var count = 1;
        list.push({
            text: rootName,
            value: rootId
        });
        for (var i = 0; i < flatMenu.length; i += 1) {
            node = flatMenu[i];
            node['text'] = flatMenu[i]['MenuText'];

            if ($('#filterRoles').val() == '-1' || $.inArray($('#filterRoles').val(), node['roleIDs']) > -1) {
                if (node.MenuLevel != 1) {
                    var parentNode = list[map[node['ParentMenuId']]];
                    if (parentNode != undefined) {
                        map[node['Id']] = count; // use map to look-up the parents
                        count = count + 1;

                        list.push({
                            text: parentNode['text'] + " > " + node['MenuText'],
                            value: node['Id']
                        });
                    }
                } else {
                    map[node['Id']] = count; // use map to look-up the parents
                    count = count + 1;
                    list.push({
                        text: rootName + " > " + node['MenuText'],
                        value: node['Id']
                    });
                }
            }
        }
        return list;
    }

    /*
     * author: TrungNDT
     * method: [INIT] Get menu list from db & render menu tree
     */
    var renderMenuTree = function () {
        $.ajax({
            url: '/Admin/Menu/GetMenuList',
            dataType: 'Json',
            success: function (result) {

                var rootName = $('li.active .menu-tab').text();
                var rootId = $('li.active .menu-tab').data('id');
                var listMenu = [];
                var treeId = "#tree-store";
                if (rootId == result['adminId']) {
                    listMenu = result['adminMenu'];
                    treeId = "#tree-admin";
                } else {
                    listMenu = result['storeMenu'];
                }

                //load dropdown
                var list = convertToDropDownList(listMenu, rootId, rootName);
                var dropdown = $('#cbxParentMenuId');
                $('option', dropdown).remove();

                $.each(list, function () {
                    dropdown.append("<option value='" + this['value'] + "'>" + this['text'] + "</option>");
                });

                //load admin tree
                var tree = convertFlatListToTree(listMenu, rootName);
                $(treeId).treeview({
                    data: tree,
                    onNodeSelected: function (event, data) {
                        var form = $('#editForm');

                        $.each(data, function (i, e) {
                            form.find('input[name="' + i + '"]').val(e);
                            form.find('select[name="' + i + '"]').val(e);
                            if (i == 'roleIDs') {
                                $('#allRoles').magicSuggest().clear();
                                $('#allRoles').magicSuggest().setValue(e);
                                $('#allRoles').magicSuggest().collapse();
                            }
                        });

                        //switch mode add/edit
                        if ($('#txtMenuID').val() == '') {
                            $('#btnSave').hide();
                            $('#btnAdd').show();
                        } else {
                            $('#btnAdd').hide();
                            $('#btnSave').show();
                        }
                    },
                    onNodeUnselected: function () {
                        $(':input', '#editForm').val('');

                        //switch mode add/edit
                        if ($('#txtMenuID').val() == '') {
                            $('#btnSave').hide();
                            $('#btnAdd').show();
                        } else {
                            $('#btnAdd').hide();
                            $('#btnSave').show();
                        }
                    }
                });

            }
        });
    }

    return {
        renderMenuTree: renderMenuTree
    }
}();

HMS.General.init();

/*
 * author: TrungNDT
 * method: Convert number to money format (10000 -> 10.000đ)
 * params: {Number} val: money value
 */
var toMoney = function (val, splitter, unit) {
    var s = splitter || '.';
    var u = unit || '';
    return accounting.formatMoney(val, "", 0, s) + u;
}

/*
 * author: TrungNDT
 * method: Convert money to number (10.000đ -> 10000)
 * params: {String} str: money string
 */
var moneyToNumber = function (str) {
    return parseInt(str.replace('đ', '').replace(/\./g, ''));
}

function ShowMessage(message, type, time) {
    var mapper = {
        '1': ['error', 'Lỗi'],
        '2': ['info', 'Thông tin'],
        '3': ['success', 'Thành công']
    }

    $.gritter.add({
        title: mapper[type][1],
        text: message,
        time: time || 2000, //ms
        class_name: 'gritter-' + mapper[type][0] //+ ' gritter-light'
    });
    //  $.gritter.removeAll();
}

jQuery.expr[":"].Contains = jQuery.expr.createPseudo(function (arg) {
    return function (elem) {
        return jQuery(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
    };
});

//redraw datatable without reload: LinhNV
function ReDrawDatatable(tableId) {
    $.fn.dataTableExt.oApi.fnStandingRedraw = function (oSettings) {
        if (oSettings.oFeatures.bServerSide === false) {
            var before = oSettings._iDisplayStart;
            oSettings.oApi._fnReDraw(oSettings);
            //iDisplayStart has been reset to zero - so lets change it back
            oSettings._iDisplayStart = before;
            oSettings.oApi._fnCalculateEnd(oSettings);
        }

        //draw the 'current' page
        oSettings.oApi._fnDraw(oSettings);
    };
    $("#" + tableId).dataTable().fnStandingRedraw();
}

var provinces = [
  "An Giang",
  "Bà Rịa - Vũng Tàu",
  "Bắc Giang",
  "Bắc Kạn",
  "Bạc Liêu",
  "Bắc Ninh",
  "Bến Tre",
  "Bình Định",
  "Bình Dương",
  "Bình Phước",
  "Bình Thuận",
  "Cà Mau",
  "Cao Bằng",
  "Đắk Lắk",
  "Đắk Nông",
  "Điện Biên",
  "Đồng Nai",
  "Đồng Tháp",
  "Gia Lai",
  "Hà Giang",
  "Hà Nam",
  "Hà Tĩnh",
  "Hải Dương",
  "Hậu Giang",
  "Hòa Bình",
  "Hưng Yên",
  "Khánh Hòa",
  "Kiên Giang",
  "Kon Tum",
  "Lai Châu",
  "Lâm Đồng",
  "Lạng Sơn",
  "Lào Cai",
  "Long An",
  "Nam Định",
  "Nghệ An",
  "Ninh Bình",
  "Ninh Thuận",
  "Phú Thọ",
  "Quảng Bình",
  "Quảng Nam",
  "Quảng Ngãi",
  "Quảng Ninh",
  "Quảng Trị",
  "Sóc Trăng",
  "Sơn La",
  "Tây Ninh",
  "Thái Bình",
  "Thái Nguyên",
  "Thanh Hóa",
  "Thừa Thiên Huế",
  "Tiền Giang",
  "Trà Vinh",
  "Tuyên Quang",
  "Vĩnh Long",
  "Vĩnh Phúc",
  "Yên Bái",
  "Phú Yên",
  "Cần Thơ",
  "Đà Nẵng",
  "Hải Phòng",
  "Hà Nội",
  "Hồ Chí Minh",
]


jQuery.fn.datatablevpn = function (options) {
    if (options && options.aoColumnDefs) {
        for (var i = 0; i < options.aoColumnDefs.length; i++) {
            var item = options.aoColumnDefs[i];

            if (item.fnRender) {
                item.mRender = function (a, b, c) {
                    return this.fnRender({ aData: c });
                }.bind(item);
            }
        }
    }

    jQuery.fn.dataTable.call(this, options);
}

function ShowAlert(message, type, url) {
    var mapper = {
        '1': ['error', 'Lỗi'],
        '2': ['success', 'Thành công'],
        '3': ['info', 'Thông tin']
    }
    swal({
        title: mapper[type][1],
        text: message,
        type: mapper[type][0]
    }, function () {
        if (url != null) {
            window.location.href = url;
        }
    });
}

function ShowConfirm(message, ajax) {
    swal({
        title: message,
        type: "warning",
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: "Đồng ý",
        confirmButtonClass: 'btn btn-success',
        cancelButtonText: "Không",
        showCancelButton: true,
        closeOnConfirm: false
    }, ajax
    );
}