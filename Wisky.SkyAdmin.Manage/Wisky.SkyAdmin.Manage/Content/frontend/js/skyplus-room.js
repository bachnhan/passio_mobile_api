HMS.Room = function () {
    var init = function () {
        $(window).load(function () {
            window["view-mode"] = 'floor';
            $('#floor-container').carousel('pause');
            window["total-floor"] = $('#countFloor').val();
            var floor = $('.carousel-inner .active').attr('data-floor-id');
            $('#main-container' + floor).mixItUp();
            window["current-floor"] = floor;
            filterRoom();
            //setQtipForRoom();
        });

        $(document).ready(function () {

            $('#floor-container').on('slid', function () {
                var floor = $('.carousel-inner .active').attr('data-floor-id');
                $('#main-container' + floor).mixItUp();
                $('#main-container' + window["current-floor"]).mixItUp('destroy');
                $('#main-container' + window["current-floor"] + ' .mix').css('display', 'none');
                window["current-floor"] = floor;
                $('#room-floor-cbo').val(floor);
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Open RentDetail Modal
             */
            $('#floor-container, #main-container').on('click', '.room', function () {
                event.stopPropagation();
                var $me = $(this),
                    roomData = {
                        id: $me.attr('data-id'),
                        name: $me.attr('data-name'),
                        category: $me.attr('data-category'),
                        state: parseInt($me.attr('data-state')),
                        rentId: $me.attr('data-rentid'),

                    };
                switch (roomData.state) {
                    case 1: ShowBookingForm(roomData.id, roomData.name);
                        break;
                    case 2: showRentDetails(roomData.rentId, roomData.name, roomData.id);
                        break;
                    case 3: openConfirmModal(roomData.id, roomData.name);
                        break;
                }

                window["current-room"] = roomData.id;
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $(document).on('click', '[data-role="category-filter"] .filter-item', function (e) {
                var currentActiveItem = $('[data-role="category-filter"] .filter-item.active');
                var me = $(this);
                if (currentActiveItem != me) {
                    currentActiveItem.removeClass('active');
                    me.addClass('active');
                }
                filterRoom();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] 
             */
            $(document).on('click', '[data-role=view-mode-btn]', function () {
                $('[data-role=view-mode-btn].active').removeClass('active');
                $(this).addClass('active');
                var currentMode = window["view-mode"];
                var mode = $(this).attr('data-mode');
                if (currentMode == mode) {
                    return;
                }
                var $floorBtn = $('[data-role="change-floor"] ~ .btn-group > .btn, [data-role="change-floor"] ~ .arrow-group > .btn');
                window["view-mode"] = mode;
                if (mode == 'floor') {
                    $('#main-container').mixItUp('destroy');
                    $('#main-container .mix').css('display', 'none');
                    $('[data-view-type=floor]').show(100);
                    $('[data-view-type=grid]').hide(100);
                    $('#main-container' + window["current-floor"]).mixItUp();
                    $floorBtn.removeClass('disabled');
                } else {
                    $('[data-view-type=floor]').hide(100);
                    $('[data-view-type=grid]').show(100);
                    $('#main-container').mixItUp();
                    $('#main-container' + window["current-floor"]).mixItUp('destroy');
                    $('#main-container' + window["current-floor"] + ' .mix').css('display', 'none');
                    $floorBtn.addClass('disabled');
                }
            });

            $('#room-state-filter-cbo').on('change', function () {
                filterRoom();
            });

            $('#room-floor-cbo').on('change', function () {
                var floor = parseInt($(this).val()) - 1;
                if (!isNaN(floor)) {
                    $('#floor-container').carousel(floor);
                    filterRoom();
                }
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Change display floor name when change floor dropdown
             */
            $(document).on('change', '[data-role="change-floor"]', function (e) {
                var $me = $(this),
                    $selectedFloor = $me.next().find('ul > .selected');
                $('#floor-container').carousel($selectedFloor.index());
                updateFloorDisplay();
            });

            /*
             * author: TrungNDT
             * method: [EVENT] Change display floor name when click next/prev button
             */
            $('#floor-container').on('slid.bs.carousel', function () {
                updateFloorDisplay();
            });

            //$('#floor-container').on('mouseenter', '.room', function () {
            //    var api = $(this).qtip(),
            //        $me = $(this);

            //    // Support method
            //    var setContent = function (roomId) {
            //        console.log('roomId : ' + roomId);
            //        $.ajax({
            //            url: "/RoomManager/Room/GetRoom/?id=" + roomId + "&actions=hover" // Use href attribute as URL
            //        })
            //                       .then(function (content) {
            //                           // Set the tooltip content upon successful retrieval
            //                           console.log(content);
            //                           api.set('content.text', content);
            //                       }, function (xhr, status, error) {
            //                           // Upon failure... set the tooltip content to error
            //                           api.set('content.text', status + ': ' + error);
            //                       });

            //        return 'Đang load...'; // Set some initial text
            //    }

            //    setContent($me.attr('data-id'));
            //    //$(this).qtip().set('content.text', 'hahaha');
            //});

        });

    }

    /*
     * author: TrungNDT
     * method: [EVENT] Change display floor name
     */
    var updateFloorDisplay = function () {
        var targetFloorName = $('#floor-container').find('.item.active').attr('data-floor-name');
        $('[data-role="change-floor"]').next().find('.btn .filter-option').html(targetFloorName);
    };

    /*
     * author: TrungNDT
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
     * method: [EVENT] 
     */
    var updateLabelPanel = function () {
        $.ajax({
            url: '/RoomManager/Room/GetRoomState',
            data: {
                roomId: window['current-room']
            },
            success: function (data) {
                if (data.success) {
                    $('.room[data-id="' + window['current-room'] + '"]').attr('data-state', data.data);
                    $('.room[data-id="' + window['current-room'] + '"]').attr('data-rentId', data.rentId);
                    $('.room[data-id="' + window['current-room'] + '"]').attr('data-rent', 1);
                }
            }
        });
    };

    /*
     * author: TrungNDT
     * method: [EVENT] 
     */
    var setQtipForRoom = function () {
        //console.log('setQtipForRoom');
        var $me = $(this),
            roomId = $me.attr('data-id'),
            $selectedRoom;
        var setContent = function (api, roomId) {
            //console.log('roomId : ' + roomId);
            $.ajax({
                url: "/RoomManager/Room/GetRoom/?id=" + roomId + "&actions=hover" // Use href attribute as URL
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
        $('.room').qtip({
                content: {
                    text: function (event, api) {
                        //console.log($(this).attr('data-id'));
                        //$selectedRoom = $(this);
                        //setContent(api, roomId);
                    }
                },
                position: {
                    my: 'top left',
                    at: 'top right',
                    target: 'mouse',
                    viewport: $(window),
                    adjust: {
                        method: 'shilf',
                        x: 15,
                        y: 50
                    }
                },
                show: '', 
                //show: {
                //    solo: true,
                //    delay: 400
                //},
                style: 'qtip-primary',
                //events: {
                //    show: function (event, api) {
                //        //console.log(api.get('content.text'));
                //        setContent(api, roomId);
                //    }
                //}
            });
    };

    /*
     * author: TrungNDT
     * method: [EVENT] 
     */
    var filterRoom = function () {
        var condition = '';

        var floor = $('.carousel-inner .active').attr('data-floor-id');
        // State: Ready/InUse/Prepare
        var state = $('[data-role="category-filter"] .filter-item.active').data('state');
        if (state != -1) {
            condition += '[data-state=' + state + ']';
        }

        // Room Category (Loai phong): VIP / Super VIP / VIPVIP
        var category = $('#room-state-filter-cbo').val();
        if (category != -1) {
            condition += '[data-category=' + category + ']';
        }

        condition = condition.length == 0
            ? 'all'
            : condition[condition.length - 1] == ','
            ? condition.substr(0, condition.length - 1)
            : condition;

        var mode = window['view-mode'];
        if (mode == 'floor') {
            $('#main-container' + floor).mixItUp('filter', condition);
        } else {
            $('#main-container').mixItUp('filter', condition);
        }
    };

    /*
     * author: TrungNDT
     * method: [EVENT] 
     */
    var closeRentDetailForm = function () {
        var roomId = $("#roomId").val();
        var rentType = $("#hdfRentType").val();

        //Kiem tra rentType o mainform neu khac thi load
        $(".aRoom").removeClass('active');
        var roomRentType = $("#r-" + roomId).attr("data-rentType");
        if (roomRentType != rentType) {
            updateRoomStatus(roomId);
        }
    };

    var focusRentType = function () {
        //Focus rent type button
        var rentType = $("#hdfRentType").val();
        $('[data-rentType]').removeClass("active");
        var btnCurrentFee = $('[data-rentType =' + rentType + ']');
        btnCurrentFee.addClass("active");
    };

    var setDateTimePicker = function () {

        //Chinh datetime picker
        var checkindate = $('#CheckInDate').val();

        //$('#CheckInDate').val(ConvertDateVNToUS(checkindate));

        $('#CheckInDate').datetimepicker({
            onClose: function (dateText, inst) {
                var rentType = $("#hdfRentType").val();
                UpdateRentFee(rentType);
            },
            dateFormat: 'dd-mm-yy'
        });

        $('#CheckInDate').datetimepicker('setDate', (new Date($('#CheckInDate').val())));
        //Chinh Datetime picker

        //var checkoutdate = $('#CheckOutDate').val();
        //$('#CheckOutDate').val(ConvertDateVNToUS(checkoutdate));

        $('#CheckOutDate').datetimepicker({
            onClose: function (dateText, inst) {
                var rentType = $("#hdfRentType").val();
                UpdateRentFee(rentType);
            },
            dateFormat: 'dd-mm-yy'
        });
        $('#CheckOutDate').datetimepicker('setDate', (new Date($('#CheckOutDate').val())));

        $("#CheckInDate").change(function () {
            $(this).closest('.form-group').addClass("has-success");
        });

        $("#CheckOutDate").change(function () {
            $(this).closest('.form-group').addClass("has-success");
        });

    };

    var changeStatusUpdateButton = function (disable) {
        if (disable) {
            $("#btnUpdateRentInfo").attr("disabled", "disabled");
            $("#btnUpdateRentInfo").removeClass("btn-info");
            $("#btnCheckOut").removeAttr("disabled", "disabled");
        } else {
            $("#btnUpdateRentInfo").removeAttr("disabled");
            $("#btnUpdateRentInfo").addClass("btn-info");
            $("#btnCheckOut").attr("disabled", "disabled");

        }
    };

    var applyScrollbar = function () {
        $('#rentFeeDetail').popover();
        $("#wrapMenu").mCustomScrollbar({

            advanced: {
                updateOnContentResize: true
            }
        });
        $("#wrapOrderTable").mCustomScrollbar({

            advanced: {
                updateOnContentResize: true
            }
        });
        $("#wrapPaymentTable").mCustomScrollbar({

            advanced: {
                updateOnContentResize: true
            }
        });
        $("#wrapCustomerTable").mCustomScrollbar({

            advanced: {
                updateOnContentResize: true
            }
        });
    };

    /*
     * author: TrungNDT
     * method: Load current room's ordered item into "Table Item Service" 
     *         (left side table)
     */
    var loadItemService = function (rentId) {
        var rentId = rentId || parseInt($('#rentId').val());
        $.ajax({
            url: "/RoomManager/Service/LoadItemService",
            type: "GET",
            data: { rentId: rentId },
            cache: true,
            async: true,
            success: function (result) {
                window.HMS.DualScreen.loadTableItemService(result);
            }
        });
    }

    var changeRoomStatusToReady = function (roomId) {
        $.ajax({
            url: "/Room/ChangeRoomStatus",
            type: "GET",
            data: { roomId: roomId, fromStatus: 3, toStatus: 1 }, //3: Prepare --> 1: Ready
            success: function (result) {
                if (result == "True") {
                    //location.reload();     
                    $('.room[data-id=' + roomId + ']').attr('data-state', 1);
                }
            }
        });
    }

    var openConfirmModal = function (roomId, roomName) {
        bootbox.dialog({
            message: "<h3 class='bigger-110'>Xác nhận dọn phòng</h3><br/>" +
                "<h6>Thay đổi phòng " + roomName + " sang trạng thái cho thuê?</h6>",
            buttons:
            {
                "ok":
                 {
                     "label": "<i class='icon-ok'></i> Đồng ý!",
                     "className": "btn-sm btn-success",
                     "callback": function () {
                         changeRoomStatusToReady(roomId);
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
    };

    var showRentDetails = function (rentId, roomName, roomId) {
        closeRentDetailForm();
        $.ajax({
            url: "/RoomManager/Rent/Index",
            type: "GET",
            data: { rentId: rentId },
            cache: true,
            success: function (result) {
                $('#rentId').val(rentId);
                $('#roomId').val(roomId);

                $.when(
                    //window.HMS.DualScreen.loadTabOrder(),
                    window.HMS.DualScreen.loadTabCustomer(rentId),
                    window.HMS.DualScreen.loadTabRentFee(rentId),
                    window.HMS.DualScreen.loadTabPayment(rentId),
                    window.HMS.DualScreen.loadTabChangeRoom(),
                    loadItemService(rentId)
                    ).then(function () {
                        window.HMS.DualScreen.renderTabHandlers();
                        window.HMS.DualScreen.activeFirstTab();
                    });

                $('#modalRentDetails').modal('show');
                $('#roomTitle').html(roomName);
                //focusRentType();
                //setDateTimePicker();
                //changeStatusUpdateButton(true);
                //applyScrollbar();
            }
        });


    };
    ////////////// RENT BOOKING FORM  ////////////////////////////////
    var ShowBookingForm = function (roomId, roomName) {
        $('.loading-gif').fadeIn();
        $.ajax({
            url: "/RoomManager/Rent/ShowBookingForm",
            type: "GET",
            data: { roomId: roomId, roomName: roomName },
            success: function (result) {
                $("#modal").html(result).modal('show');
                $('#CheckInDate').datetimepicker({ format: 'dd-mm-yy' });
                $('#CheckInDate').datetimepicker('setDate', (new Date()));
                $('#InvoiceID').val(getDateStamp());
                $('.loading-gif').fadeOut();
            }
        });
    }   

    function padStr(i) {
        return (i < 10) ? "0" + i : "" + i;
    }

    function getDateStamp() {
        var date = new Date();
        var dateStr = padStr(date.getFullYear() - 2000) +
                padStr(1 + date.getMonth()) +
                    padStr(date.getDate()) +
                        padStr(date.getHours()) +
                            padStr(date.getMinutes()) +
                                padStr(date.getSeconds());
        return dateStr;
    }
   
    return {
        init: init,
        changeStatusUpdateButton: changeStatusUpdateButton,
        loadItemService: loadItemService
    }
}();
