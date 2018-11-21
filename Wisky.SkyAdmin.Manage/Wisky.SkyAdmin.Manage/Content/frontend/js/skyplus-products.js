/* ==================================== PRODUCT COMMON ==================================== */
HMS.Products = function () {
    var init = function () {
        $(document).ready(function () {
            /*
             * author: TrungNDT
             * method: [EVENT] (Single item) Increase item's quantity by 1
             */
            $(document).on('click', '.service-item [data-spinner]', function (e) {
                var $item = $(this).parents('.service-item'),
                    $input = $item.find('.input-quantity'),
                    type = $(this).data('spinner');
                currentValue = $($input).val();
                if (type == 'plus')
                    currentValue++;
                else if (type == 'minus' && currentValue > 1)
                    currentValue--;
                $($input).val(currentValue);
            });

        });
    }



    /*
     * author: TrungNDT
     * method: 
     */
    var loadListExtra = function (storeId, storeName, productId) {
        $('#divLoadListExtra').html("");
        $.ajax({
            url: '/Products/' + storeId + '/' + storeName + '/Product/ListExtra',
            type: "GET",
            //data: productId,
            success: function (result) {
                $('#divLoadListExtra').html(result);
            }
        });
    }

    /*
     * author: TrungNDT
     * method: 
     */
    var activateTabByIndex = function (index) {
        $('#menu-tab li').hide();
        $('#menu-tab li:nth-child(' + index + ')').show().children('a').tab('show');
    }

    /*
     * author: TrungNDT
     * method: setup SlimScroll for ComponentList
     */
    var setupSlimScroll = function () {
        $('.slim-scroll').each(function () {
            var $this = $(this);
            $this.slimScroll({
                height: $this.data('height') || 100,
                railVisible: true
            });
        });
    }

    /*
     * author: SinhCV
     * method: 
     */
    var refreshAssignTable = function () {
        //console.log('test 1');
        var oTable = $("#assignDatatable").dataTable();
        //oTable._fnPageChange(0)
        oTable._fnAjaxUpdate();
    }

    /*
     * author: SinhCV
     * method: [EVENT] 
     */
    $(document).on('click', '#addFile', function () {
        $('#imageFile').click();
    });

    /*
     * author: SinhCV
     * method: [EVENT] 
     */
    $(document).on('change', '#imageFile', function () {
        $('#uploadButton').attr('disabled', false);
    });

    return {
        init: init,
        loadListExtra: loadListExtra,
        activateTabByIndex: activateTabByIndex,
        setupSlimScroll: setupSlimScroll,
        refreshAssignTable: refreshAssignTable
    }
}();

/* ==================================== SINGLE ==================================== */
HMS.ProductSingle = function () {
    var storeId, storeName; // {String} storeId, storeName
    var assignRecorder = {}; // {JSON} assignRecorder: local variable
    var currentProductId = 0;

    /*
     * author: TrungNDT
     * method: initiate events
     */
    var init = function () {
        $(document).ready(function () {
            storeId = $('#hiddenStoreId').val();
            storeName = $('#hiddenStoreName').val();
            brandId = $('#hiddenBrandId').val();

            renderDatatable();
            InitAssignDatatable();
            /*
             * author: TrungNDT
             * method: [EVENT] Reload datatable when category changed
             */
            $(document).on('change', '#selectProductCategory', function () {
                var catId = $(this).find(':selected').attr('data-catid');
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/LoadProductByCategory',
                    type: "GET",
                    data: { productCategoryId: catId },
                    success: function (result) {
                        $("#pro").html(result);
                        //$("#listProductTable").dataTable();
                        //window.HMS.General.refreshTable('#listProductTable');
                        InitAssignDatatable();
                        renderDatatable();
                    },
                    error: function (result) {
                        ShowMessage("Có lỗi xảy ra, vui lòng thử lại.", 1);
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT][Create] Load "Create" partial and show related tab
             */
            $(document).on('click', '#btnOpenCreateModal', function () {
                HMS.Products.activateTabByIndex(1);
                $("#tableChooseItem").empty();

                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/Create',
                    type: "GET",
                    success: function (result) {
                        $('#div-Product').html(result);
                        $('#formCreateSingle [name=ProductType]').val(1);
                        $('#createModal').modal('show');
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [Create] Validate "Create product" form and submit
             */
            $(document).on('click', '#submitCreate', function () {
                var name = $("#ProductName").val();
                if (name.trim() == "") {
                    ShowMessage("Vui lòng nhập tên Sản phẩm", 3);
                    return;
                }

                var sPrice = $("#Price").val();
                if (sPrice.trim() == "") {
                    ShowMessage("Vui lòng nhập giá Sản phẩm", 3);
                    return;
                }
                if (!$.isNumeric(sPrice)) {
                    ShowMessage("Vui lòng nhập chữ số cho giá Sản phẩm", 3);
                    return;
                }
                //var price = parseInt(sPrice);
                //if (price < 0) {
                //    ShowMessage("Vui lòng nhập giá lớn hơn 0", 3);
                //    return;
                //}
                $("#formCreateSingle").submit();
            });

            /*
             * author: SinhCV
             * method: [EVENT][Create] Submit creating new product
             */
            $(document).on('submit', '#formCreateSingle', function () {
                var $form = $(this);
                var itemCheck = $('input[name="product-single-type"]:checked').val();
                if (itemCheck == 'normal') {
                    $('#general-form').attr("disabled", "disabled");
                    $form.find('[name=ProductType]').val('0');
                    //$('form[id=createProduct] [name=GeneralProductId]').val(null);
                }
                $form.find('[name=ProductType]').val(null);

                var formData = new FormData($(this)[0]);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/Create',
                    type: 'POST',
                    data: formData,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            RefreshTableSingle();
                            ShowMessage('Tạo thành công!', 2);
                            $('#createModal').modal('hide');
                        } else {
                            ShowMessage('Tạo ko thành công!', 2);
                        }
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });

                return false;
            });
            function RefreshTableSingle() {
                var oTable = $("#listProductTable").dataTable();
                ////oTable._fnPageChange(0)
                //oTable._fnAjaxUpdate();
            };
            /*
             * author: SinhCV
             * method: [EVENT][Composition] Load "Composition" partial and show related tab
             */
            $(document).on('click', '[data-action="init-composition-section"]', function () {
                HMS.Products.activateTabByIndex(2);

                var productId = parseInt($(this).attr('data-id'));

                $('#chooseProductID').val(productId);
                loadSelectedCompositions(productId);
                loadCompositionList();

                HMS.Products.setupSlimScroll();
                HMS.Products.loadListExtra(storeId, storeName, productId); //???
            });

            /*
             * author: TrungNDT
             * method: [EVENT][Composition] Add new composition into selected item
             */
            $(document).on('click', '[data-action="add-composition"]', function () {
                var $me = $(this),
                    $item = $me.parents('.service-item'),
                    $txtQuantity = $item.find('.input-quantity'),
                    itemId = parseInt($(this).attr('data-id')),
                    productId = $("#chooseProductID").val();

                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/AddComp',
                    type: 'GET',
                    data: {
                        itemID: itemId,
                        quantity: parseInt($txtQuantity.val()),
                        productID: productId
                    },
                    success: function (result) {
                        if (result == 0) {
                            ShowMessage("Bạn đã thêm nguyên liệu này rồi", 1);
                        }
                        else {
                            $txtQuantity.val(1);
                            loadSelectedCompositions(productId);
                        }
                    }
                });
            });

            /*
             * author: TrungNDT
             * method: Remove current composition
             */
            $(document).on('click', '[data-active=remove-composition]', function () {
                var $me = $(this),
                    id = $me.attr('id');
                var productId = document.getElementById("productID-" + id).value;
                var itemId = document.getElementById("itemID-" + id).value;
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/RemoveItem',
                    type: "POST",
                    data: { productID: productId, itemID: itemId },
                    success: function (result) {
                        $("#tableChooseItem").fadeOut(200, function () {
                            $("#tr-" + id).remove();
                            $("#tableChooseItem").fadeIn();
                        });

                    }

                });
            });

            /*
             * author: SinhCV
             * method: [EVENT][Extra] Load "Extra" partial and show related tab
             */
            $(document).on('click', '[data-action="init-extra-section"]', function () {
                HMS.Products.activateTabByIndex(3);

                var productId = parseInt($(this).attr('data-id'));

                $('#chooseProductID').val(productId);
                loadSelectedExtra(productId);
                HMS.Products.loadListExtra(storeId, storeName);
            });

            /*
             * author: SinhCV
             * method: [EVENT][Extra] Add an extra item to selected item
             */
            $(document).on('click', '[data-action="add-extra"]', function () {
                var extraId = parseInt($(this).attr('data-extraId'));
                var productId = $("#chooseProductID").val();
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/AddExtra',
                    type: 'GET',
                    data: {
                        productId: productId,
                        extraId: extraId
                    },
                    success: function (result) {
                        if (result == 0) {
                            ShowMessage("Bạn đã thêm nguyên liệu này rồi", 1);
                        }
                        else {
                            loadSelectedExtra(productId);
                        }
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT][Children] 
             */
            $(document).on('click', '[data-action="remove-extra"]', function () {
                var extraId = parseInt($(this).attr('data-extraId'));
                var productId = $("#chooseProductID").val();
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/DeleteExtra',
                    type: 'GET',
                    data: {
                        productId: productId,
                        extraId: extraId
                    },
                    success: function () {
                        loadSelectedExtra(productId);
                    }
                });
            });

            /*
             * author: SinhCV
             * method: Open "AssignToStore" modal
             */
            $(document).on('click', '[data-role=setting-storeId-product-btn]', function () {
                var id = $(this).data('product-id'),
                    name = $(this).data('product-name'),
                    price = $(this).data('price');
                assignToStore(id, name, price);
            });

            /*
             * author: SinhCV
             * method: 
             */
            $(document).on('change', "[data-role='change-price-chk']", function () {
                if ($(this).prop('checked')) {
                    $(this).parent().parent().find('[data-role=change-price-txt]').val($(this).data('price'));
                    $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', false);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', false);
                } else {
                    $(this).parent().parent().find('[data-role=change-price-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', true);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', true);
                }
                assignRecorder.elementChange($(this));
            });

            /*
             * author: SinhCV
             * method: 
             */
            $(document).on('change', "[data-role='change-price-txt'],[data-role='change-discount-txt']", function () {
               
                assignRecorder.elementChange($(this));
                

            });
            
            /*
             * author: SinhCV
             * method: ["AssignToStore" View]
             */
            $(document).on('click', '#btnUpdateAssign', function () {
                var a = assignRecorder;
               

                $.ajax({
                    url: window.urls.Product_UpdateAssignToStore,
                    type: 'POST',
                    dataType: 'json',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        'productId': currentProductId,
                        'model': assignRecorder.getForAjax()
                    }),
                    success: function (data) {
                        if (data == "1") {
                            notifyBottomRight("Cập nhập thành công.");
                            //                            ShowMessage("Cập nhập thành công.", 2);
                            assignRecorder.recordList = [];
                            $('#assignToStoreModal').modal('hide');
                        } else {
                            notifyBottomRight("Cập nhập thất bại!!!");

                            //                            ShowMessage("Cập nhập không thành công.", 1);
                        }
                    },
                    error: function () {
                        notifyBottomRight("Cập nhập thất bại!!!");

                        //                        ShowMessage("Cập nhập không thành công.", 1);
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT] Load "Edit" partial and show related tab
             */
            $(document).on('click', '[data-action="init-editting-section"]', function () {
                HMS.Products.activateTabByIndex(1);
                var productId = parseInt($(this).data('id'));
                $("#chooseProductID").val(productId);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/Edit',
                    type: "GET",
                    data: { productID: productId },
                    success: function (result) {
                        $('#createModal').modal('show');
                        $("#div-Product").html(result);

                    }
                });
            });

            /*
             * author: SinhCV
             * method: Validate "Edit product" form and submit
             */
            $(document).on('click', '#submitEdit', function () {
                var name = $("#ProductName").val();
                if (name.trim() == "") {
                    ShowMessage("Vui lòng nhập tên Sản phẩm", 3);
                    return;
                }

                var sPrice = $("#Price").val();
                if (sPrice.trim() == "") {
                    ShowMessage("Vui lòng nhập giá Sản phẩm", 3);
                    return;
                }

                if (!$.isNumeric(sPrice)) {
                    showmessage("vui lòng nhập chữ số cho giá sản phẩm", 3);
                    return;
                }
                //var price = parseint(sprice);
                //if (price < 0) {
                //    showmessage("vui lòng nhập giá lớn hơn 0", 3);
                //    return;
                //}
                $("#editProductForm").submit();
            });


            $(document).on("submit", "#editProductForm", function (e) {

                var url = "/Products/Product/Edit"; // the script where you handle the form input.

                $.ajax({
                    type: "POST",
                    url: url,
                    dataType: 'JSON',
                    data: $("#editProductForm").serialize(), // serializes the form's elements.
                    success: function (data) {
                        RefreshTableSingle();
                        $('#createModal').modal('hide');
                    }
                });
                e.preventDefault(); // avoid to execute the actual submit of the form.
            });


            /*
             * author: SinhCV
             * method: [EVENT] Remove selected product
             */
            $(document).on('click', '[data-action="remove-product"]', function () {
                var id = parseInt($(this).data('id'));
                bootbox.dialog({
                    title: 'Xác nhận',
                    message: "<h5>Bạn có muốn xóa?</h5>",
                    buttons:
                    {
                        "ok":
                        {
                            "label": "<i class='icon-ok'></i> Đồng ý",
                            "className": "btn-sm btn-success",
                            "callback": function () {
                                $.ajax({
                                    url: "/Products/Product/Delete",
                                    type: 'POST',
                                    data: { ProductID: id },
                                    error: function () {
                                        ShowMessage("Không thể xóa. Xin vui lòng thử lại sau!", 1);
                                    },
                                    success: function (data) {
                                        if (data == "1") {
                                            $("#listProductTable").fadeOut(200, function () {
                                                //console.log('#trProdut' + id);
                                                $("#trProduct-" + id).remove();
                                                //HMS.General.refreshTable('#listProductTable');??? not work???
                                            });
                                            $("#listProductTable").fadeIn();

                                            ShowMessage("Xóa Sản phẩm thành công", 2);
                                        } else {
                                            ShowMessage("Không thể xóa. Xin vui lòng thử lại sau!", 1);
                                        }
                                    }
                                });
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

            /*
             * author: TrungNDT
             * method: [EVENT] Toggle group general based on selected radio type
             */
            $(document).on('change', 'input[name="product-single-type"]', function (e) {
                var radio = $(e.currentTarget).val();
                var groupGeneral = $('#groupGeneral');
                if (radio == 'normal') {
                    groupGeneral.hide();
                } else if (radio == 'general') {
                    groupGeneral.show();
                }
            });

            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            //$(document).on('click', '#isComposition', function () {
            //    var storeId = $('#hiddenStoreId').val();
            //    var storeName = $('#hiddenStoreName').val();
            //    $('#submitCreate').attr('disabled', true);
            //    var check = document.getElementById("isComposition");
            //    if (check.checked) {
            //        $('#menu-tab li:nth-child(2)').css('display', 'block');
            //        $.ajax({
            //            url: '/Products/' + storeId + '/' + storeName + '/Product/CreateWithCompostion',
            //            type: "POST",
            //            data: $("#formCreateSingle").serialize(),
            //            success: function (result) {
            //                $('#chooseProductID').val(result);
            //            }
            //        });
            //    }
            //});
        });
    }
   
    /*
     * author: SinhCV
     * method: 
     */
    var renderDatatable = function () {
        var categoryId = parseInt($('#hiddenCategoryId').val());
        $("#listProductTable").datatablevpn({
            "bFilter": true,
            "bRetrieve": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "sAjaxSource": "/Products/Product/GetAllProducts",
            "bProcessing": true,
            "dom": '<"top"f>rt<"bottom"ilp><"clear">',
            "oLanguage": {
                "sSearch": "Tên Danh mục:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sFirst": '<i class="zmdi zmdi-more"></i>',
                    "sNext": '<i class="zmdi zmdi-chevron-right"></i>',
                    "sPrevious": '<i class="zmdi zmdi-chevron-left"></i>',
                    "sLast": '<i class="zmdi zmdi-more"></i>'
                }
            },
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "productCategoryId", "value": categoryId });
            },

            "aoColumnDefs": [
                { "aTargets": [1, 3, 5] },
                {
                    "aTargets": [0],
                    "bSortable": false,
                },
                {
                    "aTargets": [4],
                    "fnRender": function (o) {
                        var money = o.aData[4];
                        return money;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [2],
                    "fnRender": function (o) {
                        var img = o.aData[2];
                        return "<img width='45' height='45' src='" + img + "' onerror='window.HMS.ServiceItemHandler.resetDefaultImage(this)' />";
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [6],
                    "fnRender": function (o) {
                        var isCombo = o.aData[6];
                        if (isCombo == 0) {
                            return $('<strong/>', { html: 'Sản phẩm thường' })[0].outerHTML;
                        } else if (isCombo == 1) {
                            return $('<strong/>', { html: 'Gói sản phẩm' })[0].outerHTML;
                        } else if (isCombo == 2) {
                            return $('<strong/>', { html: 'Phòng' })[0].outerHTML;
                        } else if (isCombo == 3) {
                            return $('<strong/>', { html: 'Phí phụ thu' })[0].outerHTML;
                        } else if (isCombo == 4) {
                            return $('<strong/>', { html: 'Giảm giá' })[0].outerHTML;
                        }
                        return 'N/A';
                    },
                    "bSortable": false,
                    "bVisible": false,
                },
                {
                    "aTargets": [7],
                    "fnRender": function (o) {
                        var i = o.aData[9];


                        var $btnComposition = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-primary',
                            'data-toggle': 'modal',
                            'data-target': '#createModal',
                            'data-action': 'init-composition-section',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-plus"></i> Thành phần'
                        });

                        var $btnExtra = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-danger',
                            'data-toggle': 'modal',
                            'data-target': '#createModal',
                            'data-action': 'init-extra-section',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-certificate"></i> Extra'
                        });

                        var $btnRelatedProduct = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-success',
                            'data-role': 'setting-storeId-product-btn',
                            'data-product-id': i,
                            'data-product-name': o.aData[3],
                            'data-price': o.aData[4],
                            'html': 'SP đồng giá'
                        });
                        return $btnComposition[0].outerHTML
                            + ' ' + (o.aData[10] ? $btnExtra[0].outerHTML : '')
                            + ' ' + (i != -1 ? $btnRelatedProduct[0].outerHTML : '');
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [8],
                    "fnRender": function (o) {
                        //var isCombo = o.aData[7];
                        //var str = "";
                        //var trash = "";
                        //trash = "<button class='btn btn-sm btn-danger' onclick='deleteProduct(this)' id='" + o.aData[8] + "'><span class='icon-trash icon-white'></span></button>";
                        //if (isCombo != null && isCombo == 1) {
                        //    str = "<a class='btn btn-sm btn-info' data-action='init-editting-section' data-comboId='" + o.aData[8] + "'><span class='icon-edit icon-white'></span></a>";
                        //} else if (o.aData[5] == 'GENERAL') {
                        //    str = "<a href='#createModal' role='button' class='btn btn-sm btn-info' data-toggle='modal' onclick='EditProductGeneral(" + o.aData[8] + ")'><span class='icon-edit icon-white'></span></a>";
                        //}
                        //else {
                        //    str = "<a href='#createModal' role='button' class='btn btn-sm btn-info' data-toggle='modal' onclick='EditProduct(" + o.aData[8] + ")'><span class='icon-edit icon-white'></span></a>";
                        //}
                        //return str + trash;
                        var $btnEdit = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-info',
                            'data-action': 'init-editting-section',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-pencil"></i>'
                        });

                        var $btnRemove = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-danger',
                            'data-action': 'remove-product',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-trash"></i>'
                        });
                        return $btnEdit[0].outerHTML + ' ' + $btnRemove[0].outerHTML;
                    },
                    "bSortable": false,

                },
                {
                    "aTargets": [9],
                    "bVisible": false,
                }
            ],
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                nRow.setAttribute('id', "trProduct-" + aData[8]);
            },
            "bAutoWidth": false,
        });
    }

    /*
     * author: TrungNDT
     * method: Render list of composition items into "composition" modal
     */
    var loadCompositionList = function () {
        $("#divLoadItem").empty();
        $.ajax({
            url: '/Products/' + storeId + '/' + storeName + '/Product/listItem',
            type: "GET",
            success: function (result) {
                $("#divLoadItem").html(result);
            }
        });
    }

    /*
     * author: TrungNDT
     * method: Load list of selected compositions that belong to selected item
     */
    var loadSelectedCompositions = function (productID) {
        $("#tableChooseItem").html("");
        $.ajax({
            url: '/Products/' + storeId + '/' + storeName + '/Product/listItemByPro',
            type: "GET",
            data: { productID: productID },
            success: function (result) {
                $("#divChooseItem").fadeOut(200, function () {
                    $("#divChooseItem").html(result);
                    $("#divChooseItem").fadeIn(200);
                });
            }

        });
    }

    /*
     * author: TrungNDT
     * method: 
     */
    var loadSelectedExtra = function (productId) {
        $("#tableChooseExtra").html("");
        $.ajax({
            url: '/Products/' + storeId + '/' + storeName + '/Product/ListItemExtraByPro',
            type: "GET",
            data: { productId: productId },
            success: function (result) {
                $("#divChooseExtra").fadeOut(200, function () {
                    $("#divChooseExtra").html(result);
                    $("#divChooseExtra").fadeIn(200);
                });
            }
        });
    }

    //#region Assign To Store
    /*
     * author: SinhCV
     * method: 
     */
    var assignToStore = function (productID, productName, price) {
        currentProductId = productID;
        $('#assignToStoreModal').modal('show');
        $('#assignModalHeader').text("Sản phẩm: " + productName);
        $('#productID').val(productID);
        $('#assignToStoreModal').data('price', price);
        assignRecorder = {
            recordList: [],
            getRecord: function (id) {
                for (var record in this.recordList) {
                    if (this.recordList[record].StoreId == id) {
                        return this.recordList[record];
                    }
                }
                return null;
            },
            addOrUpdate: function (id, price, discount, isChecked) {
                var p = this.getRecord(id);
                if (p == null) {
                    this.recordList.push({
                        StoreId: id,
                        Price: price,
                        DiscountPrice: discount,
                        IsChecked: isChecked
                    });
                } else {
                    p.Price = price;
                    p.DiscountPrice = discount;
                    p.IsChecked = isChecked;
                }
            },
            elementChange: function (ele) {
                var id = ele.data('store-id');
                var parent = ele.parent().parent();
                var price = parent.find('[data-role=change-price-txt]').val();
                var discount = parent.find('[data-role=change-discount-txt]').val();
                var isChecked = parent.find('[data-role=change-price-chk]').prop('checked');
                this.addOrUpdate(id, price, discount, isChecked);
            },
            getForAjax: function () {
                var eles = [];
                for (var i in this.recordList) {
                    eles.push({
                        StoreId: this.recordList[i].StoreId,
                        Price: this.recordList[i].Price,
                        DiscountPrice: this.recordList[i].DiscountPrice,
                        IsChecked: this.recordList[i].IsChecked
                    });
                }
                return eles;
            }
        };
        //console.log('test 1 1')
        HMS.Products.refreshAssignTable();

    }
    /*
     * author: SinhCV
     * method: 
     */
    var InitAssignDatatable = function () {
        $("#assignDatatable").datatablevpn({
            "bFilter": true,
            "bSort": false,
            "bRetrieve": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "sAjaxSource": window.urls.Product_GetStoreList,
            "bProcessing": true,
            "aLengthMenu": [10, 20, 50],
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "productID", "value": currentProductId });
            },
            "oLanguage": {
                "sSearch": "ID Hóa đơn:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sFirst": '<i class="fa fa-more"></i>',
                    "sNext": '<i class="fa fa-chevron-right"></i>',
                    "sPrevious": '<i class="fa fa-chevron-left"></i>',
                    "sLast": '<i class="fa fa-more"></i>'
                }

            },
            "aoColumnDefs": [
                { "aTargets": [1] }
                ,
                {
                    "aTargets": [0],
                    "bSortable": false
                },
                {
                    "aTargets": [2],
                    "fnRender": function (o) {
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var value, disable;
                        if (r != null) {
                            disable = !r.IsChecked ? 'disabled' : '';
                            value = r.Price;
                        } else {
                            disable = !o.aData[4] ? 'disabled' : '';
                            value = o.aData[2];
                        }
                        var priceTxt = $('<input/>', {
                            'id': 'priceTxt',
                            'type': 'number',
                            'data-role': 'change-price-txt',
                            'data-store-id': o.aData[5],
                            'value': value,
                            'min': 0,
                            'step': 100,                          
                            'oninput':"this.value= this.value.replace(/[^0-9.]/g,'');"
                        });
                        if (disable == '')
                            return priceTxt[0].outerHTML;
                        priceTxt.attr(disable, '');
                        return priceTxt[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [3],
                    "fnRender": function (o) {
                        
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var value, disable;
                        if (r != null) {
                            disable = !r.IsChecked ? 'disabled' : '';
                            value = r.DiscountPrice;
                        } else {
                            disable = !o.aData[4] ? 'disabled' : '';
                            value = o.aData[3];
                        }
                        var discountTxt = $('<input/>', {
                            'id': 'discountTxt',
                            'type': 'number',
                            'data-role': 'change-discount-txt',
                            'data-store-id': o.aData[5],
                            'value': value,
                            'min':0,
                            'max': 100,
                            'oninput': "this.value= this.value.replace(/[^0-9.]/g,'');",
                            'onKeyUp': "if(this.value>100){this.value='100';}"
                        });
                        if (disable == '')
                            return discountTxt[0].outerHTML;
                        discountTxt.attr(disable, '');
                        return discountTxt[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [4],
                    "fnRender": function (o) {
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var disable;
                        if (r != null) {
                            disable = r.IsChecked ? 'checked' : '';
                        } else {
                            disable = o.aData[4] ? 'checked' : '';
                        }
                        var assignCbx = $('<input/>', {
                            'type': 'checkbox',
                            'data-role': 'change-price-chk',
                            'data-price': $('#assignToStoreModal').data('price'),
                            'data-store-id': o.aData[5]
                        });
                        if (disable == '')
                            return assignCbx[0].outerHTML;
                        assignCbx.attr(disable, '');
                        return assignCbx[0].outerHTML;
                    },
                    "bSortable": false
                }
            ],
            "bAutoWidth": false

        });
    }
    //#endregion

    return {
        init: init
    }
}();

/* ==================================== COMBO ==================================== */
HMS.ProductCombo = function () {
    var storeId, storeName; // {String} storeId, storeName
    var currentProductId = 0;
    var assignRecorder = {}

    var init = function () {
        $(document).ready(function () {
            storeId = $('#hiddenStoreId').val();
            storeName = $('#hiddenStoreName').val();

            renderDatatable();
            InitAssignDatatable();

            /*
             * author: TrungNDT
             * method: [EVENT] Reload datatable when category changed
             */
            $(document).on('change', '#selectProductComboCategory', function () {
                var catId = $(this).find(':selected').attr('data-catid');
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/LoadProductComboByCategory',
                    type: "GET",
                    data: { productCategoryId: catId },
                    success: function (result) {
                        $("#pro").html(result);
                        //$("#listProductTable").dataTable();
                        //window.HMS.General.refreshTable('#listProductTable');
                        renderDatatable();
                    },
                    error: function (result) {
                        ShowMessage("Có lỗi xảy ra, vui lòng thử lại.", 1);
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT] Open create modal
             */
            $(document).on('click', '#btnOpenCreateModal', function () {
                HMS.Products.activateTabByIndex(1);
                $("#tableChooseItem").empty();

                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/CreateCombo',
                    type: "GET",
                    success: function (result) {
                        $('#div-Product').html(result);
                        $('#formCreateCombo [name=ProductType]').val(1);
                        $('#createModal').modal('show');
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT] Validate and submit creating new combo product
             */
            $(document).on('click', '#submitCreate', function () {
                var name = $('#ProductName').val();

                if (name != undefined) {
                    if (name.trim() == '') {
                        ShowMessage("Vui lòng nhập tên Sản phẩm", 3);
                        return;
                    }
                }
                $('#formCreateCombo').submit();
            });

            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            $(document).on('submit', '#formCreateCombo', function () {
                var formData = new FormData($(this)[0]);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/CreateCombo',
                    type: 'POST',
                    data: formData,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            RefreshTable();
                            ShowMessage('Tạo thành công!', 2);
                            $('#createModal').modal('fade');
                        } else {
                            ShowMessage('Tạo ko thành công!', 2);
                        }
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });

                return false;
            });

            /*
             * author: SinhCV
             * method: [EVENT] Load "Composition" partial and show related tab
             */
            $(document).on('click', '[data-action="init-composition-section"]', function () {
                HMS.Products.activateTabByIndex(2);

                var productId = parseInt($(this).attr('data-id'));

                $('#chooseProductID').val(productId);

                loadCompositionList();
                loadSelectedCompositions(productId);

                HMS.Products.setupSlimScroll();
            });

            /*
             * author: TrungNDT
             * method: Add new composition into selected item
             */
            $(document).on('click', '[data-action="add-composition"]', function () {
                var $me = $(this),
                    $item = $me.parents('.service-item'),
                    $txtQuantity = $item.find('.input-quantity'),
                    itemId = parseInt($(this).attr('data-id')),
                    productId = $("#chooseProductID").val();

                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/AddComp',
                    type: 'GET',
                    data: {
                        itemID: itemId,
                        quantity: parseInt($txtQuantity.val()),
                        productID: productId
                    },
                    success: function (result) {
                        if (result == 0) {
                            ShowMessage("Bạn đã thêm nguyên liệu này rồi", 1);
                        }
                        else {
                            $txtQuantity.val(1);
                            loadSelectedCompositions(productId);
                        }
                    }
                });
            });

            /*
             * author: TrungNDT
             * method: Remove current composition
             */
            $(document).on('click', '[data-active=remove-composition]', function () {
                //var $me = $(this),
                //    id = $me.attr('id');
                //var productId = document.getElementById("productID-" + id).value;
                //var itemId = document.getElementById("itemID-" + id).value;
                //$.ajax({
                //    url: '/Products/' + storeId + '/' + storeName + '/Product/RemoveItem',
                //    type: "POST",
                //    data: { productID: productId, itemID: itemId },
                //    success: function (result) {
                //        $("#tableChooseItem").fadeOut(200, function () {
                //            $("#tr-" + id).remove();
                //            $("#tableChooseItem").fadeIn();
                //        });

                //    }
                //});
            });

            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            $(document).on('change', "[data-role='change-price-chk']", function () {
                if ($(this).prop('checked')) {
                    $(this).parent().parent().find('[data-role=change-price-txt]').val($(this).data('price'));
                    $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', false);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', false);
                } else {
                    $(this).parent().parent().find('[data-role=change-price-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', true);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', true);
                }
                assignRecorder.elementChange($(this));
            });

            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            $(document).on('change', "[data-role='change-price-txt'],[data-role='change-discount-txt']", function () {
                assignRecorder.elementChange($(this));
            });

            /*
             * author: SinhCV
             * method: [EVENT] Open "AssignToStore" modal
             */
            $(document).on('click', '[data-role=setting-storeId-product-btn]', function () {
                var id = $(this).data('product-id'),
                    name = $(this).data('product-name'),
                    price = $(this).data('price');
                assignToStore(id, name, price);
            });

            /*
             * author: SinhCV
             * method: [EVENT] Load "Edit" partial and show related tab
             */
            $(document).on('click', '[data-action="init-editting-section"]', function () {
                HMS.Products.activateTabByIndex(1);
                var productId = parseInt($(this).data('id'));
                $("#chooseProductID").val(productId);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/EditCombo',
                    type: "GET",
                    data: { productID: productId },
                    success: function (result) {
                        $('#createModal').modal('show');
                        $("#div-Product").html(result);

                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT] Remove selected product
             */
            $(document).on('click', '[data-action="remove-product"]', function () {
                var cf = confirm('Bạn có muốn xóa Sản phẩm này khỏi sản phẩm hay không');
                if (cf) {
                    var productId = $(this).attr('data-product-id');
                    var comboId = $(this).attr('data-comboid');
                    $.ajax({
                        url: '/Products/' + storeId + '/' + storeName + '/Product/DeleteComboItem',
                        type: 'POST',
                        data: {
                            'ComboID': comboId,
                            'ProductId': productId
                        },
                        success: function (result) {
                            if (result.success) {
                                loadSelectedCompositions(comboId);
                            } else {
                                alert('Có lỗi xẩy ra, vui lòng thử lại');
                            }
                        },
                        error: function () {
                            alert('Có lỗi xẩy ra, vui lòng thử lại');
                        }
                    });
                }
            });

            //??????
            $(document).on('click', '#add-combo-product-btn', function () {
                var storeId = $('#hiddenStoreId').val();
                var storeName = $('#hiddenStoreName').val();
                var productId = $(this).attr('data-productId');
                var comboId = $('#chooseProductID').val();
                var qty = $(this).parent().parent().parent().find('input[data-productId=' + productId + ']').val();
                if (isNaN(parseInt(qty))) {
                    alert('Số lượng không hợp lệ!');
                    return;
                }
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/AddComboItem',
                    type: 'POST',
                    data: {
                        'ComboID': comboId,
                        'ProductId': productId,
                        'Quantity': qty
                    },
                    success: function (result) {
                        if (result.success) {
                            loadSelectedCompositions(comboId);
                        } else {
                            alert('Có lỗi xẩy ra, vui lòng thử lại');
                        }
                    },
                    error: function () {
                        alert('Bạn đã thêm sản phẩm này trước đó.');
                    }
                });
            });
        });

        //$(window).load(function () {
        //    $('#selectProductComboCategory').trigger('change');
        //});
    }

    /*
     * author: SinhCV
     * method: 
     */
    var renderDatatable = function () {
        var categoryId = parseInt($('#hiddenCategoryId').val());
        $("#listProductComboTable").datatablevpn({
            "bFilter": true,
            "bRetrieve": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "sAjaxSource": "/Products/Product/GetAllProductCombo",
            "bProcessing": true,
            "dom": '<"top"f>rt<"bottom"ilp><"clear">',
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "productCategoryId", "value": categoryId });
            },
            "oLanguage": {
                "sSearch": "Tên Danh mục:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sFirst": '<i class="zmdi zmdi-more"></i>',
                    "sNext": '<i class="zmdi zmdi-chevron-right"></i>',
                    "sPrevious": '<i class="zmdi zmdi-chevron-left"></i>',
                    "sLast": '<i class="zmdi zmdi-more"></i>'
                }
            },

            "aoColumnDefs": [
                { "aTargets": [1, 3, 5] },
                {
                    "aTargets": [0],
                    "bSortable": false,
                },
                {
                    "aTargets": [4],
                    "fnRender": function (o) {
                        var money = o.aData[4];
                        return money;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [2],
                    "fnRender": function (o) {
                        var img = o.aData[2];
                        return "<img width='45' height='45' src='/Content/images/product/" + img + "' onerror='window.HMS.ServiceItemHandler.resetDefaultImage(this)' />";
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [6],
                    "fnRender": function (o) {
                        var isCombo = o.aData[6];
                        if (isCombo == 0) {
                            return $('<strong/>', { html: 'Sản phẩm thường' })[0].outerHTML;
                        } else if (isCombo == 1) {
                            return $('<strong/>', { html: 'Gói sản phẩm' })[0].outerHTML;
                        } else if (isCombo == 2) {
                            return $('<strong/>', { html: 'Phòng' })[0].outerHTML;
                        } else if (isCombo == 3) {
                            return $('<strong/>', { html: 'Phí phụ thu' })[0].outerHTML;
                        } else if (isCombo == 4) {
                            return $('<strong/>', { html: 'Giảm giá' })[0].outerHTML;
                        }
                        return 'N/A';
                    },
                    "bSortable": false,
                    "bVisible": false,
                },
                {
                    "aTargets": [7],
                    "fnRender": function (o) {
                        var i = o.aData[9];
                        //var str = "";
                        //var composition = "";
                        //if (i == -1) {
                        //    str = "";
                        //} else {
                        //    str = "<button class='btn btn-sm btn-success' " +
                        //       "data-role='setting-storeId-product-btn'" +
                        //       "data-product-id='" + i + "'" +
                        //       " data-product-name='" + o.aData[3] + "'" +
                        //       "data-price= '" + o.aData[4] + "'   ><span class='icon-home icon-white'></span></button>";
                        //}
                        //composition = "<a href='#createModal' role='button' class='btn btn-sm btn-warning' data-toggle='modal' onclick='EditComboComposition(" + o.aData[8] + ")'><span class='icon-edit icon-white'></span></a>";

                        ////return "<button class='btn btn-sm btn-success' onclick='assignToStore(" + i + ",\" " + o.aData[3] + "\")' id='" + i + "'>" +
                        ////    "<span class='icon-home icon-white'></span></button>";
                        //return str + ' ' + composition;

                        var $btnComposition = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-primary',
                            'data-toggle': 'modal',
                            'data-target': '#createModal',
                            'data-action': 'init-composition-section',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-plus"></i> Thành phần'
                        });

                        var $btnRelatedProduct = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-success',
                            'data-role': 'setting-storeId-product-btn',
                            'data-product-id': i,
                            'data-product-name': o.aData[3],
                            'data-price': o.aData[4],
                            'html': 'SP đồng giá'
                        });
                        return $btnComposition[0].outerHTML + ' ' + (i != -1 ? $btnRelatedProduct[0].outerHTML : '');
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [8],
                    "fnRender": function (o) {
                        //var isCombo = o.aData[7];
                        //var str = "";
                        //var trash = "";
                        //trash = "<button class='btn btn-sm btn-danger' onclick='deleteProduct(this)' id='" + o.aData[8] + "'><span class='icon-trash icon-white'></span></button>";
                        //str = "<a class='btn btn-sm btn-info' id='edit-combo-btn' data-comboId='" + o.aData[8] + "'><span class='icon-edit icon-white'></span></a>";
                        //return str + trash;

                        var $btnEdit = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-info',
                            'data-action': 'init-editting-section',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-pencil"></i>'
                        });

                        var $btnRemove = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-danger',
                            'data-action': 'remove-product',
                            'data-comboId': o.aData[8],
                            'data-product-id': o.aData[9],
                            'html': '<i class="fa fa-trash"></i>'
                        });
                        return $btnEdit[0].outerHTML + ' ' + $btnRemove[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [9],
                    "bVisible": false,
                    "bSortable": false
                }
            ],
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                nRow.setAttribute('id', "trProduct-" + aData[7]);
            },
            "bAutoWidth": false,
        });
    }

    /*
     * author: SinhCV
     * method: 
     */
    var RefreshTable = function () {
        var oTable = $("#listProductComboTable").dataTable();
        //oTable._fnPageChange(0)
        oTable._fnAjaxUpdate();
    }

    /*
     * author: SinhCV
     * method: Render list of composition items into "composition" modal
     */
    var loadCompositionList = function () {
        $.ajax({
            url: '/Products/' + storeId + '/' + storeName + '/Product/ListComboProduct',
            type: "GET",
            success: function (result) {
                $("#divLoadItem").html(result);
            }
        });
    }

    /*
     * author: TrungNDT
     * method: Load list of selected compositions that belong to selected item
     */
    var loadSelectedCompositions = function (id) {
        $('#divChooseItem').html($('<div/>', {
            'class': 'widget-box transparent',
            html: [$('<div/>', {
                'class': 'widget-header',
                html: [$('<h3/>', {
                    style: 'color:black',
                    html: 'Danh sách Sản phẩm đã thêm'
                })]
            }), $('<div/>', {
                'class': 'widget-body',
                html: $('<div/>', {
                    'class': 'widget-main slim-scroll',
                    'data-height': '500',
                    html: $('<table/>', {
                        'class': 'table table-striped table-hover',
                        'id': 'combo-detail-table',
                        html: [$('<thead/>', {
                            html: $('<tr/>', {
                                html: [$('<th/>', {
                                    html: 'Hình ảnh'
                                }), $('<th/>', {
                                    html: 'Tên sản phẩm'
                                }), $('<th/>', {
                                    html: 'Giá'
                                }), $('<th/>', {
                                    html: 'Số lượng'
                                }), $('<th/>', {
                                    html: 'Xóa'
                                })]
                            })
                        }), $('<tbody/>')]
                    })
                })
            })]
        }));
        if (id != 0) {
            $.ajax({
                url: '/Products/' + storeId + '/' + storeName + '/Product/ListItemByCombo',
                data: { 'comboId': id },
                type: "POST",
                success: function (result) {
                    if (result != null) {
                        $('#divChooseItem [id=combo-detail-table] tbody').html('');
                        for (var i = 0; i < result.length; i++) {
                            var row = $('<tr/>', {
                                html: [
                                    $('<td/>', {
                                        html: $('<img/>', {
                                            src: result[i][0]
                                        })
                                    }),
                                    $('<td/>', {
                                        html: result[i][1]
                                    }),
                                    $('<td/>', {
                                        html: result[i][2]
                                    }),
                                    $('<td/>', {
                                        html: result[i][3]
                                    }),
                                    $('<td/>', {
                                        html: $('<a/>', {
                                            'class': 'btn btn-sm btn-primary',
                                            'html': 'Xóa',
                                            'data-product-id': result[i][4],
                                            'data-combo-id': id,
                                            id: 'delete-combo-detail-btn'
                                        })
                                    })
                                ]
                            });
                            $('#divChooseItem [id=combo-detail-table] tbody').append(row);
                        }
                    }

                }
            });
        }
    }

    /*
     * author: SinhCV
     * method: 
     */
    var assignToStore = function (productID, productName, price) {
        currentProductId = productID;
        $('#assignToStoreModal').modal('show');
        $('#assignModalHeader').text("Sản phẩm: " + productName);
        $('#productID').val(productID);
        $('#assignToStoreModal').data('price', price);
        assignRecorder = {
            recordList: [],
            getRecord: function (id) {
                for (var record in this.recordList) {
                    if (this.recordList[record].StoreId == id) {
                        return this.recordList[record];
                    }
                }
                return null;
            },
            addOrUpdate: function (id, price, discount, isChecked) {
                var p = this.getRecord(id);
                if (p == null) {
                    this.recordList.push({
                        StoreId: id,
                        Price: price,
                        DiscountPrice: discount,
                        IsChecked: isChecked
                    });
                } else {
                    p.Price = price;
                    p.DiscountPrice = discount;
                    p.IsChecked = isChecked;
                }
            },
            elementChange: function (ele) {
                var id = ele.data('store-id');
                var parent = ele.parent().parent();
                var price = parent.find('[data-role=change-price-txt]').val();
                var discount = parent.find('[data-role=change-discount-txt]').val();
                var isChecked = parent.find('[data-role=change-price-chk]').prop('checked');
                this.addOrUpdate(id, price, discount, isChecked);
            },
            getForAjax: function () {
                var eles = [];
                for (var i in this.recordList) {
                    eles.push({
                        StoreId: this.recordList[i].StoreId,
                        Price: this.recordList[i].Price,
                        DiscountPrice: this.recordList[i].DiscountPrice,
                        IsChecked: this.recordList[i].IsChecked
                    });
                }
                return eles;
            }
        };
        //console.log('test 1 1 1');
        HMS.Products.refreshAssignTable();
    }

    //$("#isComposition").on("click", function () {
    //    $('#submitCreate').attr('disabled', true);
    //    var check = document.getElementById("isComposition");
    //    if (check.checked) {
    //        $('#menu-tab li:nth-child(2)').css('display', 'block');
    //        $.ajax({
    //            url: "/Product/CreateWithCompostion",
    //            type: "POST",
    //            data: $("#formCreateSingle").serialize(),
    //            success: function (result) {
    //                $('#chooseProductID').val(result);
    //            }
    //        });
    //    }
    //});

    function deleteProduct(x) {
        bootbox.dialog({
            title: 'Xác nhận',
            message: "<h5>Bạn có muốn xóa?</h5>",
            buttons:
            {
                "ok":
                {
                    "label": "<i class='icon-ok'></i> Đồng ý",
                    "className": "btn-sm btn-success",
                    "callback": function () {
                        $.ajax({
                            url: "/Products/Product/Delete",
                            type: 'POST',
                            data: { ProductID: x.id },
                            error: function () {
                                ShowMessage("Không thể xóa. Xin vui lòng thử lại sau!", 1);
                            },
                            success: function (data) {
                                if (data == "1") {
                                    $("#listProductComboTable").fadeOut(200, function () {
                                        $("#trProduct-" + x.id).remove();
                                        $("#listProductComboTable").fadeIn();
                                        RefreshTable();

                                    });
                                    ShowMessage("Xóa Sản phẩm thành công", 2);
                                } else {
                                    ShowMessage("Không thể xóa. Xin vui lòng thử lại sau!", 1);
                                }
                            }
                        });
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
    }

    function InitAssignDatatable() {
        $("#assignDatatable").datatablevpn({
            "bFilter": true,
            "bSort": false,
            "bRetrieve": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "sAjaxSource": window.url.Product_GetStoreList,
            "bProcessing": true,
            "aLengthMenu": [10, 20, 50],
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "productID", "value": currentProductId });
            },
            "oLanguage": {
                "sSearch": "ID Hóa đơn:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sFirst": '<i class="zmdi zmdi-more"></i>',
                    "sNext": '<i class="zmdi zmdi-chevron-right"></i>',
                    "sPrevious": '<i class="zmdi zmdi-chevron-left"></i>',
                    "sLast": '<i class="zmdi zmdi-more"></i>'
                }

            },
            "aoColumnDefs": [
                { "aTargets": [1] }
                ,
                {
                    "aTargets": [0],
                    "bSortable": false
                },
                {
                    "aTargets": [2],
                    "fnRender": function (o) {
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var value, disable;
                        if (r != null) {
                            disable = !r.IsChecked ? 'disabled' : '';
                            value = r.Price;
                        } else {
                            disable = !o.aData[4] ? 'disabled' : '';
                            value = o.aData[2];
                        }
                        var priceTxt = $('<input/>', {
                            'type': 'number',
                            'data-role': 'change-price-txt',
                            'data-store-id': o.aData[5],
                            'value': value
                        });
                        if (disable == '')
                            return priceTxt[0].outerHTML;
                        priceTxt.attr(disable, '');
                        return priceTxt[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [3],
                    "fnRender": function (o) {

                        var r = assignRecorder.getRecord(o.aData[5]);
                        var value, disable;
                        if (r != null) {
                            disable = !r.IsChecked ? 'disabled' : '';
                            value = r.DiscountPrice;
                        } else {
                            disable = !o.aData[4] ? 'disabled' : '';
                            value = o.aData[3];
                        }
                        var discountTxt = $('<input/>', {
                            'type': 'number',
                            'data-role': 'change-discount-txt',
                            'data-store-id': o.aData[5],
                            'value': value
                        });
                        if (disable == '')
                            return discountTxt[0].outerHTML;
                        discountTxt.attr(disable, '');
                        return discountTxt[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [4],
                    "fnRender": function (o) {
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var disable;
                        if (r != null) {
                            disable = r.IsChecked ? 'checked' : '';
                        } else {
                            disable = o.aData[4] ? 'checked' : '';
                        }
                        var assignCbx = $('<input/>', {
                            'type': 'checkbox',
                            'data-role': 'change-price-chk',
                            'data-price': $('#assignToStoreModal').data('price'),
                            'data-store-id': o.aData[5]
                        });
                        if (disable == '')
                            return assignCbx[0].outerHTML;
                        assignCbx.attr(disable, '');
                        return assignCbx[0].outerHTML;
                    },
                    "bSortable": false
                }
            ],
            "bAutoWidth": false

        });
    }

    //function loadComboByCategory(me) {
    //    var catId = $(me).find(':selected').attr('data-catid');
    //    var storeId = $('#hiddenStoreId').val();
    //    var storeName = $('#hiddenStoreName').val();
    //    $.ajax({
    //        url: '/Products/' + storeId + '/' + storeName + '/Product/LoadProductComboByCategory',
    //        type: "GET",
    //        data: { productCategoryId: catId },
    //        success: function (result) {
    //            var loadPro = $("#pro");
    //            $(loadPro).html(result);
    //            $("#listProductComboTable").dataTable();
    //        },
    //        error: function (result) {
    //            ShowMessage("Có lỗi xảy ra, vui lòng thử lại.", 1);
    //        }
    //    }
    //    );
    //}

    //function EditCombo(comboId) {
    //    var storeId = $('#hiddenStoreId').val();
    //    var storeName = $('#hiddenStoreName').val();
    //    $('#menu-tab a:first').tab('show');
    //    $('#menu-tab li:nth-child(2)').css('display', 'none');
    //    $('#menu-tab li:nth-child(3)').css('display', 'none');
    //    $('#menu-tab li:nth-child(4)').css('display', 'none');
    //    $("#div-Product").html("");
    //    document.getElementById("chooseProductID").value = comboId;
    //    $.ajax({
    //        url: '/Products/' + storeId + '/' + storeName + '/Product/EditCombo',
    //        type: "GET",
    //        data: { productID: comboId },
    //        success: function (result) {
    //            $("#div-Product").html(result);

    //        }
    //    });
    //    loadListProduct();
    //    loadSelectedCompositions(comboId);
    //}

    //function EditComboComposition(productId) {
    //    //$('#menu-tab a:first').tab('show');
    //    $('#menu-tab li:nth-child(1)').css('display', 'none');
    //    $('#menu-tab li:nth-child(2)').css('display', 'block').children('a').tab('show');
    //    $('#menu-tab li:nth-child(3)').css('display', 'none');
    //    $('#menu-tab li:nth-child(4)').css('display', 'none');
    //    document.getElementById("chooseProductID").value = productId;
    //    loadListProduct();
    //    loadSelectedCompositions(productId);
    //}

    return {
        init: init
    }
}();

/* ==================================== GENERAL ==================================== */
HMS.ProductGeneral = function () {
    var storeId, storeName, // {String} storeId, storeName
        assignRecorder = {}, // {JSON} assignRecorder: local variable
        currentProductId = 0;


    var init = function () {
        $(document).ready(function () {
            storeId = $('#hiddenStoreId').val();
            storeName = $('#hiddenStoreName').val();

            renderDatatable();
            InitAssignDatatable();

            $(document).on('change', "[data-role='change-price-chk']", function () {
                if ($(this).prop('checked')) {
                    $(this).parent().parent().find('[data-role=change-price-txt]').val($(this).data('price'));
                    $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', false);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', false);
                } else {
                    $(this).parent().parent().find('[data-role=change-price-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
                    $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', true);
                    $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', true);
                }
                assignRecorder.elementChange($(this));
            });

            $(document).on('change', "[data-role='change-price-txt'],[data-role='change-discount-txt']", function () {
                assignRecorder.elementChange($(this));
            });

            $(document).on('click', '[data-role=setting-storeId-product-btn]', function () {
                assignToStore($(this).data('product-id'), $(this).data('product-name'), $(this).data('price'));
            });
            /*
             * author: TrungNDT
             * method: [EVENT] Reload datatable when category changed
             */
            $(document).on('change', '#selectProductGeneralCategory', function () {
                var catId = $(this).find(':selected').attr('data-catid');
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/LoadProductGeneralCategory',
                    type: "GET",
                    data: { productCategoryId: catId },
                    success: function (result) {
                        $("#pro").html(result);
                        //$("#listProductTable").dataTable();
                        //window.HMS.General.refreshTable('#listProductTable');
                        renderDatatable();
                    },
                    error: function (result) {
                        ShowMessage("Có lỗi xảy ra, vui lòng thử lại.", 1);
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT] Load "Create" partial and show related tab
             */
            $(document).on('click', '#btnOpenCreateModal', function () {
                HMS.Products.activateTabByIndex(1);
                $("#tableChooseItem").empty();

                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/CreateProductGeneral',
                    type: "GET",
                    success: function (result) {
                        $('#div-Product').html(result);
                        //$('#formCreateGeneral [name=ProductType]').val(1);
                        $('#createModal').modal('show');
                    }
                });
            });

            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            $(document).on('click', '#submitCreateGeneral', function () {
                var name = $('#ProductName').val();

                if (name.trim() == '') {
                    ShowMessage('Vui lòng nhập tên Sản phẩm', 3);
                    return;
                }

                $('#formCreateGeneral').submit();
            });

            $(document).on('click', '#submitEditGeneral', function () {
                var name = $('#ProductName').val();

                if (name.trim() == '') {
                    ShowMessage('Vui lòng nhập tên Sản phẩm', 3);
                    return;
                }

                $('#formEditProductGeneral').submit();
            });

            $(document).on("submit", "#formEditProductGeneral", function (e) {

                var url = "/Products/Product/EditProductGeneral"; // the script where you handle the form input.

                $.ajax({
                    type: "POST",
                    url: url,
                    dataType: 'JSON',
                    data: $("#formEditProductGeneral").serialize(), // serializes the form's elements.
                    success: function (data) {
                        RefreshTableGeneral();
                        $('#createModal').modal('hide');
                    }
                });
                e.preventDefault(); // avoid to execute the actual submit of the form.
            });

            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            $(document).on('submit', '#formCreateGeneral', function () {
                var formData = new FormData($(this)[0]);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/CreateProductGeneral',
                    type: 'POST',
                    data: formData,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            RefreshTableGeneral();
                            $('#createModal').modal('hide');
                            ShowMessage('Tạo thành công!', 2);
                            //EditProductGeneral(data.data);
                        } else {
                            ShowMessage('Tạo ko thành công!', 2);
                        }
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });

                return false;
            });

            $(document).on('submit', '#formEditProductGeneral', function () {
                var formData = new FormData($(this)[0]);
                $.ajax({
                    url: '/' + brandId + '/Admin/' + storeId + '/Product/EditProductGeneral',
                    type: 'POST',
                    data: formData,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            RefreshTableGeneral();
                            $('#createModal').modal('hide');
                            ShowMessage('Sửa thành công!', 2);
                            //EditProductGeneral(data.data);
                        } else {
                            ShowMessage('Sửa ko thành công!', 2);
                        }
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });

                return false;
            });
            /*
             * author: SinhCV
             * method: [EVENT] 
             */
            $(document).on('click', '[data-action="init-children-section"]', function () {
                //HMS.Products.activateTabByIndex(4);

                //HMS.Products.setupSlimScroll();
            });
            //shown.bs.modal

            $(document).on('click', '[data-action="modalGeneralsChildren"]', function (e) {
                var $me = $(this),
                    $btnChildren = $(e.relatedTarget),
                    productId = parseInt($(this).attr('data-id')),
                    productName = $btnChildren.data('name');
                //$('#chooseProductID').val(productId);
                //console.log($me);
                //$me.find('.modal-title').html(productName + ' - Quản lý sản phẩm con');

                //loadCurrentChildren(productId);
                //loadChildrenCreatingForm(productId);
                //$.ajax({
                //    url: '/Products/' + storeId + '/' + storeName + '/Product/ProductGeneralDetail',
                //    type: "GET",
                //    data: { productId: productId },
                //    dataType: 'HTML',
                //    success: function (result) {
                //        console.log(result);

                //        //if (result.success) {
                //        //    loadCurrentChildren(result.parentId);
                //        //    ShowMessage('Xóa thành công!', 2, 500);
                //        //} else {
                //        //    ShowMessage('Xóa không thành công!', 3, 500);
                //        //}
                //    }
                //});
            });

            /*
             * author: SinhCV
             * method: [EVENT][Children] 
             */
            $(document).on('submit', '#formCreateProductChildGeneral', function () {
                var formData = new FormData($(this)[0]);
                $.ajax({
                    //url: '/Admin/Product/CreateChildGeneral',
                    url: '/' + brandId + '/Admin/' + storeId + '/Product/CreateChildGeneral',
                    type: 'POST',
                    data: formData,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            //loadListGeneral(data.data);
                            loadCurrentChildren(data.data);
                            ShowMessage('Tạo thành công!', 2);

                        } else {
                            ShowMessage('Tạo ko thành công!', 3);
                        }
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });
                return false;
            });

            /*
             * author: SinhCV
             * method: [EVENT][Children]
             */
            $(document).on('click', '#submitCreateChild', function () {
                var name = $("#ProductName").val();

                if (name.trim() == "") {
                    ShowMessage("Vui lòng nhập tên Sản phẩm", 3);
                    return;
                }
                $('#formCreateProductChildGeneral').submit();
            });

            /*
             * author: SinhCV
             * method: [EVENT][Children] 
             */
            $(document).on('click', '[data-action="edit-child-general"]', function () {
                //HMS.Products.activateTabByIndex(1);
                //var productId = parseInt($(this).data('id'));
                //$('chooseProductID').val(productId);
                //$.ajax({
                //    url: '/Products/' + storeId + '/' + storeName + '/Product/EditChildGeneral',
                //    type: "GET",
                //    data: { productID: productId },
                //    success: function (result) {
                //        $("#div-Product").html(result);

                //    }
                //});
                //loadCurrentChildren(productId);
                //loadChildrenCreatingForm(productId);
                var productId = parseInt($(this).data('id'));
                var $me = $(this),
                    status = $me.data('status'),// Status = 0: Turn to editable mode, Status = 1: Saving mode, turn off editable
                    $editableCol = $(this).parent().siblings();
                //console.log($editableCol);
                $.each($editableCol, function (i, e) {
                    if ($(e)[0] != $editableCol.last()[0]) {
                        $(e).attr('contentEditable', status == 0);
                    }
                });

                if (status == 0) {

                    $me.html('Lưu');
                    $me.data('status', 1);
                    $editableCol.first().focus();
                    $editableCol.last().find("select").prop('disabled', false);

                } else {
                    var att = [];
                    var price = 0;
                    var discount = 0;
                    var SaleType = $editableCol.last().find("select").val();
                    if ($editableCol.length == 7) {
                        att.push($editableCol[2].textContent.trim());
                        att.push($editableCol[3].textContent.trim());
                        price = $editableCol[4].textContent.trim();
                        discount = $editableCol[5].textContent.trim()

                    } else if ($editableCol.length == 8) {
                        att.push($editableCol[2].textContent.trim());
                        att.push($editableCol[3].textContent.trim());
                        att.push($editableCol[4].textContent.trim());
                        price = $editableCol[5].textContent.trim();
                        discount = $editableCol[6].textContent.trim()

                    } else {
                        att.push($editableCol[2].textContent.trim());
                        price = $editableCol[3].textContent.trim();
                        discount = $editableCol[4].textContent.trim();
                    }
                    $editableCol.last().find("select").prop('disabled', true);
                    $.ajax({
                        //url: '/Admin/Product/UpdateProductGeneral',
                        url: '/' + brandId + '/Admin/' + storeId + '/Product/UpdateProductGeneral',
                        type: "POST",
                        data: {
                            productId: productId,
                            Name: $editableCol[0].textContent.trim(),
                            Code: $editableCol[1].textContent.trim(),
                            Attributes: att,
                            Price: price,
                            Discount: discount,
                            SaleType: SaleType,
                        },
                        error: function () {
                            //console.log('error');
                            ShowMessage("Không thể sửa. Xin vui lòng thử lại sau!", 1);
                        },
                        success: function (data) {
                            if (data.success) {
                                ShowMessage("sửa thành công", 2, 500);
                                //                                $(this).attr('data-action').val(data.dataAction);
                                loadCurrentChildren($('#input-hidden-productId').val());
                            } else {
                                //console.log('error 1');
                                ShowMessage("Không thể sửa. Xin vui lòng thử lại sau!", 1);
                            }
                        }
                    });
                    $me.html('Sửa');
                    $me.data('status', 0);
                }

            });

            /*
             * author: SinhCV
             * method: [EVENT][Children] 
             */
            $(document).on('click', '[data-action="remove-child-general"]', function () {
                //HMS.Products.activateTabByIndex(1);
                var productId = parseInt($(this).data('id'));
                $('chooseProductID').val(productId);
                $.ajax({
                    //url: '/Admin/Product/DeleteChildGeneral',
                    url: '/' + brandId + '/Admin/' + storeId + '/Product/DeleteChildGeneral',
                    type: "GET",
                    data: { productId: productId },
                    success: function (result) {
                        if (result.success) {
                            loadCurrentChildren(result.parentId);
                            ShowMessage('Xóa thành công!', 2, 500);
                        } else {
                            ShowMessage('Xóa không thành công!', 3, 500);
                        }
                        //$("#div-Product").html(result);


                    }
                });

                //loadCurrentChildren(productId);
                //loadChildrenCreatingForm(productId);

                //HMS.Products.loadSelectedCompositions(productId);
                //loadSelectedExtra(productId);
                //loadCompositionList(storeId, storeName);
                // HMS.Products.loadListExtra(storeId, storeName);

            });
            $(document).on('click', '[data-action="changeDefaultProductChildTakeAway"]', function () {
                //console.log('changeDefaultProductChildTakeAway');
                //var childProductId = $('#TakeAwayChildProductDropdown option:selected').val();
                var childProductId = $(this).attr('data-id');
                $.ajax({
                    url: '/' + brandId + '/Admin/' + storeId + '/Product/ChangeDefaultProductChildTakeAway',
                    type: "POST",
                    data: { generalProductId: $('#input-hidden-productId').val(), childProductId: childProductId },
                    success: function (result) {
                        if (result.success) {
                            loadCurrentChildren($('#input-hidden-productId').val());
                            notifyBottomRight('Cập nhật thành công.');
                            //                            ShowMessage(result.message, 2, 500);
                        } else {
                            //                            ShowMessage(result.message, 1, 500);
                            notifyBottomRight('Cập nhật thất bại!!!');
                        }
                    },
                })
            });

            $(document).on('click', '[data-action="changeDefaultChildProduct"]', function () {
                //console.log('changeDefaultChildProduct');
                //var childProductId = $('#AtStoreChildProductDropdown option:selected').val();
                var childProductId = $(this).attr('data-id');
                //console.log(childProductId);
                $.ajax({
                    url: '/' + brandId + '/Admin/' + storeId + '/Product/ChangeDefaultProductChild',
                    type: "POST",
                    data: { generalProductId: $('#input-hidden-productId').val(), childProductId: childProductId },
                    success: function (result) {
                        if (result.success) {
                            loadCurrentChildren($('#input-hidden-productId').val());
                            notifyBottomRight('Cập nhật thành công.');
                            //                            ShowMessage(result.message, 2, 500);
                        } else {
                            notifyBottomRight('Cập nhật thất bại!!!');
                            //                            ShowMessage(result.message, 1, 500);
                        }
                    },
                })
            });
            /*
             * author: SinhCV
             * method: [EVENT][Children] Validate "Edit children" form and submit
             */
            $(document).on('click', '#submitEditChild', function () {
                var name = $("#ProductName").val();

                if (name.trim() == "") {
                    //                    ShowMessage("Vui lòng nhập tên Sản phẩm", 3);
                    notifyBottomRight('Vui lòng nhập tên sản phẩm!!!');

                    return;
                }
                $("#formEditProductChildGeneral").submit();
            });

            /*
             * author: SinhCV
             * method: [EVENT][Children] 
             */
            $(document).on('submit', '#formEditProductChildGeneral', function () {
                var formData = new FormData($(this)[0]);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/EditChildGeneral',
                    type: 'POST',
                    data: formData,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            loadCurrentChildren(data.data);
                            $('#createModal').modal('hide');
                            ShowMessage('Cập nhật thành công!', 2);
                        } else {
                            ShowMessage('Cập nhật ko thành công!', 2);
                        }
                    },
                    cache: false,
                    contentType: false,
                    processData: false
                });

                return false;
            });

            /*
             * author: SinhCV
             * method: [EVENT][Children] Back to "children list" page
             */
            $(document).on('click', '#backProductDetail', function () {
                HMS.Products.activateTabByIndex(4);
                var productId = parseInt($('[name="GeneralProductId"]').val());
                loadCurrentChildren(productId);
                loadChildrenCreatingForm(productId);
            });

            /*
             * author: SinhCV
             * method: [EVENT] Load "Edit" partial and show related tab
             */
            $(document).on('click', '[data-action="init-editting-section"]', function () {
                HMS.Products.activateTabByIndex(1);
                var productId = parseInt($(this).data('id'));
                $("#chooseProductID").val(productId);
                $.ajax({
                    url: '/Products/' + storeId + '/' + storeName + '/Product/EditProductGeneral',
                    type: "GET",
                    data: { productID: productId },
                    success: function (result) {
                        $('#createModal').modal('show');
                        $("#div-Product").html(result);
                    }
                });
                loadCurrentChildren(productId);
                loadChildrenCreatingForm(productId);
            });

            /*
             * author: SinhCV
             * method: [EVENT] Load "Edit" partial and show related tab
             */
            $(document).on('click', '[data-action="remove-product"]', function () {
                var productId = parseInt($(this).data('id'));
                bootbox.dialog({
                    title: 'Xác nhận',
                    message: "<h5>Bạn có muốn xóa?</h5>",
                    buttons:
                    {
                        "ok":
                        {
                            "label": "<i class='icon-ok'></i> Đồng ý",
                            "className": "btn-sm btn-success",
                            "callback": function () {
                                $.ajax({
                                    url: "/Products/Product/Delete",
                                    type: 'POST',
                                    data: { ProductID: productId },
                                    error: function () {
                                        ShowMessage("Không thể xóa. Xin vui lòng thử lại sau!", 1);
                                    },
                                    success: function (data) {
                                        if (data == "1") {
                                            $("#listProductGeneralTable").fadeOut(200, function () {
                                                $("#trProduct-" + productId).remove();
                                                //RefreshTable();
                                            });
                                            $("#listProductGeneralTable").fadeIn();
                                            ShowMessage("Xóa Sản phẩm thành công", 2);
                                        } else {
                                            ShowMessage("Không thể xóa. Xin vui lòng thử lại sau!", 1);
                                        }
                                    }
                                });
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


            //$('#isComposition').on('click', function () {
            //    $('#submitCreate').attr('disabled', true);
            //    var check = document.getElementById('isComposition');
            //    if (check.checked) {
            //        $('#menu-tab li:nth-child(2)').css('display', 'block');
            //        $.ajax({
            //            url: '/Product/CreateWithCompostion',
            //            type: 'POST',
            //            data: $('#createProductGeneral').serialize(),
            //            success: function (result) {
            //                $('#chooseProductID').val(result);
            //            }
            //        });
            //    }
            //});

        });

    }



    /*
     * author: SinhCV
     * method: 
     */
    function renderDatatable() {
        var categoryId = parseInt($('#hiddenCategoryId').val());
        $("#listProductGeneralTable").datatablevpn({
            "bFilter": true,
            "bRetrieve": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "sAjaxSource": "/Admin/Product/GetAllProductGeneral",
            "bProcessing": true,
            "dom": '<"top"f>rt<"bottom"ilp><"clear">',
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "productCategoryId", "value": categoryId });
            },
            "oLanguage": {
                "sSearch": "Tên Danh mục:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sFirst": '<i class="zmdi zmdi-more"></i>',
                    "sNext": '<i class="zmdi zmdi-chevron-right"></i>',
                    "sPrevious": '<i class="zmdi zmdi-chevron-left"></i>',
                    "sLast": '<i class="zmdi zmdi-more"></i>'
                }
            },

            "aoColumnDefs": [
                { "aTargets": [1, 3, 5] },
                {
                    "aTargets": [0],
                    "bSortable": false,
                },
                {
                    "aTargets": [4],
                    "fnRender": function (o) {
                        var money = o.aData[4];
                        return money;
                    },
                    "bSortable": false,
                    "bVisible": false,
                },
                {
                    "aTargets": [2],
                    "fnRender": function (o) {
                        var img = o.aData[2];
                        return "<img width='45' height='45' src='" + img
                            + "' onerror='window.HMS.ServiceItemHandler.resetDefaultImage(this)' />";
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [6],
                    "fnRender": function (o) {
                        var isCombo = o.aData[6];
                        if (isCombo == 0) {
                            return $('<strong/>', { html: 'Sản phẩm thường' })[0].outerHTML;
                        } else if (isCombo == 1) {
                            return $('<strong/>', { html: 'Gói sản phẩm' })[0].outerHTML;
                        } else if (isCombo == 2) {
                            return $('<strong/>', { html: 'Phòng' })[0].outerHTML;
                        } else if (isCombo == 3) {
                            return $('<strong/>', { html: 'Phí phụ thu' })[0].outerHTML;
                        } else if (isCombo == 4) {
                            return $('<strong/>', { html: 'Giảm giá' })[0].outerHTML;
                        }
                        return 'N/A';
                    },
                    "bSortable": false,
                    "bVisible": false,
                },
                {
                    "aTargets": [7],
                    "fnRender": function (o) {
                        //var str = "";
                        //str = "<a href='#createModal' role='button' class='btn btn-sm btn-warning' data-toggle='modal' onclick='EditProductDetail(" + o.aData[8] + ")'><span class='icon-edit icon-white'></span></a>";
                        //return str;

                        var $btnComposition = $('<a/>', {
                            //'type': 'button',
                            'class': 'btn btn-sm btn-primary',
                            //'data-toggle': 'modal',
                            //'data-target': '#modalGeneralsChildren',
                            'href': '/Admin/' + storeId + '/' + storeName + '/Product/ProductGeneralDetail?productId=' + o.aData[7],
                            'data-name': o.aData[3],
                            'data-id': o.aData[7],
                            //'data-action': 'modalGeneralsChildren',
                            //'onclick': 'generalProductDetail(this)',
                            'html': '<i class="fa fa-plus"></i> Chi tiết'
                        });
                        return $btnComposition[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [8],
                    "fnRender": function (o) {
                        //var i = o.aData[8];
                        //var str = "<a href='#createModal' role='button' class='btn btn-sm btn-info' data-toggle='modal' onclick='EditProductGeneral(" + o.aData[8] + ")'><span class='icon-edit icon-white'></span></a>";
                        //var trash = "<button class='btn btn-sm btn-danger' onclick='deleteProduct(this)' id='" + i + "'><span class='icon-trash icon-white'></span></button>";
                        //return str + ' ' + trash
                        var $btnEdit = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-info',
                            'data-action': 'init-editting-section',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-pencil"></i>'
                        });

                        var $btnRemove = $('<button/>', {
                            'type': 'button',
                            'class': 'btn btn-sm btn-danger',
                            'data-action': 'remove-product',
                            'data-id': o.aData[8],
                            'html': '<i class="fa fa-trash"></i>'
                        });
                        return $btnEdit[0].outerHTML + ' ' + $btnRemove[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [9],
                    "fnRender": function (o) {

                        var i = o.aData[9];
                        if (i == -1) {
                            return "<label>Đồng giá</label>";
                        }
                        var btn = $('<button/>', {
                            'class': 'btn btn-sm btn-success',
                            'data-role': 'setting-storeId-product-btn',
                            'data-product-id': i,
                            'data-product-name': o.aData[3],
                            'html': $('<span/>', {
                                'class': 'icon-home icon-white'
                            }),
                            'data-price': o.aData[4]
                        });

                        //return "<button class='btn btn-sm btn-success' onclick='assignToStore(" + i + ",\" " + o.aData[3] + "\")' id='" + i + "'>" +
                        //    "<span class='icon-home icon-white'></span></button>";
                        return btn[0].outerHTML;
                    },
                    "bSortable": false,
                    "bVisible": false,
                }
            ],
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                nRow.setAttribute('id', "trProduct-" + aData[7]);
            },
            "bAutoWidth": false,
        });
    }

    /*
     * author: SinhCV
     * method: 
     */
    function RefreshTableGeneral() {
        var oTable = $("#listProductGeneralTable").dataTable();
        //oTable._fnPageChange(0)
        oTable._fnAjaxUpdate();
    }


    /*
     * author: SinhCV
     * method: 
     */
    function loadCurrentChildren(productId) {
        var storeId = $('#hiddenStoreId').val();
        var storeName = $('#hiddenStoreName').val();
        $('#tableChooseGeneral').html("");
        $.ajax({
            //url: '/Admin/Product/ListGeneral',
            url: '/' + brandId + '/Admin/' + storeId + '/Product/ListGeneral',
            type: "GET",
            data: { productId: productId },
            success: function (result) {
                $("#divChooseGeneral").fadeOut(200, function () {
                    $("#divChooseGeneral").html(result);
                    $("#divChooseGeneral").fadeIn(200);
                });
            }
        });
    }

    /*
     * author: SinhCV
     * method: 
     */
    function loadChildrenCreatingForm(productId) {
        $.ajax({
            url: '/Admin/' + storeId + '/' + storeName + '/Product/CreateChildGeneral',
            type: "GET",
            data: { productId: productId },
            success: function (result) {
                $('#divLoadGeneral').html(result);

            }
        });
    }

    //function assignToStore(productID, productName, price) {
    //    currentProductId = productID;
    //    $('#assignToStoreModal').modal('show');
    //    $('#assignModalHeader').text("Sản phẩm: " + productName);
    //    $('#productID').val(productID);
    //    $('#assignToStoreModal').data('price', price);
    //    assignRecorder = {
    //        recordList: [],
    //        getRecord: function (id) {
    //            for (var record in this.recordList) {
    //                if (this.recordList[record].StoreId == id) {
    //                    return this.recordList[record];
    //                }
    //            }
    //            return null;
    //        },
    //        addOrUpdate: function (id, price, discount, isChecked) {
    //            var p = this.getRecord(id);
    //            if (p == null) {
    //                this.recordList.push({
    //                    StoreId: id,
    //                    Price: price,
    //                    DiscountPercent: discount,
    //                    IsChecked: isChecked
    //                });
    //            } else {
    //                p.Price = price;
    //                p.DiscountPercent = discount;
    //                p.IsChecked = isChecked;
    //            }
    //        },
    //        elementChange: function (ele) {
    //            var id = ele.data('store-id');
    //            var parent = ele.parent().parent();
    //            var price = parent.find('[data-role=change-price-txt]').val();
    //            var discount = parent.find('[data-role=change-discount-txt]').val();
    //            var isChecked = parent.find('[data-role=change-price-chk]').prop('checked');
    //            this.addOrUpdate(id, price, discount, isChecked);
    //        },
    //        getForAjax: function () {
    //            var eles = [];
    //            for (var i in this.recordList) {
    //                eles.push({
    //                    StoreId: this.recordList[i].StoreId,
    //                    Price: this.recordList[i].Price,
    //                    DiscountPercent: this.recordList[i].DiscountPercent,
    //                    IsChecked: this.recordList[i].IsChecked
    //                });
    //            }
    //            return eles;
    //        }
    //    };
    //    console.log('test 1')
    //    RefreshAssignTable();
    //}

    //function RefreshAssignTable() {
    //    var oTable = $("#assignDatatable").dataTable();
    //    oTable._fnAjaxUpdate();
    //}

    function InitAssignDatatable() {
        $("#assignDatatable").datatablevpn({
            "bFilter": true,
            "bSort": false,
            "bRetrieve": true,
            "bServerSide": true,
            "bScrollCollapse": true,
            "sAjaxSource": '/Admin/' + storeId + '/' + storeName + '/Product/GetStoreList',
            "bProcessing": true,
            "aLengthMenu": [10, 20, 50],
            "fnServerParams": function (aoData) {
                aoData.push({ "name": "productID", "value": currentProductId });
            },
            "oLanguage": {
                "sSearch": "ID Hóa đơn:",
                "sZeroRecords": "Không có dữ liệu phù hợp",
                "sInfo": "Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng",
                "sEmptyTable": "Không có dữ liệu",
                "sInfoFiltered": " - lọc ra từ _MAX_ dòng",
                "sLengthMenu": "Hiển thị _MENU_ dòng",
                "sProcessing": "Đang xử lý...",
                "oPaginate": {
                    "sFirst": '<i class="zmdi zmdi-more"></i>',
                    "sNext": '<i class="zmdi zmdi-chevron-right"></i>',
                    "sPrevious": '<i class="zmdi zmdi-chevron-left"></i>',
                    "sLast": '<i class="zmdi zmdi-more"></i>'
                }

            },
            "aoColumnDefs": [
                { "aTargets": [1] }
                ,
                {
                    "aTargets": [0],
                    "bSortable": false
                },
                {
                    "aTargets": [2],
                    "fnRender": function (o) {
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var value, disable;
                        if (r != null) {
                            disable = !r.IsChecked ? 'disabled' : '';
                            value = r.Price;
                        } else {
                            disable = !o.aData[4] ? 'disabled' : '';
                            value = o.aData[2];
                        }
                        var priceTxt = $('<input/>', {
                            'type': 'number',
                            'data-role': 'change-price-txt',
                            'data-store-id': o.aData[5],
                            'value': value
                        });
                        if (disable == '')
                            return priceTxt[0].outerHTML;
                        priceTxt.attr(disable, '');
                        return priceTxt[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [3],
                    "fnRender": function (o) {

                        var r = assignRecorder.getRecord(o.aData[5]);
                        var value, disable;
                        if (r != null) {
                            disable = !r.IsChecked ? 'disabled' : '';
                            value = r.DiscountPrice;
                        } else {
                            disable = !o.aData[4] ? 'disabled' : '';
                            value = o.aData[3];
                        }
                        var discountTxt = $('<input/>', {
                            'type': 'number',
                            'data-role': 'change-discount-txt',
                            'data-store-id': o.aData[5],
                            'value': value
                        });
                        if (disable == '')
                            return discountTxt[0].outerHTML;
                        discountTxt.attr(disable, '');
                        return discountTxt[0].outerHTML;
                    },
                    "bSortable": false
                },
                {
                    "aTargets": [4],
                    "fnRender": function (o) {
                        var r = assignRecorder.getRecord(o.aData[5]);
                        var disable;
                        if (r != null) {
                            disable = r.IsChecked ? 'checked' : '';
                        } else {
                            disable = o.aData[4] ? 'checked' : '';
                        }
                        var assignCbx = $('<input/>', {
                            'type': 'checkbox',
                            'data-role': 'change-price-chk',
                            'data-price': $('#assignToStoreModal').data('price'),
                            'data-store-id': o.aData[5]
                        });
                        if (disable == '')
                            return assignCbx[0].outerHTML;
                        assignCbx.attr(disable, '');
                        return assignCbx[0].outerHTML;
                    },
                    "bSortable": false
                }
            ],
            "bAutoWidth": false

        });
    }

    function CreateProductGeneral() {
        $('#menu-tab a:first').tab('show');
        $('#menu-tab li:nth-child(2)').css('display', 'none');
        $('#menu-tab li:nth-child(3)').css('display', 'none');
        $('#menu-tab li:nth-child(4)').css('display', 'none');
        $('#div-Product').html("");
        $("#tableChooseItem").html("");
        $.ajax({
            url: '/Admin/' + storeId + '/' + storeName + '/Product/CreateProductGeneral',
            type: "GET",
            success: function (result) {
                $('#div-Product').html(result);
                $('form[id=createProductGeneral] [name=ProductType]').val('6');

            }
        });
        loadCompositionList(storeId, storeName);
    }

    function loadSelectedExtra(productId) {
        var storeId = $('#hiddenStoreId').val();
        var storeName = $('#hiddenStoreName').val();
        var brandId = $('#hiddenBrandId').val();
        $("#tableChooseExtra").html("");
        $.ajax({
            url: '/Products/' + storeId + '/' + storeName + '/Product/ListItemExtraByPro',
            type: "GET",
            data: { productId: productId },
            success: function (result) {
                $("#divChooseExtra").fadeOut(200, function () {
                    $("#divChooseExtra").html(result);
                    $("#divChooseExtra").fadeIn(200);
                });
            }
        });
    }

    return {
        init: init
    }
}();

var isFirstLoad = true;

function UploadImage() {
    $("#ImgForm").submit();
}

function UploadImage_Complete() {
    //Check to see if this is the first load of the iFrame
    if (isFirstLoad == true) {
        isFirstLoad = false;
        return;
    }

    //Reset the image form so the file won't get uploaded again
    document.getElementById("ImgForm").reset();

    //Grab the content of the textarea we named jsonResult .  This shold be loaded into 
    //the hidden iFrame.
    var newImg = $.parseJSON($("#UploadTarget").contents().find("#jsonResult")[0].innerHTML);

    //If there was an error, display it to the user
    if (newImg.IsValid == false) {
        ShowMessage(newImg.Message, 3);
        return;
    }

    //Create a new image and insert it into the Images div.  Just to be fancy, 
    //we're going to use a "FadeIn" effect from jQuery
    var imgDiv = document.getElementById("Images");
    var img = new Image();
    img.src = newImg.ImagePath;
    img.style.height = '50px';
    img.style.width = '50px';
    img.class = 'img-rounded';

    //Hide the image before adding to the DOM
    $(img).hide();
    $(imgDiv).html(img);
    //Now fade the image in
    $(img).fadeIn(500, null);

    var imageName = newImg.ImagePath.split("/");
    $("#PicURL").val(imageName[imageName.length - 1]);
}

function isNumeric(input) {
    var number = /^\-{0,1}(?:[0-9]+){0,1}(?:\.[0-9]+){0,1}$/i;
    var regex = RegExp(number);
    return regex.test(input) && input.length > 0;
}

//setInterval('checkInput()', 300); !!!

function checkInput() {
    if (document.getElementById("ProductName") != null) {

        var name = document.getElementById("ProductName").value;
        //var price = document.getElementById("Price").value;&& isNumeric(price)
        if (name.length != 0) {
            $("#isComposition").attr('disabled', false);
        }
        else {
            $("#isComposition").attr('disabled', true);
        }
        //checkUpload();
    }
}

//$('#createProduct').unbind('submit').bind('submit', function (e) {
//    e.preventDefault();
//    $.ajax({
//        url: this.action,
//        type: this.method,
//        contentType: attr("enctype", "multipart/form-data"),
//        data: $(this).serialize(),
//        success: function () {
//            $('#myModal').modal('hide');
//        }
//    });
//});

//function checkUpload() {
//    var imageData = document.getElementById('imageFile').value;
//    if (imageData.length == 0) {
//        $('#uploadButton').attr('disabled', true);
//    }
//}

//unused
//function enableInput(storeID) {
//    if ($("#" + storeID).is(":checked")) {
//        $("#price-" + storeID).removeAttr('disabled');
//        $("#discount-" + storeID).removeAttr('disabled');
//    } else {
//        $("#price-" + storeID).attr('disabled', 'disabled');
//        $("#discount-" + storeID).attr('disabled', 'disabled');
//    }
//}


//function EditProduct(productId) {
//    var storeId = $('#hiddenStoreId').val();
//    var storeName = $('#hiddenStoreName').val();
//    $('#menu-tab a:first').tab('show');
//    $('#menu-tab li:nth-child(2)').css('display', 'none');
//    $('#menu-tab li:nth-child(3)').css('display', 'none');
//    $('#menu-tab li:nth-child(4)').css('display', 'none');
//    $("#div-Product").html("");
//    document.getElementById("chooseProductID").value = productId;
//    $.ajax({
//        url: '/Products/' + storeId + '/' + storeName + '/Product/Edit',
//        type: "GET",
//        data: { productID: productId },
//        success: function (result) {
//            $("#div-Product").html(result);

//        }
//    });
//    HMS.Products.loadSelectedCompositions(productId);
//    loadSelectedExtra(productId);
//    loadCompositionList(storeId, storeName);
//    HMS.Products.loadListExtra(storeId, storeName);
//}

//function EditComposition(productId) {
//    //$('#menu-tab a:first').tab('show');
//    $('#menu-tab li:nth-child(1)').css('display', 'none');
//    $('#menu-tab li:nth-child(2)').css('display', 'block').children('a').tab('show');
//    $('#menu-tab li:nth-child(3)').css('display', 'none');
//    $('#menu-tab li:nth-child(4)').css('display', 'none');
//    document.getElementById("chooseProductID").value = productId;
//    HMS.Products.loadSelectedCompositions(productId);
//    HMS.Products.loadListExtra(storeId, storeName);
//    loadCompositionList(storeId, storeName);
//}

//$(document).on('change', '[data-role="change-price-chk"]', function () {
//    if ($(this).prop('checked')) {
//        $(this).parent().parent().find('[data-role=change-price-txt]').val($(this).data('price'));
//        $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
//        $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', false);
//        $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', false);
//    } else {
//        $(this).parent().parent().find('[data-role=change-price-txt]').val(0);
//        $(this).parent().parent().find('[data-role=change-discount-txt]').val(0);
//        $(this).parent().parent().find('[data-role=change-price-txt]').prop('disabled', true);
//        $(this).parent().parent().find('[data-role=change-discount-txt]').prop('disabled', true);
//    }
//    assignRecorder.elementChange($(this));
//});

//$(document).on('change', '[data-role="change-price-txt"],[data-role="change-discount-txt"]', function () {
//    assignRecorder.elementChange($(this));
//});

//$(document).on('click', '[data-role=setting-storeId-product-btn]', function () {
//    assignToStore($(this).data('product-id'), $(this).data('product-name'), $(this).data('price'));
//});
//$("#isComposition").attr('disabled', true);
