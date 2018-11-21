var WISKY = window.WISKY || {};

WISKY.layout = function () {
    function init() {
        // loadPublisherNameOrBrand();

    };

    function loadPublisherNameOrBrand() {
        window.location.href = "/../BrandManager/Home";

        //$.ajax({
        //    type: "POST",
        //    url: '/Home/LoadPublisherOrBrandName',
        //    success: function (result) {
        //        alert(result.Name + ' - ' + result.Role);
        //        if (result.Name != "") {
        //            //var publihserOrBrandName = document.getElementById("PublisherOrBrandName");
        //            //publihserOrBrandName.innerHTML = result.Name || '';
        //        } else if (result.Role == "Publisher") {
        //            window.location.href = "/../Publisher/Publishers/Home";
        //        } else if (result.Role == "BrandManager") {
        //            window.location.href = "/../BrandManager/Home";
        //        }
        //    },
        //    error: function (error) {
        //    }
        //});
    };

    var getBrandID = function () {
        return $('#hiddenAPGId').val();
    };

    var getLocationID = function () {
        return $('#hiddenAPGId').val();
    };

    return {
        init: init,
        getBrandID: getBrandID
    };
}();

WISKY.charts = function () {
    function setupHighchart(chartdata) {
        var rowsString = chartdata.trim().split("\n");
        var rowArrayOfCells = new Array();
        for (var i = 0 ; i < rowsString.length; i++) {
            rowArrayOfCells.push(rowsString[i].split(","));
        }
        //Impression
        var ImpressionObject = new Object();
        ImpressionObject["name"] = rowArrayOfCells[0][1];
        ImpressionObject["data"] = new Array();
        for (var i = 1; i < rowArrayOfCells.length; i++) {
            ImpressionObject["data"].push(parseInt(rowArrayOfCells[i][1]));
        }
        ImpressionObject["pointInterval"] = 3600 * 1000 * 24 /*one day*/;
        var pointStartDate = moment(rowArrayOfCells[1][0], "DD-MM-YYYY").toDate();
        var pointStartUTC = Date.UTC(pointStartDate.getFullYear(), pointStartDate.getMonth(), pointStartDate.getDate());
        ImpressionObject["pointStart"] = pointStartUTC;

        //Click
        var ClickObject = new Object();
        ClickObject["name"] = rowArrayOfCells[0][2];
        ClickObject["data"] = new Array();
        for (var i = 1; i < rowArrayOfCells.length; i++) {
            ClickObject["data"].push(parseInt(rowArrayOfCells[i][2]));
        }
        ClickObject["pointInterval"] = 3600 * 1000 * 24 /*one day*/;
        ClickObject["pointStart"] = pointStartUTC;

        $('#container').highcharts({
            series: [
                ImpressionObject,
                ClickObject,
            ],
            title: {
                text: 'Charts'
            },
            subtitle: {
                text: ''
            },
            xAxis: {
                type: "datetime",
            },
            yAxis: [{ // left y axis
                title: {
                    text: null
                },
                labels: {
                    align: 'left',
                    x: 3,
                    y: 16,
                    format: '{value:.,0f}'
                },
                showFirstLabel: false
            }, { // right y axis
                linkedTo: 0,
                gridLineWidth: 0,
                opposite: true,
                title: {
                    text: null
                },
                labels: {
                    align: 'right',
                    x: -3,
                    y: 16,
                    format: '{value:.,0f}'
                },
                showFirstLabel: false
            }],
            legend: {
                align: 'left',
                verticalAlign: 'top',
                y: 20,
                floating: true,
                borderWidth: 0
            },

            tooltip: {
                shared: true,
                crosshairs: true
            },

            plotOptions: {
                series: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function (e) {
                                hs.htmlExpand(null, {
                                    pageOrigin: {
                                        x: e.pageX || e.clientX,
                                        y: e.pageY || e.clientY
                                    },
                                    headingText: this.series.name,
                                    maincontentText: Highcharts.dateFormat('%A, %b %e, %Y', this.x) + ':<br/> ' +
                                        this.y + ' visits',
                                    width: 200
                                }); 00
                            }
                        }
                    },
                    marker: {
                        lineWidth: 1
                    }
                }
            }
        });
    }
}();

WISKY.brandHome = function () {
    /*
     * author: TrungNDT
     * method: Initiate functions
     */
    var init = function () {
        initGoogleMap();
    }

    /*
     * author: ???
     * method: Load google map & markers
     */
    var initGoogleMap = function () {
        var map = null;
        $.ajax({
            url: '/BrandManager/Home/getLocationLatLngAndNameByApgId',
            data: { ApgId: window.WISKY.layout.getBrandID() },
            type: "POST",
            success: function (result) {
                // calculate average coordinate
                var avgLat = 0;
                var avgLng = 0;
                for (var i = 0; i < result.listLat.length; i++) {
                    avgLat += result.listLat[i];
                    avgLng += result.listLng[i];
                }
                avgLat = avgLat / result.listLat.length;
                avgLng /= result.listLat.length;
                var mapProp = {
                    center: new google.maps.LatLng(avgLat, avgLng),
                    zoom: 6,
                    mapTypeId: google.maps.MapTypeId.ROADMAP
                };
                map = new google.maps.Map(document.getElementById("googleMap"), mapProp);
                for (var i = 0; i < result.listLat.length; i++) {
                    var marker = new google.maps.Marker({
                        position: new google.maps.LatLng(result.listLat[i], result.listLng[i]),
                    });
                    marker.setMap(map);
                }
            },
            error: function (error) {
                alert("Error occured, please try again later");
            }
        });
    };

    return {
        init: init
    };
}();

WISKY.statistics = function () {
    var init = function () { };
    var setupDaterangepicker = function () {
        $('#reportrange span').html(moment().subtract(30, 'days').format('MMMM D, YYYY') + ' - ' + moment().subtract(1, 'days').format('MMMM D, YYYY'));
        $('#reportrange').daterangepicker({
            format: 'MM/DD/YYYY',
            startDate: moment().subtract(30, 'days'),
            endDate: moment().subtract(1, 'days'),
            minDate: '01/01/2012',
            maxDate: '12/31/2015',
            dateLimit: { days: 60 },
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                'Last 7 Days': [moment().subtract(6, 'days'), moment().subtract(1, 'days')],
                'Last 30 Days': [moment().subtract(30, 'days'), moment().subtract(1, 'days')],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
            },
            opens: 'left',
            drops: 'down',
            buttonClasses: ['btn', 'btn-sm'],
            applyClass: 'btn-primary',
            cancelClass: 'btn-default',
            separator: ' to ',
            locale: {
                applyLabel: 'Submit',
                cancelLabel: 'Cancel',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            }
        }, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);

            $('#reportrange span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        });

        $('#reportrange').on('apply.daterangepicker', function (ev, picker) {


            console.log(picker.startDate + "aaaaa");
            console.log(picker.endDate);
            var startTime = picker.startDate;
            var endTime = picker.endDate;

            window.glbStartDate = startTime.toISOString();
            window.glbEndDate = endTime.toISOString();
            LoadPresenceData();
        });
    }

}();

WISKY.campaign = function () {
    var initAdvertising = function () {
        $(document).ready(function () {
            $(document).on('click', '[data-role="ad-type"]', function () {
                $('[data-role="ad-type"].active').removeClass('active');
                $(this).addClass('active');
            });
            //$('.modal').on('scroll', function () {
            //    console.log('scroll');
            //    $('.daterangepicker').daterangepicker('place')
            //});
            $(document).on('shown.bs.modal', '.modal', function () {
                $('.modal [data-toggle="tooltip"]').tooltip();

            });
        });

    }
    return {
        initAdvertising: initAdvertising
    }
}();

WISKY.location = function () {
    var init = function () {
        $(document).ready(function () {
            InitDatatable();

            $(document).on('keydown', '.phoneNum', function (e) {
                // Allow: delete[46], backspace[8], tab[9], escape[27], enter[13] , .[190] , +[107] and -[189]
                if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190, 107, 189]) !== -1 ||
                    // Allow: Ctrl+A
                    (e.keyCode == 65 && e.ctrlKey === true) ||
                    // Allow: Ctrl+C
                    (e.keyCode == 67 && e.ctrlKey === true) ||
                    //Allow: Shift + =
                    (e.keyCode == 187 && e.shiftKey === true) ||
                    // Allow: home, end, left, right, down, up
                    (e.keyCode >= 35 && e.keyCode <= 40)) {
                    return;
                }
                // Ensure that it is a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
            });

            $(document).on('keydown', '.location', function (e) {
                // Allow:  .
                if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                    // Allow: Ctrl+A
                    (e.keyCode == 65 && e.ctrlKey === true) ||
                    // Allow: Ctrl+C
                    (e.keyCode == 67 && e.ctrlKey === true) ||
                    // Allow: home, end, left, right, down, up
                    (e.keyCode >= 35 && e.keyCode <= 40)) {
                    return;
                }
                // Ensure that it is a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
            });
            // prevent hit enter to submit
            $(window).on('keydown', function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();
                    return false;
                }
            });

            $(document).on('click', '[data-action="openCreateModal"]', function () {
                openCreateModal();
            });

            $(document).on('click', '[data-action="openEditModal"]', function () {
                openEditModal(parseInt($(this).data('id')));
            });

            $(document).on('click', '[data-action="deleteLocation"]', function () {
                deleteLocation(parseInt($(this).data('id')));
            });

            $(document).on('hidden.bs.modal', '.modal', function () {
                $(this).empty();
            });

            //Form submit
            $(document).on('submit', '#createLocationForm', function (e) {
                console.log('createLocationForm');
                e.preventDefault();
                AddLocation();
            });

            $(document).on('submit', '#editLocationForm', function (e) {
                e.preventDefault();
                editLocation();
            });
        });
    }

    //=========================== Create Modal ===========================//
    var openCreateModal = function (callback) {
        return $.ajax({
            type: "POST",
            url: '/BrandManager/Location/CreateLocationForm',
            data: { accessPointGroupId: parseInt($('#apgId').val()) },
            async: false,
            success: function (result) {
                $.when(
                    $('#createModal').html(result),
                    $('#createLocationModal').modal('show'),
                    initialize('pac-input-create', 'hiddenLatitude', 'hiddenLongitude', 'googleMapCreate')
                ).then(function () {
                    if (callback != undefined) {
                        callback();
                    }
                });

            },
            error: function (error) {
                alert("Error Occur!");
            }
        });
    }

    function AddLocation() {
        var formData = new FormData(document.getElementById('createLocationForm'));
        formData.append("accessPointGroupId", $('#apgId').val());
        $.ajax({
            type: "POST",
            url: '/BrandManager/Location/AddLocation',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                alert(result.message);
                if (result.success) {
                    $('#createLocationModal').modal('hide');
                    reDrawDatatable();
                }
            },
            error: function (error) {
                alert("Error Occur");
            }
        });
    }

    //=========================== Edit Modal ===========================//
    function openEditModal(locationID) {
        $.ajax({
            type: "POST",
            url: '/BrandManager/Location/EditLocationForm',
            data: { id: locationID },
            success: function (result) {
                $('#editModal').html(result);
                $('#editLocationModal').modal('show');
                initialize('pac-input-edit', 'hiddenLatitude', 'hiddenLongitude', 'googleMap');
            },
            error: function (error) {
                alert("Error Occur");
            }
        });
    }
   

    function editLocation() {
        var formData = new FormData(document.getElementById('editLocationForm'));
        formData.append('IsApplyAll', $('#IsApplyAll').prop('checked'));
        $.ajax({
            type: "POST",
            url: '/BrandManager/Location/EditLocation',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                alert(result.message);
                if (result.success) {
                    reDrawDatatable();
                    $('#editLocationModal').modal('hide');
                }
            },
            error: function (error) {
                alert("Error Occurs");
            }
        });
    }

    //=========================== Common ===========================//
    var RefreshTable = function () {
        var oTable = $("#LocationDatatable").dataTable();
        oTable._fnPageChange(0);
        oTable._fnAjaxUpdate();
    }

    var InitDatatable = function () {
        $('#LocationDatatable').dataTable({
            'bFilter': true,
            'bRetrieve': true,
            'bServerSide': true,
            'bScrollCollapse': true,
            'sAjaxSource': '/BrandManager/Location/LoadLocationDatatableFromAPGId',
            'bProcessing': true,
            'fnServerParams': function (aoData) {
                aoData.push({ 'name': 'accessPointGroupId', 'value': parseInt($("#apgId").val()) });
            },
            'aoColumnDefs': [
                {//No, APGName, LocName, Address, Phone, Fax, ContactPerson, ContactPhone
                    'aTargets': [0, 1, 2, 3, 4, 5, 6, 7],
                    'bSortable': false,
                },

                {//action
                    'aTargets': [8],
                    'mRender': function (data, type, row) {
                        var data = row[8];
                        return '<a class="btn btn-sm btn-primary" data-id="' + data + '" onclick="edit(' + data + ')" ><i class="glyphicon glyphicon-pencil"></i></a>'
                              + ' <a class="btn btn-sm btn-primary" data-id="' + data + '" data-action="deleteLocation"><i class="fa fa-close"></i></a>';
                        //                        return '<a class="btn btn-sm btn-primary" data-id="' + data + '" data-action="openEditModal" ><i class="glyphicon glyphicon-pencil"></i></a>'
                        //                              + ' <a class="btn btn-sm btn-primary" data-id="' + data + '" data-action="deleteLocation"><i class="fa fa-close"></i></a>';
                    },
                    'bSortable': false,
                },
                {//APs
                    'aTargets': [9],
                    'mRender': function (data, type, row) {
                        var locID = row[9];
                        return '<a class="btn btn-sm btn-primary" href="/BrandManager/Accesspoint?locId=' + locID + '">View</a>';
                    },
                    'bSortable': false
                }
            ],
            'bAutoWidth': false,

        });
    }


    function deleteLocation(locId) {
        if (confirm("Do you want to delete?")) {
            $.ajax({
                type: "POST",
                url: '/BrandManager/Location/Delete',
                data: { locId: locId },
                success: function (result) {
                    if (result.success) {
                        alert("Deleted successfully!");
                        reDrawDatatable();
                    } else {
                        alert("Error Occurs");
                    }
                },
                error: function (error) {
                    alert("Error Occurs");
                }
            });
        }
    }

    var reDrawDatatable = function () {
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
        $("#LocationDatatable").dataTable().fnStandingRedraw();
    }

    return {
        init: init,
        openCreateModal: openCreateModal
    }
}();

