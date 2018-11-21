HMS.Dashboard = function () {
    var isStoreInit = false;
    var isProductInit = false
    var isCashierInit = false;
    var isPaymentInit = false;
    var ControllerName,
        Domain,
        StoreId,
        BrandId,
        storeChartData;

    var init = function (domain) {
        isStoreInit = false;
        isProductInit = false
        isCashierInit = false;
        isPaymentInit = false;
        $(document).ready(function () {
            Domain = domain;
            ControllerName = "Date" + 'DashBoard';
            StoreId = parseInt($('#hiddenStoreId').val());
            BrandId = parseInt($('#hiddenBrandId').val());
            StartDate = '';
            EndDate = '';
            $('[data-role=unit]').html(domain == 'Date' ? 'ngày' : 'tháng');
            DateList = [];
            MonthList = [];

            renderStatisticByMonth();

            if ($("div#storeContent").hasClass("active")) {
                isStoreInit = true;
                //$('#storeReportDatatable').dataTable().fnDestroy();
                initStoreDatatable();
            } else if ($("div#productContent").hasClass("active")) {
                isProductInit = true;
                //$('#tblProduct').dataTable().fnDestroy();
                initProductDatatable();
            } else if ($("div#staffContent").hasClass("active")) {
                isCashierInit = true;
                //$('#tblCashier').dataTable().fnDestroy();
                initCashierDatatable();
            } else if ($("div#cardPaymentContent").hasClass("active")) {
                isPaymentInit = true;
                //$('#tblCashier').dataTable().fnDestroy();
                initCardPayment();
            }

            $('#storeReportDatatable>tbody').delegate('tr[role=row]', 'click', function (e) {
                e.preventDefault();
                var id = $(this)[0].id;
                var storeData = storeChartData[id];
                if (storeData.finalRevenue > 0) {
                    $('#storeReportDatatable>tbody>tr[role=row]').closest('.bgm-wheat').removeClass('bgm-wheat');
                    $(this).addClass('bgm-wheat');
                    $('#storeContainer').removeClass('col-lg-12');
                    $('#storeContainer').addClass('col-lg-7');
                    $('#storeContainer').addClass('no-padding');
                    $('#storeContainer').removeClass('col-md-12');
                    $('#storeContainer').addClass('col-md-7');
                    $('#storeChartContainer').show();
                    setupStorePieChart(storeData);
                    $('.dataTables_scrollBody').scrollLeft(70);
                    //fnHideCol('#storeReportDatatable', 0);
                    refreshStoreDatatable();
                }
            });

            $('div.container').on('click', function (e) {
                if ((e.target).closest("#storeReportDatatable_wrapper") == null && (e.target).closest("#storePieChart") == null) {
                    if ($('#storeContainer').hasClass('col-lg-7')) {
                        $('#storeContainer').removeClass('col-lg-7');
                        $('#storeContainer').removeClass('no-padding');
                        $('#storeContainer').addClass('col-lg-12');
                        $('#storeContainer').removeClass('col-md-7');
                        $('#storeContainer').addClass('col-md-12');
                        $('#storeChartContainer').hide();
                        //fnShowCol('#storeReportDatatable', 0);
                        $('#storeReportDatatable>tbody>tr[role=row]').closest('.bgm-wheat').removeClass('bgm-wheat');
                        refreshStoreDatatable();
                    //} else {
                    //    $('#storeReportDatatable>tbody>tr[role=row]').closest('.bgm-wheat').removeClass('bgm-wheat');

                    }
                }
            });

            //$(document).on('click', '#exportExcelProduct', function () {
            //    if (domain == 'Date') {
            //        $('#startTimeProduct').val($('#sTime').val());
            //        $('#endTimeProduct').val($('#eTime').val());
            //        $('#formProductExportExcel').submit();
            //    }
            //    else {
            //        $('#startTimeMonthProduct').val($('#sTime').val());
            //        $('#endTimeMonthProduct').val($('#eTime').val());
            //        $('#formProductMonthExportExcel').submit();
            //    }
            //});

            //$(document).on('click', '#exportExcelCashier', function () {
            //    if (domain == 'Date') {
            //        $('#startTimeCashier').val($('#sTime').val());
            //        $('#endTimeCashier').val($('#eTime').val());
            //        $('#formCashierExportExcel').submit();
            //    }
            //    else {
            //        $('#startTimeMonthCashier').val($('#sTime').val());
            //        $('#endTimeMonthCashier').val($('#eTime').val());
            //        $('#formCashierMonthExportExcel').submit();
            //    }
            //});

            $.ajax({
                url: '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + '/PaymentData',
                type: 'POST',
                data: { _startDate: $('#sTime').val(), _endDate: $('#eTime').val(), brandId: parseInt($('#hiddenBrandId').val()) },
                success: function (rs) {
                    //console.log(rs);
                    if (rs.success) {
                        $('#numberCashPayment').html(rs.cardPaymentQuantity.numberCashPayment - rs.cardPaymentQuantity.numberCashPaymentOrderCard);
                        $('#numberCashPaymentOrderCard').html(rs.cardPaymentQuantity.numberCashPaymentOrderCard);
                        $('#numberMemberPayment').html(rs.cardPaymentQuantity.numberMemberPayment);
                        $('#numberMasterCardPayment').html(rs.cardPaymentQuantity.numberMasterCardPayment);
                        $('#numberVisaCardPayment').html(rs.cardPaymentQuantity.numberVisaCardPayment);
                        $('#numberMomoPayment').html(rs.cardPaymentQuantity.numberMomoPayment);
                        $('#numberOtherPayment').html(rs.cardPaymentQuantity.numberOtherPayment);
                        $('#numberTotalPayment').html(rs.cardPaymentQuantity.numberTotalPayment);
                        
                        $('#totalCashPayment').html(numberWithSeparator((rs.cardPaymentQuantity.totalCashPayment)));
                        $('#totalCashPaymentOrderCard').html(numberWithSeparator(rs.cardPaymentQuantity.totalCashPaymentOrderCard));
                        $('#totalMemberPayment').html(numberWithSeparator(rs.cardPaymentQuantity.totalMemberPayment));
                        $('#totalMasterCardPayment').html(numberWithSeparator(rs.cardPaymentQuantity.totalMasterCardPayment));
                        $('#totalVisaCardPayment').html(numberWithSeparator(rs.cardPaymentQuantity.totalVisaCardPayment));
                        $('#totalMomoPayment').html(numberWithSeparator(rs.cardPaymentQuantity.totalMomoPayment));
                        $('#total-Payment-Bank').html(numberWithSeparator(rs.cardPaymentQuantity.totalVisaCardPayment + rs.cardPaymentQuantity.totalMasterCardPayment));
                        $('#totalOtherPayment').html(numberWithSeparator(rs.cardPaymentQuantity.totalOtherPayment));
                        $('#totalPayment').html(numberWithSeparator(rs.cardPaymentQuantity.totalPayment));
                        
                    } else {
                        ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
                    }
                },
                error: function (err) {
                    ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
                }
            });
        });




        $(".bhoechie-tab-menu>.list-group a").on("click", function (e) {
            e.preventDefault();
            if (!$(this).hasClass("active")) {
                $(this).parent().siblings().children("a.active").removeClass("active");
                $(this).addClass("active");
                var index = $(this).parent().index();
                $("div.bhoechie-tab>div.bhoechie-tab-content").removeClass("active");
                $("div.bhoechie-tab>div.bhoechie-tab-content").eq(index).addClass("active");
            }
        });

        
        



        //if (StoreId == 0) { //Thêm tab cửa hàng khi load dữ liệu theo brand
        //    $("#storeTab").show();
        //    refreshStoreDatatable();
        //} else {
        //    $("#storeTab").hide();
        ////}
        //$('.chartOptionActive').removeClass('chartOptionActive');
        //$('.chartOption').first().addClass('chartOptionActive');


        //$(document).on('click', '.chartOption', function () {
        //    $('.chartOptionActive').removeClass('chartOptionActive');
        //    $(this).addClass('chartOptionActive');
        //    getChartData($(this).text());
        //    //GetData();
        //});


    }

    //Binding event for tab
    $('#productTab').on('click', function (e) {
        e.preventDefault();
        if (isProductInit == false) {
            isProductInit = true;
            //$('#tblProduct').dataTable().fnDestroy();
            initProductDatatable();
        }
    });

    $('#storeTab').on('click', function (e) {
        e.preventDefault();
        if (isStoreInit == false) {
            initStoreDatatable();
            isStoreInit = true;
        } else {
            refreshStoreDatatable();
        }
    });

    $('#staffTab').on('click', function (e) {
        e.preventDefault();
        if (isCashierInit == false) {
            //$('#tblCashier').dataTable().fnDestroy();
            initCashierDatatable();
            isCashierInit = true;
        }
    });

    $('#cardPaymentTab').on('click', function (e) {
        e.preventDefault();
        if (isPaymentInit == false) {
            isPaymentInit = true;
            //$('#tblCashier').dataTable().fnDestroy();
            initCardPayment();
        }
    });
    //./Binding event for tab


    /*
     * author: TrungNDT
     * method: 
     */
    var setupMonthPicker = function () {
        $('#datetimepicker1').datetimepicker({
            viewMode: 'months',
            format: 'MM/YYYY',
            defaultDate: moment(),
            maxDate: moment()

        });

        $('#datetimepicker2').datetimepicker({
            viewMode: 'months',
            format: 'MM/YYYY',
            defaultDate: moment(),
            maxDate: moment()

        });
    }



    /*
     * author: TrungNDT
     * method: 
     */
    //var setupMonthPicker = function () {
    //    function cb(start, end, label) {
    //        var startTime = start.format("DD/MM/YYYY"),
    //            endTime = end.format("DD/MM/YYYY"),
    //            dateString = "(" + startTime + (startTime == endTime ? "" : " - " + endTime) + ")";

    //        //if (label != "Tùy chọn") {
    //        //    $('#date-string').val(label);
    //        //} else {
    //        $("#date-string").val(dateString);
    //        //}

    //        $("#sTime").val(startTime);
    //        $("#eTime").val(startTime);
    //        $("#date-string").html(dateString);
    //        $("#dateRange").html(dateString);
    //        $('#startDate').val(startTime);
    //        $('#endDate').val(startTime);
    //    }
    //    $('#monthpicker').datepicker({
    //        changeYear: true,
    //        showButtonPanel: true,
    //        format: 'yyyy',
    //        viewMode: "month",
    //        orientation: "left",
    //        endDate: new Date(),
    //        autoclose: true,
    //        minViewMode: "month",
    //    }, cb);
    //    //$(".date-picker-year").focus(function () {
    //    //    $(".ui-datepicker-month").hide();
    //    //});
    //}

    var setupDatePicker = function () {
        // Daterange picker
        function cb(start, end, label) {
            var startTime = start.format("DD/MM/YYYY"),
                endTime = end.format("DD/MM/YYYY"),
                dateString = "(" + startTime + (startTime == endTime ? "" : " - " + endTime) + ")",
                //dateRange = "(" + startTime + (startTime == moment().format("DD/MM/YYYY") ? " " + moment().format("HH:mm") : "") + ")";
                dateRange = startTime;
                timeRange = moment().format("HH:mm");
            //if (label != "Tùy chọn") {
            //    $('#date-string').val(label);
            //} else {
            $("#date-string").val(dateString);
            //}

            $("#sTime").val(startTime);
            $("#eTime").val(startTime);
            $("#date-string").html(dateString);
            var titleTmp = "Tổng quan doanh thu ngày " + dateRange;
            $("#title-date").text(titleTmp);
            if (startTime == moment().format("DD/MM/YYYY")) {
                $("#dateRange").text("Tính đến " + timeRange);
            } else {
                $("#dateRange").text("Tính đến 23:59");
            }
            $('#startDate').val(startTime);
            $('#endDate').val(startTime);
        }
        cb(moment(), moment(), "Hôm nay");

        //$('#reportrange').datepicker({
        //            changeYear: true,
        //            showButtonPanel: true,
        //            format: 'yyyy',
        //            viewMode: "month",
        //            orientation: "left",
        //            endDate: new Date(),
        //            autoclose: true,
        //            minViewMode: "month",
        //        }, cb);

        $('#reportrange').daterangepicker({
            "applyClass": "btn-primary",
            "cancelClass": "btn-success",
            "singleDatePicker": true,
            "opens": "left",
            "maxDate": moment(),
            locale: {
                format: 'DD/MM/YYYY',
            },
            //ranges: {
            //    'Hôm nay': [moment()],
            //    'Hôm qua': [moment().subtract(1, 'days')],
            //    'Tuần này': [moment().startOf('isoweek'), moment().endOf('isoweek')],
            //    'Tuần trước': [moment().subtract(1, 'week').startOf('isoweek'), moment().subtract(1, 'week').endOf('isoweek')],
            //    'Tháng này': [moment().startOf('month'), moment().endOf('month')],
            //    'Tháng trước': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
            //}
        }, cb);




        //function cb(start, end, label) {
        //    if (label != "Tùy chọn") {
        //        $('#date-string').val(label);
        //    } else if (start.format('DD/MM/YYYY') == end.format('DD/MM/YYYY')) {
        //        $('#date-string').val(start.format('DD/MM/YYYY'));
        //    } else {
        //        $('#date-string').val(start.format('DD/MM/YYYY') + ' - ' + end.format('DD/MM/YYYY'));
        //    }
        //    $('#startDate').val(start.format('DD/MM/YYYY'));
        //    $('#endDate').val(end.format('DD/MM/YYYY'));
        //}
        //cb(moment(), moment(), 'Hôm nay');

        //$('#reportrange').daterangepicker({
        //    "opens": "left",
        //    "maxDate": moment(),
        //    locale: {
        //        format: 'DD/MM/YYYY'
        //    },
        //    ranges: {
        //        'Hôm nay': [moment(), moment()],
        //        'Hôm qua': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        //        'Tuần này': [moment().startOf('isoweek'), moment().endOf('isoweek')],
        //        'Tuần trước': [moment().subtract(1, 'week').startOf('isoweek'), moment().subtract(1, 'week').endOf('isoweek')],
        //        'Tháng này': [moment().startOf('month'), moment().endOf('month')],
        //        'Tháng trước': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        //    }
        //}, cb);

    }

    /*
     * author: TrungNDT
     * method: get selected month range and render statistic within those range
     */
    var renderStatisticByMonth = function () {
        var startStr = $('#startDate').val(),
            endStr = $('#endDate').val(),
            startDate, endDate,
            labelStr,
            option = 'Tổng doanh thu';

        if (Domain == 'Month') {
            var endMonth = endStr.split('/')[0],
                endYear = endStr.split('/')[1];

            startDate = '01/' + startStr,
            //tmpEndDate = (new Date(endYear, endMonth, 0).getDate()) + '/' + endStr;
            today = moment().format('DD/MM/YYYY').toString();
            if (endStr == today.substring(3, 10)) {
                endDate = today.substring(0, 3) + endStr;
            } else {
                endDate = (new Date(endYear, endMonth, 0).getDate()) + '/' + endStr;
            }
            //alert(today.substring(3, 10));
            StartDate = startDate;
            EndDate = endDate;



        } else {
            //var dateStr = $('#date-string').val();
            //if (dateStr.toString().indexOf('/') > 0) {
            //    labelStr = 'Tổng quan ngày ' + dateStr;
            //} else {
            //    labelStr = 'Tổng quan ' + dateStr.toString().toLowerCase() + ' (' + startStr + (startStr !== endStr ? (' - ' + endStr) : '') + ')';
            //}

            startDate = startStr;
            endDate = endStr;

            StartDate = startDate;
            EndDate = endDate;
            //var dateRange = "(" + startDate + (startDate == moment().format("DD/MM/YYYY") ? " " + moment().format("HH:mm") : "") + ")";
            dateRange = startDate;
            timeRange = moment().format("HH:mm");
            //$('#dateRange').html(dateRange);
            var titleTmp = "Tổng quan doanh thu ngày "  + dateRange;
            $("#title-date").text(titleTmp);
            if (startDate == moment().format("DD/MM/YYYY")) {
                $("#dateRange").text("Tính đến " + timeRange);
            } else {
                $("#dateRange").text("Tính đến 23:59");
            }
            
        }

        //$('#date').html(labelStr);

        $('#sTime').val(startDate);
        $('#eTime').val(endDate);
        



        var storeId = parseInt($('#hiddenStoreId').val());
        $.ajax({
            url: '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + '/DateData',
            type: 'GET',
            data: { storeId: storeId, _startDate: StartDate, _endDate: EndDate },
            success: function (rs) {
                //console.log(rs);
                if (rs.success) {


                    //Doanh thu tung loai hoa don                    
                    var finalRevenue = rs.finalRevenue;
                    $('#finalRevenue').html(numberWithSeparator(finalRevenue.finalAmount - finalRevenue.finalOrderCard));
                    $('#revenueAtStore').html(numberWithSeparator(finalRevenue.finalAtStore));
                    $('#revenueDelivery').html(numberWithSeparator(finalRevenue.finalDelivery));
                    $('#revenueTakeAway').html(numberWithSeparator(finalRevenue.finalTakeAway));

                    


                    //Doanh thu sau giam gia
                    var discountRevenue = rs.discountRevenue;
                    $('#totalRevenue').html(numberWithSeparator(discountRevenue.totalAmount ));
                    $('#tongDoanhThu').html(numberWithSeparator(discountRevenue.totalAmount - discountRevenue.finalAmountCard) );
                    $('#tongGiamGia').html(numberWithSeparator(discountRevenue.totalDiscount));
                    $('#sauGiamGiaTienMat').html(numberWithSeparator(discountRevenue.finalAmountCash));
                    $('#sauGiamGiaThe').html(numberWithSeparator(discountRevenue.finalAmountCard));

                    //So luong Hoá đơn
                    var receiptQty = rs.receipt.receiptQty;
                    $('#tongHoaDoa').html(numberWithSeparator(receiptQty.totalReceipt));
                    $('#atStore').html(numberWithSeparator(receiptQty.qtyAtStore));
                    $('#takeAway').html(numberWithSeparator(receiptQty.qtyDelivery));
                    $('#delivery').html(numberWithSeparator(receiptQty.qtyTakeAway));

                    //Tổng số hóa đơn
                    $('#finalTotalBill').html(receiptQty.totalReceipt - receiptQty.qtyOrderCard);

                    $('#finalTotalBillAtStore').html(receiptQty.qtyAtStore);

                    $('#finalTotalBillDelivery').html(receiptQty.qtyDelivery);

                    $('#finalTotalBillTakeAway').html(receiptQty.qtyTakeAway);

                    // Area TotalBill
                    $('#totalBillOrder').html(numberWithSeparator(receiptQty.totalReceipt));
                    $('#totalBillAtStore').html(numberWithSeparator(receiptQty.qtyAtStore));
                    $('#totalBillDelivery').html(numberWithSeparator(receiptQty.qtyDelivery));
                    $('#totalBillTakeAway').html(numberWithSeparator(receiptQty.qtyTakeAway));
                    $('#totalBillCardPayment').html(numberWithSeparator(receiptQty.qtyOrderCard));
                    //
                    //console.log(dataBill);

                    //Bình quân giá trị hóa đơn
                    var receiptAvg = rs.receipt.receiptAvg;
                    $('#avgFinalReceipt').html(numberWithSeparator(receiptAvg.avgFinalReceipt.toFixed(0)));
                    $('#avgFinalAtStore').html(numberWithSeparator(receiptAvg.avgFinalAtStore.toFixed(0)));
                    $('#avgFinalDelivery').html(numberWithSeparator(receiptAvg.avgFinalDelivery.toFixed(0)));
                    $('#avgFinalTakeAway').html(numberWithSeparator(receiptAvg.avgFinalTakeAway.toFixed(0)));

                    //Hoa don huy
                    var canceledReceipt = rs.canceledReceipt
                    $('#totalPreCancel').html(numberWithSeparator(canceledReceipt.qtyReceiptPreCancel));
                    $('#totalCancel').html(numberWithSeparator(canceledReceipt.qtyReceiptCancel));
                    $('#totalOrderCancel').html(numberWithSeparator(canceledReceipt.qtyReceiptTotalCancel));
                    $('#finalReceiptPreCancel').html(numberWithSeparator(canceledReceipt.finalReceiptPreCancel));
                    $('#finalReceiptCancel').html(numberWithSeparator(canceledReceipt.finalReceiptCancel));
                    $('#finalReceiptTotalCancel').html(numberWithSeparator(canceledReceipt.finalReceiptTotalCancel));

                    $("#finalAVGPreCancel").html(canceledReceipt.finalReceiptPreCancel + "/" + canceledReceipt.qtyReceiptPreCancel);
                    $("#finalAVGAfterCancel").html(canceledReceipt.finalReceiptCancel + "/" + canceledReceipt.qtyReceiptCancel);
                    //console.log(dataAmount);                    

                    //Số lượng sản phẩm theo loại hóa đơn
                    var productQty = rs.product.productQty;

                    //Bình quân số lượng sản phẩm mỗi hóa đơn
                    var productAvg = rs.product.productAvg;
                    $('#avgTotalProductPerReceipt').html(numberWithSeparator(productAvg.avgTotalProductPerReceipt.toFixed(1)));
                    $('#avgProductAtStoretPerReceipt').html(numberWithSeparator(productAvg.avgProductAtStorePerReceipt.toFixed(1)));
                    $('#avgProductDeliveryPerReceipt').html(numberWithSeparator(productAvg.avgProductDeliveryPerReceipt.toFixed(1)));
                    $('#avgProductTakeAwayPerReceipt').html(numberWithSeparator(productAvg.avgProductTakeAwayPerReceipt.toFixed(1)));
                    $('#avgProductTakeAwayPerReceipt').html(numberWithSeparator(productAvg.avgProductTakeAwayPerReceipt.toFixed(1)));

                    var cardPaymentQuantity = rs.cardPaymentQuantity;

                    //$('#pieChart').remove();
                    setupBillPieChart(receiptQty);
                    setupRevenuePieChart(finalRevenue);                    

                    //Hình thúc thanh toán
                    //dataPayment = rs.dataPayment;
                    //$('#tongThu').html(numberWithSeparator(dataPayment.payment));
                    //$('#tienMat').html(numberWithSeparator(dataPayment.paymentCash));
                    //$('#theThanhVien').html(numberWithSeparator(dataPayment.paymentUserCard));
                    //$('#theTinDung').html(numberWithSeparator(dataPayment.paymentCreditCard));

                    //if (startStr !== endStr) {
                    //    $('#avgTotalAmount').show();
                    //    $('#avgTotalAmount strong').html(numberWithSeparator((dataAmount.totalAmount / dataPayment.totalPaymentList.length).toFixed(0)))

                    //    $('#avgTotalDiscount').show();
                    //    $('#avgTotalDiscount strong').html(numberWithSeparator((dataAmount.totalDiscount / dataPayment.totalPaymentList.length).toFixed(0)))

                    //    $('#avgFinalAmountCash').show();
                    //    $('#avgFinalAmountCash strong').html(numberWithSeparator((dataAmount.finalAmount / dataPayment.totalPaymentList.length).toFixed(0)))
                    //} else {
                    //    $('#avgTotalAmount').hide();
                    //    $('#avgTotalDiscount').hide();
                    //    $('#avgFinalAmountCash').hide();
                    //}

                    //Phần trăm các hình thức
                    //var cashPercentage, userCardPercentage, creditCardPercentage;
                    //if (dataPayment.payment > 0) {
                    //    cashPercentage = (dataPayment.paymentCash / dataPayment.payment) * 100;
                    //    userCardPercentage = (dataPayment.paymentUserCard / dataPayment.payment) * 100;
                    //    creditCardPercentage = (dataPayment.paymentCreditCard / dataPayment.payment) * 100;
                    //}
                    // Real code here
                    //$('#phanTramTienMat').data('percent', cashPercentage.toFixed(0))
                    //    .children().html(cashPercentage.toFixed(0));
                    //$('#phanTramTheThanhVien').data('percent', userCardPercentage.toFixed(0))
                    //    .children().html(userCardPercentage.toFixed(0));
                    //$('#phanTramTheTinDung').data('percent', creditCardPercentage.toFixed(0))
                    //    .children().html(creditCardPercentage.toFixed(0));

                    // Sample data for testing
                    //$('#phanTramTienMat').data('percent', 44)
                    //    .children().html(44);
                    //$('#phanTramTheThanhVien').data('percent', 27)
                    //    .children().html(27);
                    //$('#phanTramTheTinDung').data('percent', 29)
                    //    .children().html(29);
                    //setupPercentPiechart();

                    //$('#phanTramTienMat').html(cashPercentage.toFixed(2) + '%');
                    //$('#phanTramTheThanhVien').html(userCardPercentage.toFixed(2) + '%');
                    //$('#phanTramTheTinDung').html(creditCardPercentage.toFixed(2) + '%');

                    //$('#tienMatPB').css('width', cashPercentage.toFixed(2) + '%');
                    //$('#theThanhVienPB').css('width', userCardPercentage.toFixed(2) + '%');
                    //$('#theTinDungPB').css('width', creditCardPercentage.toFixed(2) + '%');

                    //Tổng thu chart
                    //var $incomeChart = $('#tongThuChart');
                    if (Domain == 'Month' && rs.dateList.length >= 2) {
                        DateList = rs.dateList;
                        //if (Domain == 'Month') {
                        //    MonthList = rs.monthList;
                        //}
                        //$incomeChart.show();

                        //var firstColText = $('[data-role=unit]').eq(0).text();
                        //$('#tongThuChart .widget-box .widget-body .widget-main').html('<canvas id="lineChart" style="width: 100%; height: 300px;"></canvas>');
                        //$('#storeReportDatatableArea').html('<table id="storeReportDatatable" class="table table-striped table-bordered table-hover" style="width: 100% !important">'
                        //+ '<thead style="white-space: nowrap; text-overflow: ellipsis;"><tr><th rowspan="2" style="text-align: center">'
                        //+ firstColText.charAt(0).toUpperCase() + firstColText.slice(1)
                        //+ '</th><th colspan="3" style="text-align: center">Doanh thu</th><th colspan="4" style="text-align: center">Hóa đơn</th></tr><tr><th>'
                        //+ '<label class="">Tổng doanh thu</label></th><th>'
                        //+ '<label class="">Giảm giá</label></th><th><label class="">Doanh thu sau giảm giá</label></th><th><label class="">Hóa đơn (AtStore)</label>'
                        //+ '</th><th><label class="">Hóa đơn (Delivery)</label></th><th><label class="">Hóa đơn (TakeAway)</label></th><th><label class="">Tổng hóa đơn</label>'
                        //+ '</th></tr></thead><tbody></tbody><tfoot></tfoot></table>');

                        //Bình quân doanh thu sau khuyến mãi
                        $('#avgTotalAmount').show();
                        $('#avgTotalAmount strong').html(numberWithSeparator((discountRevenue.totalAmount / DateList.length).toFixed(0)))

                        $('#avgTotalDiscount').show();
                        $('#avgTotalDiscount strong').html(numberWithSeparator((discountRevenue.totalDiscount / DateList.length).toFixed(0)))

                        $('#avgFinalAmountCash').show();
                        $('#avgFinalAmountCash strong').html(numberWithSeparator((discountRevenue.finalAmountCash / DateList.length).toFixed(0)))

                        $('#avgFinalAmountCard').show();
                        $('#avgFinalAmountCard strong').html(numberWithSeparator((discountRevenue.finalAmountCard / DateList.length).toFixed(0)))

                        //Bình quân hóa đơn hủy
                        $('#avgTotalPreCancel').show();
                        $('#avgTotalPreCancel strong').html(numberWithSeparator((canceledReceipt.qtyReceiptPreCancel / DateList.length).toFixed(0)))

                        $('#avgTotalCancel').show();
                        $('#avgTotalCancel strong').html(numberWithSeparator((canceledReceipt.qtyReceiptCancel / DateList.length).toFixed(0)))

                        $('#avgTotalOrderCancel').show();
                        $('#avgTotalOrderCancel strong').html(numberWithSeparator((canceledReceipt.qtyReceiptTotalCancel / DateList.length).toFixed(0)))

                        //Bình quân doanh thu theo từng loại
                        $('#avgFinalRevenue').show();
                        $('#avgfinalRevenue strong').html(numberWithSeparator((finalRevenue.finalAmount - finalRevenue.finalOrderCard / DateList.length).toFixed(0)));

                        $('#avgRevenueAtStore').show();
                        $('#avgRevenueAtStore strong').html(numberWithSeparator((finalRevenue.finalAtStore / DateList.length).toFixed(0)));

                        $('#avgRevenueDelivery').show();
                        $('#avgRevenueDelivery strong').html(numberWithSeparator((finalRevenue.finalDelivery / DateList.length).toFixed(0)));

                        $('#avgRevenueTakeAway').show();
                        $('#avgRevenueTakeAway strong').html(numberWithSeparator((finalRevenue.finalTakeAway / DateList.length).toFixed(0)));


                       

                        //getChartData(option);

                        $('#monthOverviewChartContainer').show();
                        var monthOverviewList = rs.monthOverviewList;
                        //var monthReceipt = monthOverviewList.receiptMonth;
                        //var monthRevenue = monthOverviewList.revenueMonth;
                        //var monthProduct = monthOverviewList.productMonth;
                        overviewMonthChart(monthOverviewList.receiptQtyTotalList, monthOverviewList.revenueFinalList, rs.dateList);
                        //revenueMonthChart(monthRevenue, finalRevenue.finalAmount);
                        //receiptMonthChart(monthReceipt, receiptQty.totalReceipt);
                        //productMonthChart(monthProduct, productQty.qtyTotalProduct);

                    } else {
                        $('#monthOverviewChartContainer').hide();
                        //$incomeChart.hide();

                        //setupProductPieChart(productQty);

                        $('#avgTotalAmount').hide();
                        $('#avgTotalDiscount').hide();
                        $('#avgFinalAmountCash').hide();
                        $('#avgFinalAmountCard').hide();
                        $('#avgFinalRevenue').hide();
                        $('#avgRevenueAtStore').hide();
                        $('#avgRevenueDelivery').hide();
                        $('#avgRevenueTakeAway').hide();

                    }
                    //$('#loading-image').fadeOut(400, function () {
                    //    $(this).trigger('onFadeOutComplete');
                    //});

                } else {
                    ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
                }
            }
        });
    }

    /*
     * author: HungNM, TrungNDT
     * method: 
     */
    var setupRevenuePieChart = function (dataRevenue) {
        $('#revenuePieChart').highcharts({
            chart: {
                type: 'pie'
            },
            title: {
                text: 'Doanh thu thực tế'
            },
            subtitle: {
                text: 'Tổng số: <b>' + numberWithSeparator(dataRevenue.finalAmount - dataRevenue.finalOrderCard) + '</b> VNĐ'
            },
            tooltip: {
                headerFormat: '',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>:<br/>Doanh thu: <b>{point.y}</b> VNĐ'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true,
                }
            },
            series: [{
                name: 'Doanh thu thực tế',
                colorByPoint: true,
                data: [{
                    name: 'Tại cửa hàng',
                    y: dataRevenue.finalAtStore,
                    color: 'rgba(200, 81, 81, 1)'
                }, {
                    name: 'Giao hàng',
                    y: dataRevenue.finalDelivery,
                    color: 'rgba(204, 155, 30, 1)'
                }, {
                    name: 'Mang về',
                    y: dataRevenue.finalTakeAway,
                    color: 'darkolivegreen'
                },
                //{
                //    name: 'Nạp thẻ',
                //    y: dataRevenue.finalOrderCard,
                //    color: 'navy'
                //}
                ]
            }]

        });
    }

    var setupOrderCardPieChart = function (cardPaymentQuantity) {
        $('#orderCardPieChart').highcharts({
            chart: {
                type: 'pie'
            },
            title: {
                text: 'Loại hình thanh toán'
            },
            subtitle: {
                text: 'Tổng số: <b>' + numberWithSeparator(cardPaymentQuantity.numberTotalPayment - cardPaymentQuantity.numberCashPaymentOrderCard) + '</b> giao dịch'
            },
            tooltip: {
                headerFormat: '',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>:<br/>Doanh thu: <b>{point.y}</b> giao dịch'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true,
                }
            },
            series: [{
                name: 'Giao dịch',
                colorByPoint: true,
                data: [{
                    name: 'Tiền mặt',
                    y: cardPaymentQuantity.numberCashPayment,
                    color: 'rgba(200, 81, 81, 1)'
                }, {
                    name: 'Thẻ thành viên',
                    y: cardPaymentQuantity.numberMemberPayment,
                    color: 'rgba(204, 155, 30, 1)'
                }, {
                    name: 'Ngân Hàng',
                    y: cardPaymentQuantity.numberMasterCardPayment + cardPaymentQuantity.numberVisaCardPayment,
                    color: 'darkolivegreen'
                }, {
                    name: 'Third Party',
                    y: cardPaymentQuantity.numberMomoPayment, // Tổng hợp loại thanh toán từ third party 
                    color: '#E91E63'
                }, {
                    name: 'Khác',
                    y: cardPaymentQuantity.numberOtherPayment,
                    color: '#00cc66'
                }
                ]
            }]

        });
    }

    var setupOrderCardRevenuePieChart = function (cardPaymentQuantity) {
        $('#orderCardRevenuePieChart').highcharts({
            chart: {
                type: 'pie'
            },
            title: {
                text: 'Lượng tiền thanh toán'
            },
            subtitle: {
                text: 'Tổng số: <b>' + numberWithSeparator(cardPaymentQuantity.totalPayment - cardPaymentQuantity.totalCashPaymentOrderCard) + '</b> VND'
            },
            tooltip: {
                headerFormat: '',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>:<br/>Doanh thu: <b>{point.y}</b> VND'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true,
                }
            },
            series: [{
                name: 'Giao dịch',
                colorByPoint: true,
                data: [{
                    name: 'Tiền mặt',
                    y: cardPaymentQuantity.totalCashPayment,
                    color: 'rgba(200, 81, 81, 1)'
                }, {
                    name: 'Thẻ thành viên',
                    y: cardPaymentQuantity.totalMemberPayment,
                    color: 'rgba(204, 155, 30, 1)'
                }, {
                    name: 'Ngân Hàng',
                    y: cardPaymentQuantity.totalMasterCardPayment + cardPaymentQuantity.totalVisaCardPayment,
                    color: 'darkolivegreen'
                }, {
                    name: 'Third Party',
                    y: cardPaymentQuantity.totalMomoPayment,
                    color: '#E91E63'
                },
                {
                    name: 'Khác',
                    y: cardPaymentQuantity.totalOtherPayment,
                    color: '#00cc66'
                }]
            }]

        });
    }

    var setupBillPieChart = function (dataBill) {
        //$('#pieChartArea').html('<canvas id="pieChart"></canvas>');

        //dataBill = {
        //    totalBillAtStore: 45000,
        //    totalBillTakeAway: 120000,
        //    totalBillDelivery: 90000
        //}

        //HighCharts
        $('#billPieChart').highcharts({
            chart: {
                type: 'pie'
            },
            title: {
                text: 'Hóa đơn bán hàng'
            },
            subtitle: {
                text: 'Tổng hóa đơn bán hàng: <b>' + (dataBill.totalReceipt - dataBill.qtyOrderCard) + '</b>'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true
                }
            },

            tooltip: {
                headerFormat: '',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>:<br/>Số lượng: <b>{point.y}</b>'
            },
            series: [{
                name: 'Hóa đơn bán hàng',
                colorByPoint: true,
                data: [{
                    name: 'Tại cửa hàng',
                    y: dataBill.qtyAtStore,
                    color: 'rgba(200, 81, 81, 1)'
                }, {
                    name: 'Giao hàng',
                    y: dataBill.qtyDelivery,
                    color: 'rgba(204, 155, 30, 1)'
                }, {
                    name: 'Mang về',
                    y: dataBill.qtyTakeAway,
                    color: 'darkolivegreen'
                },
                //{
                //    name: 'Nạp thẻ',
                //    y: dataBill.qtyOrderCard,
                //    color: 'navy'
                //}
                ]
            }]

        });
    }
    //ChartJS
    //var pieChartCanvas = $("#pieChart").get(0).getContext("2d");
    //var pieData = [
    //  {
    //      value: dataBill.totalBillAtStore,
    //      color: "#f56954",
    //      highlight: "#f56954",
    //      label: "AtStore"
    //  },
    //  {
    //      value: dataBill.totalBillTakeAway,
    //      color: "#00a65a",
    //      highlight: "#00a65a",
    //      label: "TakeAway"
    //  },
    //  {
    //      value: dataBill.totalBillDelivery,
    //      color: "#f39c12",
    //      highlight: "#f39c12",
    //      label: "Delivery"
    //  },
    //];
    //var pieOptions = {
    //    //Boolean - Whether we should show a stroke on each segment
    //    segmentShowStroke: true,
    //    //String - The colour of each segment stroke
    //    segmentStrokeColor: "#fff",
    //    //Number - The width of each segment stroke
    //    segmentStrokeWidth: 2,
    //    //Number - The percentage of the chart that we cut out of the middle
    //    percentageInnerCutout: 60, // This is 0 for Pie charts
    //    //Number - Amount of animation steps
    //    animationSteps: 100,
    //    //String - Animation easing effect
    //    animationEasing: "easeOutBounce",
    //    //Boolean - Whether we animate the rotation of the Doughnut
    //    animateRotate: true,
    //    //Boolean - Whether we animate scaling the Doughnut from the centre
    //    animateScale: false,
    //    //Boolean - whether to make the chart responsive to window resizing
    //    responsive: true,
    //    // Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
    //    maintainAspectRatio: true,
    //    //String - A legend template
    //    legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>",
    //    // Boolean - Determines whether to draw tooltips on the canvas or not
    //    showTooltips: true,
    //    // Function - Determines whether to execute the customTooltips function instead of drawing the built in tooltips (See [Advanced - External Tooltips](#advanced-usage-custom-tooltips))
    //    customTooltips: false,
    //    // Array - Array of string names to attach tooltip events
    //    tooltipEvents: ["mousemove", "touchstart", "touchmove"],
    //    // String - Tooltip background colour
    //    tooltipFillColor: "rgba(0,0,0,0.8)",
    //    // String - Tooltip label font declaration for the scale label
    //    tooltipFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",
    //    // Number - Tooltip label font size in pixels
    //    tooltipFontSize: 14,
    //    // String - Tooltip font weight style
    //    tooltipFontStyle: "normal",
    //    // String - Tooltip label font colour
    //    tooltipFontColor: "#fff",
    //    // String - Tooltip title font declaration for the scale label
    //    tooltipTitleFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",
    //    // Number - Tooltip title font size in pixels
    //    tooltipTitleFontSize: 14,
    //    // String - Tooltip title font weight style
    //    tooltipTitleFontStyle: "bold",
    //    // String - Tooltip title font colour
    //    tooltipTitleFontColor: "#fff",
    //    // Number - pixel width of padding around tooltip text
    //    tooltipYPadding: 6,
    //    // Number - pixel width of padding around tooltip text
    //    tooltipXPadding: 6,
    //    // Number - Size of the caret on the tooltip
    //    tooltipCaretSize: 8,
    //    // Number - Pixel radius of the tooltip border
    //    tooltipCornerRadius: 6,
    //    // Number - Pixel offset from point x to tooltip edge
    //    tooltipXOffset: 10,
    //    // String - Template string for single tooltips
    //    tooltipTemplate: function (label) { return label.label + ": " + numberWithSeparator(label.value.toString()) },
    //    // String - Template string for multiple tooltips
    //    //multiTooltipTemplate: function (label) { return label.datasetLabel + ": " + numberWithSeparator(label.value.toString()) },
    //};
    ////Create pie or douhnut chart
    //// You can switch between pie and douhnut using the method below.
    //var pieChart = new Chart(pieChartCanvas).Doughnut(pieData, pieOptions);
    //console.log(pieChart);
    //$('#pieChart-legend').html(pieChart.generateLegend());
    var setupProductPieChart = function (dataProduct) {

        //HighCharts
        $('#productPieChart').highcharts({
            chart: {
                type: 'pie'
            },
            title: {
                text: 'Sản phẩm'
            },
            subtitle: {
                text: 'Tổng: <b>' + dataProduct.qtyTotalProduct + '</b>'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true,
                }
            },

            tooltip: {
                headerFormat: '',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>:<br/>Số lượng: <b>{point.y}</b>'
            },
            series: [{
                name: 'Sản phẩm',
                colorByPoint: true,
                data: [{
                    name: 'Tại cửa hàng',
                    y: dataProduct.qtyProductAtStore,
                    color: 'rgba(200, 81, 81, 1)'
                }, {
                    name: 'Giao hàng',
                    y: dataProduct.qtyProductDelivery,
                    color: 'rgba(204, 155, 30, 1)'
                }, {
                    name: 'Mang về',
                    y: dataProduct.qtyProductTakeAway,
                    color: 'darkolivegreen'
                }]
            }]

        });
    }

    var setupStorePieChart = function (storeData) {
        //HighCharts
        $('#storePieChart').highcharts({
            chart: {
                type: 'pie'
            },
            title: {
                text: 'Đóng góp doanh thu'
            },
            subtitle: {
                text: 'Cửa hàng ' + storeData.storeName
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true,
                }
            },

            tooltip: {
                headerFormat: '',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>:<br/>Doanh thu: <b>{point.y}</b> VNĐ'
            },
            series: [{
                name: 'Cửa hàng',
                colorByPoint: true,
                data: [{
                    name: 'Tại cửa hàng',
                    y: storeData.revenueAtStore,
                    color: 'rgba(200, 81, 81, 1)'
                }, {
                    name: 'Giao hàng',
                    y: storeData.revenueDelivery,
                    color: 'rgba(204, 155, 30, 1)'
                }, {
                    name: 'Mang về',
                    y: storeData.revenueTakeAway,
                    color: 'darkolivegreen'
                }]
            }]

        });
    }

    /*
     * author: HungNM
     * method: Write the chart out
     */
    var setupTotalIncomeChart = function (option, data) {
        //Delete the old one to avoid overlay
        //$('#tongThuChart .widget-box .widget-body .widget-main').html('<canvas id="lineChart" style="width: 100%; height: 300px;"></canvas>'
        //+ '<div id="barChart-legend" class="chart-legend"></div>');

        //Change the title of chart
        $('#chartTitle').html(option);

        //Determine xAxis label list
        var listLabel = [];
        if (Domain == 'Month') {
            listLabel = MonthList;
        } else {
            listLabel = DateList;
        }

        if (option == "Tổng doanh thu") {
            setUpIncomeChart(data, listLabel);
        } else {
            setUpBillStackedColumnChart(data, listLabel);
        }
    }

    /*
     * author: HungNM
     * method: Write the chart out
     */
    var setUpIncomeChart = function (data, listLabel) {
        //console.log(data);
        $('#lineChart').highcharts({
            title: {
                text: 'Doanh thu',
                x: -20 //center
            },
            //subtitle: {
            //    text: 'Source: WorldClimate.com',
            //    x: -20
            //},
            xAxis: {
                categories: listLabel
            },
            yAxis: {
                title: {
                    text: 'VND'
                },
                plotLines: [{
                    value: 0,
                    width: 1,
                    color: '#808080'
                }]
            },
            //tooltip: {
            //    valueSuffix: '°C'
            //},
            legend: {
                layout: 'vertical',
                align: 'right',
                verticalAlign: 'middle',
                borderWidth: 0
            },
            series: [{
                name: 'Tổng doanh thu',
                data: data.totalAmountList
            }, {
                name: 'Doanh thu sau giảm giá',
                data: data.finalAmountList
            }]
        });
    }

    /*
     * author: HungNM
     * method: Write the chart out
     */
    var setUpBillStackedColumnChart = function (data, listLabel) {
        $('#lineChart').highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: 'Hóa đơn'
            },
            xAxis: {
                categories: listLabel
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'Total fruit consumption'
                },
                stackLabels: {
                    enabled: true,
                    style: {
                        fontWeight: 'bold',
                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                    }
                }
            },
            legend: {
                align: 'right',
                x: -30,
                verticalAlign: 'top',
                y: 25,
                floating: true,
                backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
                borderColor: '#CCC',
                borderWidth: 1,
                shadow: false
            },
            tooltip: {
                headerFormat: '<b>{point.x}</b><br/>',
                pointFormat: '{series.name}: {point.y}'
            },
            plotOptions: {
                column: {
                    stacking: 'normal',
                    //dataLabels: {
                    //    enabled: true,
                    //    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                    //    style: {
                    //        textShadow: '0 0 3px black'
                    //    }
                    //}
                }
            },
            series: [{
                name: 'AtStore',
                data: data.totalBillAtStoreList
            }, {
                name: 'TakeAway',
                data: data.totalBillTakeAwayList
            }, {
                name: 'Delivery',
                data: data.totalBillDeliveryList
            }]
        });
    }

    /*
     * author: TrungNDT
     * method: 
     */
    var setupPercentPiechart = function () {
        $('.easy-pie-chart.percentage').each(function () {
            var $box = $(this).closest('.infobox');
            var barColor = $(this).data('color') || (!$box.hasClass('infobox-dark') ? $box.css('color') : 'rgba(255,255,255,0.95)');
            var trackColor = barColor == 'rgba(255,255,255,0.95)' ? 'rgba(255,255,255,0.25)' : '#E2E2E2';
            var size = parseInt($(this).data('size')) || 50;
            $(this).easyPieChart({
                barColor: barColor,
                trackColor: trackColor,
                scaleColor: false,
                lineCap: 'butt',
                lineWidth: parseInt(size / 10),
                animate: /msie\s*(8|7|6)/.test(navigator.userAgent.toLowerCase()) ? false : 1000,
                size: size
            });
        });
    }

    /*
     * author: HungNM
     * method: Get chart data then write it
     */
    var getChartData = function (option) {
        //var result = {};
        $.ajax({
            url: '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + '/ChartData',
            type: 'GET',
            data: { storeId: parseInt($('#hiddenStoreId').val()), _option: option, _dateList: JSON.stringify(DateList) },
            success: function (rs) {
                //console.log(rs);
                if (rs.success) {
                    setupTotalIncomeChart(option, rs.data);
                } else {
                    ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
                }
            }
        });
        //return result;
    };

    /*
     * author: HungNM
     * method: [SUPPORT]
     */
    var numberWithSeparator = function (x) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    };

    /*
     * author: HungNM
     * method: [Cashier]
     */
    var refreshCashierTable = function () {
        var oTable = $("#tblCashier").dataTable();
        oTable._fnPageChange(0);
        oTable._fnAjaxUpdate();
    }

    /*
     * author: HungNM
     * method: [Cashier]
     */
    var initCashierDatatable = function () {
        var storeId = parseInt($('#hiddenStoreId').val());
        var startMonth = $('#sTime').val();
        var endMonth = $('#eTime').val();
        //.substring(0, $('#endDate').val().indexOf('/'))
        var startDate = moment().subtract(moment().month() + 1 - startMonth, 'month').startOf('month').format('DD/MM/YYYY');
        var endDate = moment().subtract(moment().month() + 1 - endMonth, 'month').endOf('month').format('DD/MM/YYYY');
        $.fn.DataTable.ext.pager.numbers_length = 6;
        $("#tblCashier").datatablevpn({
            "bFilter": false,
            "bSort": false,
            "bRetrieve": false,
            "bDestroy": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "bDeferRender": true,
            "sAjaxSource": '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + "/CashierData",
            "bProcessing": true,
            "dom": '<l<t>p>',
            "iDisplayLength": 5,
            "aLengthMenu": [5, 15, 30, 50],
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "_startDate", "value": $('#sTime').val() });
                aoData.push({ "name": "_endDate", "value": $('#eTime').val() });
                aoData.push({ "name": "storeId", "value": storeId });
            },
            "oLanguage": {
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ ",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " ",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sNext": "<i class='fa fa-chevron-right'></i>",
                    "sPrevious": "<i class='fa fa-chevron-left'></i>"
                }

            },
            "aoColumnDefs": [
                //{
                //    "aTargets": [0, 1, 2, 3, 4, 5],
                //    "bSortable": false
                //},
                 {
                     "aTargets": [1, 2, 3, 4,],
                     "sClass": "text-center"
                 },
                {
                    "aTargets": [4],
                    "mRender": function (data, type, row) {
                        var money = toMoney(data, ',', '');
                        return money;
                    }
                }
            ],
            "bAutoWidth": false
        });
    }

    var initCardPayment = function () {
        $.ajax({
            url: '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + '/PaymentData',
            type: 'POST',
            data: { _startDate: $('#sTime').val(), _endDate: $('#eTime').val(), brandId: parseInt($('#hiddenBrandId').val()) },
            success: function (rs) {
                //console.log(rs);
                if (rs.success) {
                    setupOrderCardPieChart(rs.cardPaymentQuantity);
                    setupOrderCardRevenuePieChart(rs.cardPaymentQuantity);
                } else {
                    ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
                }
            },
            error: function (err) {
                ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
            }
        });
    }

    

    /*
     * author: HungNM
     * method: [Product]
     */
    var refreshProductTable = function () {
        var oTable = $("#tblProduct").dataTable();
        oTable._fnPageChange(0);
        oTable._fnAjaxUpdate();
    }

    var overviewMonthChart = function (monthReceipt, monthRevenue, dateList) {
        $('#revenueMonthChart').highcharts({
            title: {
                text: 'Doanh thu & hóa đơn'
            },
            xAxis: {
                categories: dateList
            },
            yAxis: [{ // Primary yAxis
                min: 0,
                labels: {
                    //format: '{value} VNĐ',
                    style: {
                        color: Highcharts.getOptions().colors[1]
                    }
                },
                title: {
                    text: 'Doanh thu',
                    style: {
                        color: Highcharts.getOptions().colors[1]
                    }
                }
            }, { // Secondary yAxis
                min: 0,
                title: {
                    text: 'Hóa đơn',
                    style: {
                        color: Highcharts.getOptions().colors[0]
                    }
                },
                labels: {
                    style: {
                        color: Highcharts.getOptions().colors[0]
                    }
                },
                opposite: true
            }],
            tooltip: {
                shared: true
            },
            legend: {
                layout: 'vertical',
                align: 'left',
                x: 120,
                verticalAlign: 'top',
                y: 20,
                floating: true,
                backgroundColor: (Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'
            }, plotOptions: {
                column: {
                    stacking: 'normal',
                    dataLabels: {
                        enabled: false
                    }

                }
            },
            series: [{
                name: 'Hoá đơn',
                type: 'column',
                yAxis: 1,
                data: monthReceipt,
                tooltip: {
                    valueSuffix: ''
                },
                pointWidth: 20,
                color: 'rgba(200, 81, 81, 1)'
            }, {
                name: 'Doanh thu',
                type: 'line',
                data: monthRevenue,
                tooltip: {
                    valueSuffix: ' VNĐ'
                },
                color: 'darkslategrey'
            }]
            //series: [{
            //    name: 'Tại cửa hàng',
            //    type: 'column',
            //    yAxis: 1,
            //    data: monthReceipt.receiptQtyAtStoreList,
            //    tooltip: {
            //        valueSuffix: ''
            //    },
            //    color: 'rgba(200, 81, 81, 1)'
            //}, {
            //    name: 'Giao hàng',
            //    type: 'column',
            //    yAxis: 1,
            //    data: monthReceipt.receiptQtyDeliveryList,
            //    tooltip: {
            //        valueSuffix: ''
            //    },
            //    color: 'rgba(204, 155, 30, 1)'
            //}, {
            //    name: 'Mang về',
            //    type: 'column',
            //    yAxis: 1,
            //    data: monthReceipt.receiptQtyTakeAwayList,
            //    tooltip: {
            //        valueSuffix: ''
            //    },
            //    color: 'darkolivegreen'
            //}, {
            //    name: 'Doanh thu',
            //    type: 'spline',
            //    data: monthRevenue,
            //    tooltip: {
            //        valueSuffix: 'VNĐ'
            //    },
            //    color: 'darkslategrey'
            //}]
        });
    };

    var revenueMonthChart = function (monthRevenue, finalRevenue) {
        $('#revenuePieChart').highcharts({
            chart: {
                type: 'area'
            },
            title: {
                text: 'Đóng góp doanh số'
            },
            subtitle: {
                text: 'Tổng doanh thu: ' + numberWithSeparator(finalRevenue) + ' VNĐ'
            },
            xAxis: {
                categories: ['Tuần 1', 'Tuần 2', 'Tuần 3', 'Tuần 4'],
                tickmarkPlacement: 'on',
                title: {
                    enabled: false
                }
            },
            yAxis: {
                title: {
                    text: 'Phẩn trăm'
                }
            },
            tooltip: {
                pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.percentage:.1f}%</b> ({point.y:,.0f} VNĐ)<br/>',
                split: true
            },
            plotOptions: {
                area: {
                    stacking: 'percent',
                    lineColor: '#ffffff',
                    lineWidth: 1,
                    marker: {
                        lineWidth: 1,
                        lineColor: '#ffffff'
                    }
                }
            },
            series: [{
                name: 'Tại cửa hàng',
                data: monthRevenue.revenueAtStoreList,
                color: 'rgba(200, 81, 81, 1)'
            }, {
                name: 'Giao hàng',
                data: monthRevenue.revenueDeliveryList,
                color: 'rgba(204, 155, 30, 1)'
            }, {
                name: 'Mang về',
                data: monthRevenue.revenueTakeAwayList,
                color: 'darkolivegreen'

            }]
        });
    };

    var receiptMonthChart = function (monthReceipt, totalReceipt) {
        $('#billPieChart').highcharts({
            chart: {
                type: 'area'
            },
            title: {
                text: 'Đóng góp hóa đơn'
            },
            subtitle: {
                text: 'Tổng số: ' + numberWithSeparator(totalReceipt) + ' Hóa đơn'
            },
            xAxis: {
                categories: ['Tuần 1', 'Tuần 2', 'Tuần 3', 'Tuần 4'],
                tickmarkPlacement: 'on',
                title: {
                    enabled: false
                }
            },
            yAxis: {
                title: {
                    text: 'Phẩn trăm'
                }
            },
            tooltip: {
                pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.percentage:.1f}%</b> ({point.y:,.0f} hoá đơn)<br/>',
                split: false
            },
            plotOptions: {
                area: {
                    stacking: 'percent',
                    lineColor: '#ffffff',
                    lineWidth: 1,
                    marker: {
                        lineWidth: 1,
                        lineColor: '#ffffff'
                    }
                }
            },
            series: [{
                name: 'Tại cửa hàng',
                data: monthReceipt.receiptQtyAtStoreList,
                color: 'rgba(200, 81, 81, 1)'
            }, {
                name: 'Giao hàng',
                data: monthReceipt.receiptQtyDeliveryList,
                color: 'rgba(204, 155, 30, 1)'
            }, {
                name: 'Mang về',
                data: monthReceipt.receiptQtyTakeAwayList,
                color: 'darkolivegreen'
            }]
        });
    };

    var productMonthChart = function (monthProduct, totalProduct) {
        $('#productPieChart').highcharts({
            chart: {
                type: 'area'
            },
            title: {
                text: 'Đóng góp sản phẩm'
            },
            subtitle: {
                text: 'Tổng số: ' + numberWithSeparator(totalProduct) + ' Sản phẩm'
            },
            xAxis: {
                categories: ['Tuần 1', 'Tuần 2', 'Tuần 3', 'Tuần 4'],
                tickmarkPlacement: 'on',
                title: {
                    enabled: false
                }
            },
            yAxis: {
                title: {
                    text: 'Phẩn trăm'
                }
            },
            tooltip: {
                pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.percentage:.1f}%</b> ({point.y:,.0f} sản phẩm)<br/>',
                split: true
            },
            plotOptions: {
                area: {
                    stacking: 'percent',
                    lineColor: '#ffffff',
                    lineWidth: 1,
                    marker: {
                        lineWidth: 1,
                        lineColor: '#ffffff'
                    }
                }
            },
            series: [{
                name: 'Tại cửa hàng',
                data: monthProduct.productQtyAtStoreList
            }, {
                name: 'Giao hàng',
                data: monthProduct.productQtyDeliveryList
            }, {
                name: 'Mang về',
                data: monthProduct.productQtyTakeAwayList
            }]
        });
    };

    var storeMonthChart = function (monthStore, finalRevenue) {
        var series = [];
        $.each(monthStore.storeIdList, function (index, storeId) {
            series.push({
                name: monthStore.storeData[storeId].storeName,
                data: monthStore.storeData[storeId].finalRevenue,
            });
        });
        $('#productPieChart').highcharts({
            chart: {
                type: 'area'
            },
            title: {
                text: 'Đóng góp doanh thu từng cửa hàng'
            },
            subtitle: {
                text: 'Tổng doanh thu: ' + numberWithSeparator(finalRevenue) + ' VNĐ'
            },
            xAxis: {
                categories: ['Tuần 1', 'Tuần 2', 'Tuần 3', 'Tuần 4'],
                tickmarkPlacement: 'on',
                title: {
                    enabled: false
                }
            },
            yAxis: {
                title: {
                    text: 'Phẩn trăm'
                }
            },
            tooltip: {
                pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.percentage:.1f}%</b> ({point.y:,.0f} VNĐ)<br/>',
                split: true
            },
            plotOptions: {
                area: {
                    stacking: 'percent',
                    lineColor: '#ffffff',
                    lineWidth: 1,
                    marker: {
                        lineWidth: 1,
                        lineColor: '#ffffff'
                    }
                }
            },
            series: series
        });
    }

    var initProductDatatable = function () {
        var storeId = parseInt($('#hiddenStoreId').val());
        $.fn.DataTable.ext.pager.numbers_length = 6;
        $("#tblProduct").datatablevpn({
            "bSort": false,
            "bRetrieve": false,
            "bServerSide": true,
            "bDestroy" : true,
            "bScrollCollapse": true,
            "bDeferRender" : true,
            "sAjaxSource": '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + "/ProductData",
            "bProcessing": true,
            "dom": '<l<t>p>',
            "iDisplayLength": 7,
            "aLengthMenu": [7, 15, 30, 50],
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "_startDate", "value": $('#sTime').val() });
                aoData.push({ "name": "_endDate", "value": $('#eTime').val() });
                aoData.push({ "name": "storeId", "value": storeId });
            },
            "oLanguage": {
                "sSearch": "Tìm kiếm:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sNext": "<i class='fa fa-chevron-right'></i>",
                    "sPrevious": "<i class='fa fa-chevron-left'></i>"
                }

            },
            "aoColumnDefs": [
                {
                    "aTargets": [0, 1, 2, 3, 4],
                    "bSortable": false
                },
                {
                    "aTargets": [0, 2, 3, 4],
                    "sClass": "text-center"
                },
                {
                    "aTargets": [3, 4],
                    "mRender": function (data, type, row) {
                        var money = toMoney(data, ',', '');
                        return money;
                    }
                }
                //{
                //    "aTargets": [3],
                //    "fnRender": function (o) {
                //        var money = toMoney(o.aData[4], ',', '');
                //        return money;
                //        //return o.aData[3];
                //    }
                //}
            ],
            "bAutoWidth": false
        });
    }

    /*
     * author: HungNM
     * method: [Revenue table]
     */
    var refreshStoreDatatable = function () {
        var oTable = $('#storeReportDatatable').dataTable();
        oTable.fnAdjustColumnSizing();
        $(".dataTables_scrollHeadInner").width('100%');
        $(".dataTables_scrollHeadInner>table").width('100%');
    };

    /*
     * author: HungNM
     * method: [Revenue table]
     */

    var initStoreDatatable = function () {
        $.ajax({
            url: '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + '/StoreData',
            type: 'GET',
            data: { _startDate: $('#sTime').val(), _endDate: $('#eTime').val(), brandId: parseInt($('#hiddenBrandId').val()) },
            success: function (rs) {
                //console.log(rs);
                if (rs.success) {
                    renderToStoreTable(rs.data);
                    storeChartData = rs.chart;
                } else {
                    ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
                }
            },
            error: function (err) {
                ShowMessage("Thao tác không thành công. Vui lòng thử lại sau.", 1);
            }
        });
    }

    var renderToStoreTable = function (ajaxData) {
        $("#storeReportDatatable").dataTable({
            "bSort": true,
            "bRetrieve": false,
            "bDestroy" : true,
            //"bServerSide": true,
            "bScrollCollapse": true,
            "sScrollY": "280px",
            "sScrollX": "100%",
            "bPaginate": false,
            //"sAjaxSource": '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + "/StoreData",
            //"bProcessing": true,
            //"iDisplayLength": 5,
            //"aLengthMenu": [5, 15, 30, 50],
            //"fnServerParams": function (aoData) {
            //    aoData.push({ "name": "_startDate", "value": $('#sTime').val(), });
            //    aoData.push({ "name": "_endDate", "value": $('#eTime').val() });
            //},
            "data": ajaxData,
            "oLanguage": {
                "sSearch": "Tìm kiếm:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": "",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sNext": "<i class='fa fa-chevron-right'></i>",
                    "sPrevious": "<i class='fa fa-chevron-left'></i>"
                }
            },
            "aoColumnDefs": [
                {
                    "aTargets": [0, 2, 3, 4, 5, 6, 7],
                    "sClass": "text-center"
                },
                {
                    "aTargets": [0, 1, 7],
                    "bSortable": false
                },
                {
                    "aTargets": [4, 5, 6],
                    "mRender": function (data, type, row) {
                        var money = toMoney(data, ',', '');
                        return money;
                    }
                },
                {
                    "aTargets": [3],
                    "mRender": function (data, type, row) {
                        var qty = numberWithSeparator(data.toFixed(1));
                        return qty;
                    }
                },
                {
                    "aTargets": [8],
                    "bVisible": false,
                },
                {
                    "aTargets": [7],
                    "bSearchable": false,
                    "mRender": function (data, type, row) {
                        var result;
                        if (data == "Tại cửa hàng") {
                            result = "<span class='badge' style='background-color:rgba(200, 81, 81, 1)'>";
                        } else if (data == "Mang về") {
                            result = "<span class='badge' style='background-color:darkolivegreen'>";
                        } else if (data == "Giao hàng") {
                            result = "<span class='badge' style='background-color:rgba(204, 155, 30, 1)'>"
                        } else {
                            result = "<span class='badge' style='background-color:grey'>";
                        }
                        return result + data + "</span>";
                    }
                }],
            "fnCreatedRow": function (nRow, aData, iDataIndex) {
                $(nRow).attr("id", aData[8]);
            },
            "bAutoWidth": false
        });
    };

    return {
        init: init,
        setupMonthPicker: setupMonthPicker,
        setupDatePicker: setupDatePicker
    }
    //function GetData() {
    //    $.ajax({
    //        type: 'GET',
    //        url: '/' + BrandId + '/DashBoard/' + StoreId + '/' + ControllerName + '/ProductChart',
    //        data: { '_startDate': $('#sTime').val(), '_endDate': $('#eTime').val() },
    //        success: function (result) {
    //            IntializeProductChart(result.dataChart)
    //        }

    //    });

    //}

    function IntializeProductChart(dataChart) {
        $('#container').highcharts({
            chart: {
                type: 'column',
                fontFamily: 'Times New Roman',
            },
            title: {
                text: 'TOP 5 Sản phẩm'
            },
            subtitle: {
                text: ''
            },
            xAxis: {
                categories: dataChart.nameArray,
                crosshair: true
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'Doanh thu'
                }
            },
            tooltip: {
                headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                    '<td style="padding:0"><b>{point.y:.1f}</b></td></tr>',
                footerFormat: '</table>',
                shared: true,
                useHTML: true
            },
            plotOptions: {
                column: {
                    pointPadding: 0.2,
                    borderWidth: 0
                }
            },
            series: [{
                name: 'Doanh thu',
                data: dataChart.amountArray,
                color: 'darkslategrey'
            },

            ]
        });
    }

    function fnShowCol(tableId, iCol) {
        /* Get the DataTables object again - this is not a recreation, just a get of the object */
        var oTable = $(tableId).dataTable();
        oTable.fnSetColumnVis(iCol, true);
    }

    function fnHideCol(tableId, iCol) {
        /* Get the DataTables object again - this is not a recreation, just a get of the object */
        var oTable = $(tableId).dataTable();
        oTable.fnSetColumnVis(iCol, false);
    }

}();