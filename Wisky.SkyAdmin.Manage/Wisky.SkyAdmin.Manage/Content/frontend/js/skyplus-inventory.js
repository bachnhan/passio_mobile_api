
// Module for common
window.HMS.Inventory = function() {
    var init = function () {
        
        var storeId = $('#storeIDtmp').val();
        var storeName = $('#storeNametmp').val();
       
        $(document).ready(function() {
            initCategories();
            $("#selectProviderList").select2();

            $(document).on("click", "#btn-save-itemExport", function (e) {
                saveItemToExportInventory();
                $("#quantity").val(1);
            });

            /*
             * author: TrungNDT
             * method: save new item into inventory
             */
            $(document).on("click", "#btn-save-item", function(e) {
                saveItemToInventory();
                $("#quantity").val(1);
            });

            $(document).on("click", "#btnCreateProvider", function (e) {
                $('#dialog').empty();
                $.ajax({
                    url: "/ProviderManager/Provider/CreateProviderForm",
                    type: "POST",

                    success: function (result) {
                        $('#dialog').append(result);
                        $('#dialog').modal('show');
                    }
                });
            });


            /*
             * author: TrungNDT
             * method: remove current record of added item
             */
            $(document).on("click", "[data-action=\"remove-inventory-item\"]", function(e) {
                var list = $("#listItemReceipt").data("items") || [];
                var id = $(this).data("id");
                for (var i = 0; i < list.length; i++) {
                    if (id === list[i].id) list.splice(i, 1);
                }
                $("#listItemReceipt").data("items", list);
                /* var me = $(e.currentTarget);
                //console.log(me);
                me.parents('tr').remove();*/
                dt.fnClearTable();
                if (list.length > 0) {
                    dt.fnAddData(list);
                }

            });

            /*
             * author: TrungNDT
             * method: update product by selected category
             */
            $(document).on("change", "#item-categories", function (e) {
                //console.log('haha');
                var providerId = $("#selectProviderList option:selected").val();
                $("#item-product").empty();
                $.ajax({
                    url: "/Provider/SelectChangeItemByProviderId",
                    type: "GET",
                    data: {
                        itemCatId: parseInt($("#item-categories").val()),
                        ProviderId: parseInt(providerId),
                    },
                    dataType: "json",
                    success: function (result) {
                        //console.log("#item-categories");
                        //console.log(result.data);
                        renderProductByData(result.data);
                    }
                });
            });

            $(document).on("change", "#item-categoriesExport", function (e) {
                var providerId = $("#selectProviderList option:selected").val();
                $("#item-product").empty();
                $.ajax({
                    url: "/Provider/SelectChangeItemByCategoryId",
                    type: "GET",
                    data: {
                        itemCatId: parseInt($("#item-categoriesExport").val()),
                    },
                    dataType: "json",
                    success: function (result) {
                        renderProductByData(result.data);
                    }
                });
                $("#item-product").trigger("change");
            });
            $("#item-categories").trigger("change");
            /*
             * author: TrungNDT
             * method: reset quantity when product changed
             */
            $(document).on("change", "#item-product", function(e) {
                $("#quantity").val(1);
                $("#item-unit").empty();
                var unitList = JSON.parse($(this).find("option:selected").attr("data-unit"));
                var unitRate = ($(this).find("option:selected").attr("unit-rate")) || 1;
                $.each(unitList, function (i, e) {
                    $('#item-unit').append($('<option/>', {
                        'html': e,
                        'value': i,
                        'unit-rate': (i != 0) ? unitRate : 1,
                    }));
                });
                //$("#lblUnit").html($(this).find("option:selected").data("unit"));
            });

            $(document).on("click", "#btnSubmit", function(e) {
                var data = {
                    Creator: $("#creator").val(),
                    InStoreId: $("#selectInStore").val(),
                    Notes: $("#notes-import").val(),
                    InvoiceNumber: $("#invoice-number-import").val(),
                    ReceiptItems: $("#listItemReceipt").data("items"),
                    ReceiptTypeId: $("#selectReceiptType").val(),
                    ProviderId: $("#selectProviderList").val(),
                    ImportDate: $("#sTime").val()
                };
                //console.log(data.ReceiptItems);
                var dataStr = JSON.stringify(data);
                if (validateInput()) {
                    $.ajax({
                        url: "/InventoryManager/" + storeId + "/" + storeName + "/ProductInventory/ImportInventory",
                        type: "POST",
                        data: { data: dataStr },
                        dataType: "json",
                        success: function (result) {
                            if (result.success) {
                                ShowMessage("Thêm hóa đơn thành công", 2);
                                window.location.replace("ListImportInventory");
                            } else {
                                ShowMessage(result.message, 1);
                            }
                        }
                    });
                }
            });

            $(document).on("click", "#btnSubmitTransfer", function (e) {
                var data = {
                    Creator: $("#creator").val(),
                    InStoreId: $("#selectInStore").val(),
                    Notes: $("#notes-import").val(),
                    ReceiptItems: $("#listItemReceipt").data("items"),
                    ReceiptTypeId: 2,
                    ExportDate: $("#sTime").val()
                };
                //console.log(data);
                var dataStr = JSON.stringify(data);
                if (validateTransferedInput()) {
                    $.ajax({
                        url: "/InventoryManager/" + storeId + "/" + storeName + "/ProductInventory/TransferInventory",
                        type: "POST",
                        data: { data: dataStr },
                        dataType: "json",
                        success: function (result) {
                            if (result.success) {
                                window.location.replace("ListTransferInventory");
                            } else {
                                ShowMessage("Có lỗi xảy ra, vui lòng thử lại", 1);
                            }
                        }
                    });
                }
            });

            $(document).on("click", "#btnSubmitEx", function(e) {
                var data = {
                    Creator: $("#creator").val(),
                    InStoreId: $("#selectInStore").val(),
                    Notes: $("#notes-import").val(),
                    ReceiptItems: $("#listItemReceipt").data("items"),
                    ReceiptTypeId: $("#selectReceiptType").val(),
                    ProviderId: $("#selectProviderList").val(),
                    ExportDate: $("#sTime").val()
                };
                var dataStr = JSON.stringify(data);
                if (validateInput()) {
                    $.ajax({
                        url: "/InventoryManager/" + storeId + "/" + storeName + "/ProductInventory/ExportInventory",
                        type: "POST",
                        data: { data: dataStr },
                        dataType: "json",
                        success: function(result) {
                            if (result.success) {
                                window.location.replace("ListExportInventory");
                            } else {
                                ShowMessage("Có lỗi xảy ra, vui lòng thử lại", 1);
                            }
                        }
                    });
                }
            });

            window.dt = $("#item-import").dataTable({
                "bFilter": false,
                "bLengthChange": false,
                "lengthMenu": [[5], [5]],
                "aaData": [],
                "aoColumns": [
                    { 'mDataProp': "name" },
                    { 'mDataProp': "unit" },
                    { 'mDataProp': "quantity" },
                    { 'mDataProp': "price"},
                    {
                        'mDataProp': function(data) {
                            return "<button class=\"btn btn-danger btn-sm\" data-action=\"remove-inventory-item\" data-id=\"" + data.id + "\">Xóa</button";
                        }
                    }
                ],
                "oLanguage": {
                    "sSearch": "Tìm kiếm:",
                    "sZeroRecords": "Không có dữ liệu phù hợp",
                    "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                    "sEmptyTable": "Không có dữ liệu",
                    "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                    "sLengthMenu": "Hiển thị _MENU_ dòng",
                    "sProcessing": "Đang xử lý..."

                },

            });
            /*
            $('#sTime').on('change', function () {
               
                $.ajax({
                    url: "/InventoryManager/" + storeId + "/" + storeName + "/Inventory/CheckDateCreateReport",
                    type: "POST",
                    data: { date: $('#sTime').val() },
                    dataType: "json",
                    success: function (result) {
                        if (result.success) {
                            bootbox.dialog({
                                title: 'Xác nhận',
                                message: '<h5>Đã tồn tại báo cáo kiểm kho sau ngày ' + $('#sTime').val() +
                                    '.Nếu tiếp tục báo cáo kiểm kho này sẽ bị hủy, bạn có đồng ý? </h5>',
                                closeButton: false,
                                buttons:
                                {
                                    "ok":
                                    {
                                        "label": "<i class='icon-ok'></i> Đồng ý",
                                        "className": "btn-sm btn-success",
                                        "callback": function () {
                                            //  getReportData();
                                            changeReportStatus();
                                        }
                                    },
                                    "close":
                                    {
                                        "label": "<i class='icon-remove'></i> Không",
                                        "className": "btn-sm btn-danger",
                                        "callback": function () {
                                            bootbox.hideAll();
                                        }
                                    }
                                }
                            });
                        }
                    }
                });
            });
            */
        
        });
    };
    /*
            * author: CuongHH
            * method: vaidate data input is not null
            */
    function validateInput() {
        if ($('#sTime').val() === "") {
            ShowMessage("Bạn chưa chọn ngày nhập/xuất hàng", 1);
            return false;
        } else if ($('#creator').val() === "") {
            ShowMessage("Bạn chưa chọn người nhập/xuất hàng", 1);
            return false;
        } else if ($('#invoice-number-import').val() === "") {
            ShowMessage("Bạn chưa nhập mã hóa đơn", 1);
            return false;
        } else if ($("#listItemReceipt").data("items") === undefined) {
            ShowMessage("Bạn chưa chọn sản phẩm", 1);
            return false;
        } else return true;
    }

    function validateTransferedInput() {
        if ($('#sTime').val() === "") {
            ShowMessage("Bạn chưa chọn ngày nhập/xuất hàng", 1);
            return false;
        } else if ($('#creator').val() === "") {
            ShowMessage("Bạn chưa chọn người nhập/xuất hàng", 1);
            return false;
        } else if ($("#listItemReceipt").data("items") === undefined) {
            ShowMessage("Bạn chưa chọn sản phẩm", 1);
            return false;
        } else return true;
    }

    function changeReportStatus() {
        //var id = parseInt(reportId);
        $.ajax({
            url: "/InventoryManager/" + storeId + "/" + storeName + "/Inventory/ChangeReportStatusToCancel",
            type: "POST",
            data: { date: $('#sTime').val() },
            dataType: "json",
            success: function (result) {
                if (result.sucess) {
                    ShowMessage('Báo cáo kiểm kho ngày ' + result.checkingDate + ' đã được hủy.', 1);
                } else {
                    ShowMessage("Có lỗi xảy ra, hãy thử lại", 1);
                }
            }
        });
    }

    var initCategories = function() {
        $.ajax({
            url: "/InventoryManager/ProductInventory/SelectChangeItem",
            type: "GET",
            data: { itemCatId: parseInt($("#item-categories").val()) },
            dataType: "json",
            success: function(result) {
                renderProductByData(result.data);
            }
        });
    };
    var renderProductByData = function (data) {
        //console.log('renderProductByData');
        $("#item-product").empty();
        $.each(data, function (i, item) {
            var unitList = [item.Unit];
            if (item.Unit2 != undefined)
                unitList.push(item.Unit2);
            $("#item-product").append($("<option>", {
                value: item.ItemId, 
                text: item.ItemName,
                'data-unit': JSON.stringify(unitList),
                'data-price': item.Price,
                'unit-rate': item.UnitRate,
            }));
        });
        $("#item-product").trigger("change");
    }; /*
     * author: TrungNDT
     * method: get item value (name, quantity, unit) from input form,
     *          store in hidden field and display into table
     */
    var saveItemToInventory = function() {
        // Support method: Get index of given item in the list. Return -1 if item doesn't exist
        function indexOfItem(item, list) {
            var index = -1;
            $.each(list, function(i, e) {
                if (item.id === e.id) index = i;
            });
            return index;
        }

        // Transfer to smallest unit
        function getLowestUnitName() {
            var $selected = $("#item-unit :selected");
            var unitRate = parseInt($selected.attr('unit-rate'));
            if (unitRate > 1) return $selected.siblings().eq(0).html();
            return $selected.html();
        }

        // Init variables
        var $table = $("#item-import tbody"),
            $storeField = $("#listItemReceipt"),
            newItem = {
                id: parseInt($("#item-product").val()),
                name: $("#item-product :selected").text(),
                unit: getLowestUnitName(),
                unitRate: parseInt($("#item-unit :selected").attr("data-price")),
                price: $("#item-product :selected").attr("data-price"),
                quantity: parseInt($("#quantity").val()) * parseInt($("#item-unit :selected").attr("unit-rate")),
            };
        var data = $storeField.data("items") || [];
        // Add item to data list
        var newItemIndex = indexOfItem(newItem, data);
        //if (newItem.unitRate >= 1) {
            var minUnit = ($("#item-unit").find("option"));
            //console.log(minUnit);
        //}
        if (newItemIndex >= 0) { // Case 1: if new item exist, update quantity
            data[newItemIndex].quantity = data[newItemIndex].quantity + newItem.quantity;
        } else { // Case 2: if new item does not exist, create new one
            data.push(newItem);
        }

        // Save to hidden field
        $storeField.data("items", data);
        // and update table
        dt.fnClearTable();
        dt.fnAddData(data);
        // $table.empty();
        /* $.each(data, function (i, e) {
            var record = $('<tr/>', {
                'data-id': e.id,
                html: [
                    $('<td/>', { 'html': e.name }),
                    $('<td/>', { 'html': e.unit }),
                    $('<td/>', { 'html': e.quantity }),
                    $('<td/>', {
                        'class': 'align-center',
                        'html': '<button class="btn btn-danger btn-sm" data-action="remove-inventory-item">Xóa</button'
                    })
                ]
            });
            $table.append(record);
         });;*/
    };
    var saveItemToExportInventory = function () {
        // Support method: Get index of given item in the list. Return -1 if item doesn't exist
        function indexOfItem(item, list) {
            var index = -1;
            $.each(list, function (i, e) {
                if (item.id === e.id) index = i;
            });
            return index;
        }

        // Init variables
        var $table = $("#item-import tbody"),
            $storeField = $("#listItemReceipt"),
            newItem = {
                id: parseInt($("#item-product").val()),
                name: $("#item-product :selected").text(),
                unit: $("#lblUnit").html(),
                price: $("#item-product :selected").attr("data-price"),
                quantity: parseInt($("#quantity").val())
            };
        var data = $storeField.data("items") || [];
        //console.log(data);
        // Add item to data list
        var newItemIndex = indexOfItem(newItem, data);
        if (newItemIndex >= 0) { // Case 1: if new item exist, update quantity
            data[newItemIndex].quantity = data[newItemIndex].quantity + newItem.quantity;
        } else { // Case 2: if new item does not exist, create new one
            data.push(newItem);
        }

        // Save to hidden field
        $storeField.data("items", data);
        // and update table
        dt.fnClearTable();
        dt.fnAddData(data);
        // $table.empty();
        /* $.each(data, function (i, e) {
            var record = $('<tr/>', {
                'data-id': e.id,
                html: [
                    $('<td/>', { 'html': e.name }),
                    $('<td/>', { 'html': e.unit }),
                    $('<td/>', { 'html': e.quantity }),
                    $('<td/>', {
                        'class': 'align-center',
                        'html': '<button class="btn btn-danger btn-sm" data-action="remove-inventory-item">Xóa</button'
                    })
                ]
            });
            $table.append(record);
         });;*/
    };

    return {
        init: init
    };

  
}();