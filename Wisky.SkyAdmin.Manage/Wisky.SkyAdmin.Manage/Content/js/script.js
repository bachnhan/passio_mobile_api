SKYWEB = window.SKYWEB || {};

SKYWEB.Admin = {};



SKYWEB.Admin.General = function () {
    var init = function () {
        activateSelectedMenu();
        activateSeoGenerator();
    }

    /*
     * author: ???
     * method: ???
     */
    var initElfinderEvent = function () {
        var type = getUrlParameter('elementId');

        switch (type) {
            case 'Gallery': SKYWEB.Admin.Gallery.initElfinderEvent();
                break;
            default: SKYWEB.Admin.Product.initElfinderEvent();
                break;
        }
    }

    /*
     * author: TrungNDT
     * method: Activate (highlight) current page in side menu
     * Modify by: BảoTD
     */
    var activateSelectedMenu = function () {
        var pathname = window.location.pathname.replace('%20', ' '),
            $targetUrl = $('.main-menu [href="' + pathname + '"]');

        if ($targetUrl.length) {
            $targetUrl.parent().addClass('active');
            var $sub = $targetUrl.closest('.sub-menu');
            //Add Class toggled for all parent sub-menu
            while ($sub.length) {
                $sub.addClass('toggled');
                $sub.children('ul').show();
                $sub = $sub.parent().closest('.sub-menu');
            }
        }
    }

    /*
     * author: ???
     * method: ???
     */
    var generateSeoTitle = function (origin) {
        var output = removeUnicode(origin);

        output = output.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, "-").toLowerCase();
        // remove first dash
        if (output.charAt(0) == '-') output = output.substring(1);
        // remove last dash
        var last = output.length - 1;
        if (output.charAt(last) == '-') output = output.substring(0, last);

        // Max Length: 255
        if (output.length > 255) {
            output = output.substr(0, 255);
        }

        return output;
    }

    /*
     * author: ???
     * method: ???
     */
    var removeUnicode = function (str) {
        str = str.toLowerCase();
        str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
        str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
        str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
        str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
        str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
        str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
        str = str.replace(/đ/g, "d");
        str = str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'| |\"|\&|\#|\[|\]|~|$|_/g, "-");

        str = str.replace(/-+-/g, "-"); //thay thế 2- thành 1- 
        str = str.replace(/^\-+|\-+$/g, "");

        return str;
    }

    /*
     * author: ???
     * method: ???
     */
    var activateSeoGenerator = function () {
        $(".seo-source[data-seo-target]").change(function () {
            var e = $(this);
            var target = e.attr("data-seo-target");
            $(target).val(generateSeoTitle(e.val()));
        });
    }

    /*
     * author: TrungNDT
     * method: 
     *      - Setup elfinder for opening window
     *      - Handle selecting multiple images
     */
    var setupElfinderView = function () {
        var myCommands = elFinder.prototype._options.commands;

        var options = {
            url: window.urls.FILE_LOADFILE,
            commands: myCommands,
            lang: 'vn',
            uiOptions: {
                toolbar: [
                    ['back', 'forward'],
                    ['reload'],
                    ['home', 'up'],
                    ['mkdir', 'mkfile', 'upload'],
                    ['open', 'download'],
                    ['info'],
                    ['quicklook', 'editimage'],
                    ['copy', 'cut', 'paste'],
                    ['rm'],
                    ['duplicate', 'rename', 'edit', 'resize'],
                    ['view', 'sort']
                ]
            },
            contextmenu: {
                files: [
                    'getfile', '|', 'open', 'quicklook', 'editimage'
                ]
            },
            getFileCallback: function (file) {
                //var $tmpPlaceholder = getPhotoBlock();
                //$tmpPlaceholder.find('.img-container').append($('<span/> ', {
                //    'class': 'btn-remove',
                //    'html': '<i class="glyphicon glyphicon-remove"></i></span>'
                //}));
                //$tmpPlaceholder.find('input').val(file.url);
                //$tmpPlaceholder.find('img').attr('src', file.url);
                //window.opener.$("#images-preview").append($tmpPlaceholder);
                //window.close();
            },
            handlers: {
                select: function (event, elfinderInstance) {
                    if (event.data.selected.length == 1) {
                        var item = $('#' + event.data.selected[0]);
                        if (!item.hasClass('directory')) {
                            selectedFile = event.data.selected[0];
                            $('#elfinder-selectFile').show();
                            return;
                        }
                    } else if (event.data.selected.length >= 1) {
                        var items = $('#' + event.data.selected);
                        if (!items.hasClass('directory')) {
                            selectedFile = event.data.selected;
                            $('#elfinder-selectFile').show();
                            return;
                        }
                    }
                    $('#elfinder-selectFile').hide();
                    selectedFile = null;
                }
            }
        };
        $('#elfinder').elfinder(options).elfinder('instance');
        $('#finder_browse').elfinder(options).elfinder('instance'); // Must update the form field id

        //Add more button for add multi picture
        var $btnSelectFile = $('<button/>', {
            'id': 'elfinder-selectFile',
            'class': 'elfinder-selectFile',
            'html': 'Thêm file'
        });
        $('#finder_browse').append($btnSelectFile.hide());

    }

    /*
     * author: TrungNDT
     * method: [SUPPORT] Get parameter's value from url
     * params: {String} sParam: parameter's key
     * return: {String} parameter's value
     */
    var getUrlParameter = function (sParam) {
        var sPageURL = decodeURIComponent(window.location.search.substring(1)),
            sURLVariables = sPageURL.split('&'),
            sParameterName,
            i;

        for (i = 0; i < sURLVariables.length; i++) {
            sParameterName = sURLVariables[i].split('=');

            if (sParameterName[0] === sParam) {
                return sParameterName[1] === undefined ? true : sParameterName[1];
            }
        }
    };

    /*
     * author: TrungNDT
     * method: [SUPPORT] setup Elfinder to Ckeditor
     */
    var setupElfinderToCkeditor = function (id) {
        //this is summernote, not CKEditor
        $("#" + id).summernote({
            height: 350,

        });
        //CKEDITOR.replace(id, {
        //    height: 450,
        //    //extraPlugins: 'uploadimage',
        //    filebrowserBrowseUrl: window.urls.FILE_BrowseFile
        //});
    };

    return {
        init: init,
        initElfinderEvent: initElfinderEvent,
        getUrlParameter: getUrlParameter,
        setupElfinderView: setupElfinderView,
        setupElfinderToCkeditor: setupElfinderToCkeditor
    }
}();

SKYWEB.Admin.Product = function () {
    var dropzone_upload; //store dropzone process
    var DEFAULTVALS = [{
        id: 1,
        title: 'Màu sắc',
        color: 'primary'
    }, {
        id: 2,
        title: 'Kích thước',
        color: 'warning'
    }, {
        id: 3,
        title: 'Chất liệu',
        color: 'success'
    }];

    /*
     * author: TrungNDT
     * method: - Calling initiate functions
     *         - Binding events
     */
    var init = function () {
        addPairGroup();
        //addComboPairGroup();
        //addEmployeeGroup();
        setupSelect2ForExtra();
        setupDragsort();

        // #region Pair Group
        /*
         * author: TrungNDT
         * method: [EVENT] add pair group
         */
        $('[data-action=add-pair-group]').click(function (event) {
            //console.log("A");
            addPairGroup();
        });

        /*
         * author: ANHHT
         * method: [EVENT] add pair group
         */
        $('[data-action=add-combo-pair-group]').click(function (event) {
            //console.log("A");
            addComboPairGroup();
            loadComboProductddl();
        });
        /*
         * author: TrungNDT
         * method: [EVENT] remove pair group
         */
        $('.pair-panel').on('click', '[data-action=remove-pair-group]', function () {

            if ($('.pair-group').length > 1) {
                var target = $(this).parents('.pair-group');
                //if (target.find('[name=title]').val() != '' || target.find('[name=content]').val() != '') {
                //    alert('Dòng này có chứa nội dung. Không thể xóa!');
                //} else {
                //    target.remove();
                //}
                target.remove();
            } else {
                alert('Không thể xóa thêm!');
            }
        });

        //Employee Group
        /*
         * author: PhuongTA
         * method: [EVENT] add employee group
         */
        $('[data-action=add-employee-group]').click(function (event) {
            //console.log("Add Employee Group");
            addEmployeeGroup();
        });
        /*
         * author: PhuongTA
         * method: [EVENT] remove employee group
         */
        $('.employee-panel').on('click', '[data-action=remove-employee-group]', function () {
            if ($('.box-card').length > 0) {
                var target = $(this).parents('.box-card');
                target.remove();
            } else {
                alert('Không thể xóa thêm!');
            }
        });

        /*
         * author: ANHHT
         * method: [EVENT] remove combo group
         */
        $('.combo-pair-panel').on('click', '[data-action=remove-pair-group]', function () {
            if ($('.combo-pair-group').length > 1) {
                var target = $(this).parents('.combo-pair-group');
                //if (target.find('[name=title]').val() != '' || target.find('[name=content]').val() != '') {
                //    alert('Dòng này có chứa nội dung. Không thể xóa!');
                //} else {
                //    target.remove();
                //}
                var data = $(this).parents('.combo-pair-group').find('input[name=ComboProduct]').select2("data");
                if(data != null){
                    data.disabled = false;
                }
                target.remove();
            } else {
                alert('Không thể xóa thêm!');
            }
        });


        // #endregion



        /*
         * author: PhuongTA
         * method: [EVENT] add advance setting
         */
        $('[data-action=add-adv-setting]').click(function (event) {
            //console.log("Add advance setting");
            //addAdvanceSetting();
            //var target = $(this).parents('.form-group');
            var target = $('.Advance-Setting-panel .group-append');
            target.show();
            if ($("#displayPriority").val() == "") {
                $("#displayPriority").val(1000);
            }

            if ($('#txtX').val() == "") {
                $('#txtX').val(0);
            }

            if ($('#txtY').val() == "") {
                $('#txtY').val(0);
            }
        });

        /*
         * author: PhuongTA
         * method: [EVENT] remove advance setting
         */
        $('.Advance-Setting-panel').on('click', '[data-action=remove-advSet]', function () {


            //var target = $(this).parents('.group-append');
            //target.remove();
            //$btnAppend = $('#btnAppendAdvSet').html().trim();
            //$('.Advance-Setting-panel').append($btnAppend);
            var target = $(this).parents('.group-append');
            target.hide();
            //$btnAppend = $('#btnAppendAdvSet').html().trim();
            //$('.Advance-Setting-panel').append($btnAppend);
        });




        // #region Variant Group
        /*
         * author: TrungNDT
         * method: [EVENT] add Variant group
         */
        $('[data-action=add-variant-group]').click(function (event) {
            addVariantGroup();
            checkVariantVisibility();
        });

        /*
         * author: TrungNDT
         * method: [EVENT] remove variant group
         */
        $('.variant-panel').on('click', '[data-action=remove-variant-group]', function () {
            var target = $(this).parents('.variant-group');
            target.remove();
            checkVariantVisibility();
            // update variant price table
        });

        /*
         * author: TrungNDT
         * method: [EVENT] 
         */
        $(document).on('itemAdded', '.variant-group [name=content]', function (event) {
            renderVariantsAsPriceTable();
        });
        // #endregion

        // #region Image Handling
        var swappedItem = null;

        /*
         * author: HuyTCD
         * method: [EVENT] submit image when product is submitted 
         */
        $('#btn-submit-product').on('click', function () {
            if (dropzone_upload != undefined) {
                dropzone_upload.processQueue();
            }
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('.getimagefromelfinder').on('click', function () {
            // get id
            var id = $(this).parent().find('input:first-child').attr('id');
            // set id to controller
            window.open(window.urls.FILE_GetImageFromElfinder + '?elementId=' + id + '', 'GetImageFromElfinder', 'width=1000', 'height=300');
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('#avatar-preview, #images-preview').on('mousedown mousemove', function () {
            $('li[data-placeholder]').removeAttr('style');
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('#images-preview').mousemove(function () {
            var avatarListSize = $('#avatar-preview').children(':not([data-droptarget])').size();
            if (avatarListSize == 0 && swappedItem != null) {
                swappedItem.appendTo('#avatar-preview');
                swappedItem = null;
                return;
            }
            if (avatarListSize <= 1)
                return;
            var placeholder = $('#avatar-preview li[data-placeholder]');
            swappedItem = placeholder.next().size() > 0 ? placeholder.next() : placeholder.prev();
            swappedItem.appendTo('#images-preview');
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('#images-preview').mouseup(function () {
            swappedItem = null;
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('.images-preview').on('click', '.btn-remove', function () {
            var $me = $(this);
            $me.closest('li').remove();
            if ($me.hasClass('avatar') && $me.filter(':not(avatar)').find('li').length) {
                $me.append($me.filter(':not(avatar)').find('li:first-child'));
            };
        });

        // #endregion
    }

    /*
     * author: TrungNDT
     * method: Binding events only for _ElfinderTextBox view
     */
    var initElfinderEvent = function () {
        $(document).on('click', '#elfinder-selectFile', function () {
            if (selectedFile != null)
                var postData = { values: selectedFile };

            var $avatar = window.opener.$("#avatar-preview"),
                $img = window.opener.$("#images-preview");
            $.ajax({
                type: "POST",
                url: "selectFile",
                data: postData,
                success: function (data) {
                    var name = window.opener.$('#PicURL').attr('data-name');
                    $.each(data, function (i, e) {
                        var $tmpPlaceholder = getPhotoBlock();
                        $tmpPlaceholder.find('.ratio-item').append($('<span/> ', {
                            'class': 'btn-remove',
                            'html': '<i class="glyphicon glyphicon-remove"></i></span>'
                        }));

                        var $input = $tmpPlaceholder.find('input');
                        $input.attr("name", name);
                        $input.val(e);

                        $tmpPlaceholder.find('img').attr('src', e);

                        if (i == 0) {
                            if ($avatar.find("li").length == 0) {
                                $avatar.append($tmpPlaceholder);
                            } else {
                                $img.append($tmpPlaceholder);
                            }
                        } else {
                            $img.append($tmpPlaceholder);
                        }
                    })
                    window.close();
                },
                dataType: "json",
                traditional: true
            });

        });
    }

    /*
     * author: TrungNDT
     * method: Clone a pair group from template and add to panel
     * params: {String} name : product name
     * return: {JSON} selected object
     */
    var addPairGroup = function () {
        var $newPairGroup = $('#templatePairGroup').html().trim();
        $('.pair-panel').append($newPairGroup);
    };

    /*
     * author: ANHHT
     * method: Clone a combo pair group from template and add to panel
     * params: {String} name : product name
     * return: {JSON} selected object
     */
    var addComboPairGroup = function () {
        var $newPairGroup = $('#templateComboPairGroup').html().trim();
        $('.combo-pair-panel').append($newPairGroup);
    };
    /*
     * author: PhuongTA
     * method: Clone a 'Employee' group from template and add to panel
     * params: 4 params | 1: image 2:name 3:position 4:content
     * return: {JSON} selected object
     */
    var addEmployeeGroup = function () {
        var $newEmployeeGroup = $('#tmpEmployeeGroup').html().trim();
        $('.employee-panel').append($newEmployeeGroup);
    };


    /*
     * author: PhuongTA
     * method: Clone a 'Advance Setting' group from template and add to panel
     * params: 4 params | 1: image 2:name 3:position 4:content
     * return: {JSON} selected object
     */
    addAdvanceSetting = function () {
        var $newAdvanceSetting = $('#templateAdvSetting').html().trim();
        $('.Advance-Setting-panel').append($newAdvanceSetting);
    };


    /*
     * author: TrungNDT
     * method: Clone a variant group from template and add to panel
     */
    var addVariantGroup = function () {

        var currLength = $('.variant-group').length,
            $newVariantGroup = $($('#templateVariantGroup').html().trim()),
            $content = $('.variant-group [name=content]');

        Array.prototype.removeItemById = function (id) {
            var me = this;
            for (var i = 0, flag = true, length = me.length; i < length && flag; i++)
                if (me[i].id == id) {
                    me.splice(i, 1);
                    flag = false;
                }
        }

        // Filter available item in DEFAULTVALS by removing existing item
        var tempDefaultVal = DEFAULTVALS;
        $.each($('.variant-group'), function (i, e) {
            tempDefaultVal.removeItemById($(e).data('id'));
        });

        // Define default value for 'key'
        $newVariantGroup.find('[name=title]').val(tempDefaultVal[0].title);
        $newVariantGroup.data('id', tempDefaultVal[0].id);

        //Append into panel
        $('.variant-panel').append($newVariantGroup);

        // Setup tagsinput
        $('.variant-group [name=content]:last-child').tagsinput({
            tagClass: 'label label-' + tempDefaultVal[0].color
        });
    };

    /*
     * author: TrungNDT
     * method: [Variant] 
     */
    //var renderVariantsAsPriceTable = function () {
    //    var $group = $('.variant-group'),
    //        $contents = $group.find('.bootstrap-tagsinput'),
    //        variantsMatrix = [],
    //        variantsResult = [];

    //    // [Support function] Convert 2-dimension matrix into 2-dimension combination
    //    // Eg:
    //    // ** Input matrix: 
    //    // [ [Red - Green - Blue] , [Small - Medium - Large] , ...]
    //    //
    //    // ** Output Combination
    //    // [ [Red - Small] , [Red - Medium] , [Red - Large] , [Green - Small] , [Green - Medium] , [Green - Large] , ... ]
    //    //
    //    var matrixToCombination = function (matrix) {
    //        var cloneMatrix = matrix;
    //        // Remove level-1 array which is empty
    //        $.each(cloneMatrix, function (i, e) {
    //            if (e && !e.length)
    //                cloneMatrix.splice(i, 1);
    //        });

    //        // Idea: merge 1ST level-1 array with 2ND level-1 array
    //        // When merging completed, remove the 2ND
    //        // Merging done when there just have 1 level-1 array left
    //        while (cloneMatrix.length > 1) {
    //            // Step 1: clone 1ST and 2ND, clear 1ST
    //            var firstArr = cloneMatrix[0],
    //                secondArr = cloneMatrix[1];
    //            cloneMatrix[0] = [];
    //            // Step 2: merge "cloned 1ST" (firstArr) with "cloned 2ND" (secondArr)
    //            // into cloneMatrix[0]
    //            $.each(firstArr, function (i, e) {
    //                $.each(secondArr, function (j, k) {
    //                    var position = i * secondArr.length + j,
    //                        tempArray = [];
    //                    if (cloneMatrix[0][position] == undefined)
    //                        cloneMatrix[0][position] = [];

    //                    cloneMatrix[0][position] = e.concat(k);
    //                });
    //            });
    //            // Step 3: remove 2ND level-1 array
    //            cloneMatrix.splice(1, 1);
    //        }
    //        return cloneMatrix;
    //    };

    //    // Support function: Clone a mutant group from template and add to panel
    //    var addMutantGroup = function (mutantList) {
    //        var $panel = $('.mutant-panel');
    //        $panel.empty();
    //        $.each(mutantList, function (i, e) {
    //            $.each(e, function (j, k) {
    //                var $newMutantGroup = $($('#templateMutantGroup').html().trim());
    //                $.each(k, function (m, n) {
    //                    $newMutantGroup.find('.mutant-label').append($('<span/>', {
    //                        'class': 'label label-primary m-r-5',
    //                        'html': n.content
    //                    }));
    //                });
    //                $panel.append($newMutantGroup);
    //            });
    //        });
    //    };

    //    // Collect variant from HTML into a matrix
    //    $.each($contents, function (i, e) {
    //        //variantsMatrix.push($(e).tagsinput('items'));
    //        var arr = [];
    //        $.each($(e).find('.tag'), function (j, k) {
    //            arr.push([{ id: 0, content: $(k).text() }]);
    //        });
    //        variantsMatrix.push(arr);
    //    });

    //    // Render as attributes group
    //    variantsResult = matrixToCombination(variantsMatrix);
    //    //console.log(variantsResult);

    //    // Render as UI
    //    addMutantGroup(variantsResult);

    //};

    /*
     * author: TrungNDT
     * method: [Variant] If 3 variants is added, 'Add' button will be hidden
     *          for preventing adding more variant
     */
    var checkVariantVisibility = function () {
        var $btnAddVariant = $('[data-action=add-variant-group]');
        if ($('.variant-group').length >= 3) {
            $btnAddVariant.hide();
        } else {
            $btnAddVariant.show();
        }
    };

    /*
     * author: TrungNDT
     * method: Setup select2js plugin for extra dropdown
     */
    var setupSelect2ForExtra = function () {
        $('#selectExtra').select2({
            maximumSelectionLength: 3
        });
    };

    // #region Photo Handler
    /*
     * author: TrungNDT
     * method: clone photo block from template tag & return as jElement
     */
    var getPhotoBlock = function () {
        return $($('#tmpPhotoBlock').html().trim());
    }

    /*
     * author: HuyTCD
     * method: set up dragsort for #images-preview element
     */
    var setupDragsort = function () {
        var $tmpPlaceholder = getPhotoBlock();
        $tmpPlaceholder.find('.img-container').addClass('placeHolder');
        $('#avatar-preview, #images-preview').dragsort({
            dragSelector: 'li .img-container',
            dragEnd: function () { },
            dragBetween: true,
            placeHolderTemplate: $tmpPlaceholder[0].outerHTML
        });
    }

    // #endregion

    /*
     * author: HuyTCD
     * method: set up dropzone for #dZUpload element
     * [NOT USED]
     */
    var setupDropzone = function () {
        Dropzone.autoDiscover = false;
        dropzone_upload = new Dropzone('#dZUpload', {
            url: window.urls.FILE_Upload,
            addRemoveLinks: true,
            createImageThumbnails: true,
            acceptedFiles: 'image/*',
            autoProcessQueue: false,
            parallelUploads: 100,
            clickable: true,
            //enqueueForUpload: true,
            success: function (file, response) {
                var imgName = response;
                file.previewElement.classList.add('dz-success');
                //console.log('Successfully uploaded :' + imgName);
            },
            error: function (file, response) {
                file.previewElement.classList.add('dz-error');
            }
        });
    };

    return {
        init: init,
        initElfinderEvent: initElfinderEvent,
        addPairGroup: addPairGroup,
        addEmployeeGroup: addEmployeeGroup,
        addAdvanceSetting: addAdvanceSetting,
        addComboPairGroup: addComboPairGroup
    }
}();

SKYWEB.Admin.WebSetting = function () {
    var markerArray = [];

    var init = function () {
        //createMap(10.773093, 106.694151);

        /*
         * author: NhatND
         * method: Init map without position marker
         */
        var map = new google.maps.Map(document.getElementById('map'), {
            center: { lat: 10.776313, lng: 106.684628 },
            zoom: 11
        });

        /*
         * author: VuHVP
         * method: [EVENT] change coordinates
         */
        $('[data-action=change-coordinates]').click(function (event) {
            changeCoordinates();
        });
    };

    /*
     * author: VuHVP
     * method: create map
     */
    var createMap = function (newLat, newLng) {
        var myLatLng = new google.maps.LatLng(newLat, newLng)

        // Create a map object and specify the DOM element for display.
        var map = new google.maps.Map(document.getElementById('map'), {
            center: myLatLng,
            scrollwheel: true,
            zoom: 14,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        // Create a marker and set its position.
        var marker = new google.maps.Marker({
            map: map,
            position: myLatLng,
        });
        markerArray.push(marker);
        google.maps.event.addListener(map, 'click', function (e) {
            clearMarker();
            var latitue = e.latLng.lat();
            var longitude = e.latLng.lng();
            placeMarker(e.latLng, map);
            document.getElementById('lat').value = latitue.toFixed(6);
            document.getElementById('lng').value = longitude.toFixed(6);
        });
    };

    /*
     * author: VuHVP
     * method: change map to new coordinates (position) based on input
     */
    var changeCoordinates = function () {
        var newLat = $('#lat').val();
        var newLng = $('#lng').val();
        createMap(newLat, newLng);
    };

    /*
     * author: VuHVP
     * method: set marker on map
     */
    var placeMarker = function (latLng, map) {
        var marker = new google.maps.Marker({
            position: latLng,
            map: map
        });
        map.panTo(latLng);
        markerArray.push(marker);
    };

    /*
     * author: VuHVP
     * method: clear all the markers exist on map
     */
    var clearMarker = function () {
        for (var i = 0; i < markerArray.length; i++) {
            markerArray[i].setMap(null);
        }
        markerArray.length = 0;
    };

    return {
        init: init
    };
}();

SKYWEB.Admin.BlogPost = function () {
    var init = function () {

    }
    /*
     * author: HuyTV
     * method: Refresh Datatable after any action
     */

    //function RefreshTable() {
    //    var oTable = $('#listBlogPostTable').DataTable();
    //    oTable._fnPageChange(0);
    //    oTable._fnAjaxUpdate();
    //}

    /*
     * author: HuyTV
     * method: Init Datatable when first load
     */
    function InitDatatable() {
        $('#listBlogPostTable').DataTable({
            'bRetrieve': true,
            'bServerSide': true,
            'bScrollCollapse': true,
            'bSort': true,
            'sAjaxSource': window.urls.BlogPost_GetData,
            'bProcessing': true,
            'fnServerParams': function (aoData) {
                //Trước khi chức năng login phân quyền hoàn thành sử dụng 1 số ngẫu nhiên (13)
                //khi hoàn thành rồi thì thay 13 bằng @*@ViewBag.storeId*@
                aoData.push({ 'name': 'id', 'value': 13 });
            },
            'aLengthMenu': [10, 20, 100],
            'oLanguage': {
                'sSearch': 'Tìm kiếm:',
                'sZeroRecords': 'Không có dữ liệu phù hợp',
                'sInfo': 'Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng',
                'sEmptyTable': 'Không có dữ liệu',
                'sInfoFiltered': ' - lọc ra từ _MAX_ dòng',
                'sLengthMenu': 'Hiển thị _MENU_ dòng',
                'sProcessing': 'Đang xử lý...'
            },
            'fnDrawCallback': function (settings) {
                $('.changeStatus').click(function () {
                    $.ajax({
                        url: window.urls.BlogPost_ChangeStatus,
                        type: 'POST',
                        data: {
                            'blogPostId': $(this).attr('data-id')
                        },
                        dataType: 'json',
                        success: function (result) {
                            ReDrawDatatable('listBlogPostTable');
                        }
                    });
                });

                $('#listBlogPostTable_previous').html('<');
                $('#listBlogPostTable_next').html('>');
            },
            'aoColumnDefs': [
                 {
                     'sWidth': '20%',
                     'aTargets': [0],
                     'bSortable': false,
                     'mRender': function (data, type, o) {
                         return '<div class="thumb-sm"><img class="img-responsive" src="' + o[0] + '"></div>';
                     }
                 },
                  {
                      'sWidth': '60%',
                      'aTargets': [1],
                      'bSortable': false,
                      'mRender': function (data, type, o) {
                          return '<a href="' + window.urls.BlogPost_Edit + '?id=' + o[3] + '">' + o[1] + '</a>';
                      }
                  },

          {
              'sWidth': '20%',
              'aTargets': [2],
              'bSortable': false,
              'mRender': function (data, type, o) {
                  if (o[2]) {
                      return '<a class="btn btn-success btn-icon-text waves-effect changeStatus" data-id="' + o[3] + '">Actived </a>';
                  } else {
                      return '<a class="btn btn-danger btn-icon-text waves-effect changeStatus" data-id="' + o[3] + '">Deactived </a>';
                  }


              }
          },
           {
               'sWidth': '8%',
               'aTargets': [3],
               'bVisible': false,
               'bSortable': false,
               'mRender': function (data, type, o) {
                   return o[3];

               }
           }

            ],
            'bAutoWidth': false

        });
    }

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
        $('#' + tableId).dataTable().fnStandingRedraw();
    }

    /*
     * author: HuyTV
     * method: [EVENT] click on 'Kích hoạt' checkbox
     */
    $('#statusChkBox').change(function () {
        if ($(this).prop('checked') == true) {
            $('#status').val('Active');
        } else {
            $('#status').val('Deactive');
        }
    });

    /*
     * author: HuyTV
     * method: [EVENT] SeoName textbox will get data from Title textbox
     */
    $('#Title').change(function () {
        var seoTitle = $('#Title').val();
        $('#SeoName').val(generateSeoTitle(seoTitle));
    });

    /*
    * author: HuyTV (DatVM)
    * method: Format data from Title textbox to Seo
    */
    function generateSeoTitle(origin) {
        var output = removeUnicode(origin);

        output = output.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, '-').toLowerCase();
        // remove first dash
        if (output.charAt(0) == '-') output = output.substring(1);
        // remove last dash
        var last = output.length - 1;
        if (output.charAt(last) == '-') output = output.substring(0, last);

        // Max Length: 255
        if (output.length > 255) {
            output = output.substr(0, 255);
        }

        return output;
    }

    function removeUnicode(str) {
        str = str.toLowerCase();
        str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, 'a');
        str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, 'e');
        str = str.replace(/ì|í|ị|ỉ|ĩ/g, 'i');
        str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, 'o');
        str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, 'u');
        str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, 'y');
        str = str.replace(/đ/g, 'd');
        str = str.replace(/!|@@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'| |\'|\&|\#|\[|\]|~|$|_/g, '-');

        str = str.replace(/-+-/g, '-'); //thay thế 2- thành 1-
        str = str.replace(/^\-+|\-+$/g, '');

        return str;
    }
    return {
        init: init
    }
}();

SKYWEB.Admin.WebPage = function () {
    var init = function () {
        /*
       * author: HuyTCD
       * method: [EVENT] 
       */
        $('.getimagefromelfinder').on('click', function () {
            // get id
            var id = $(this).parent().find('input:first-child').attr('id');
            // set id to controller
            window.open(window.urls.FILE_GetImageFromElfinder + '?elementId=' + id + '', 'GetImageFromElfinder', 'width=1000', 'height=300');
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('#avatar-preview, #images-preview').on('mousedown mousemove', function () {
            $('li[data-placeholder]').removeAttr('style');
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('#images-preview').mousemove(function () {
            var avatarListSize = $('#avatar-preview').children(':not([data-droptarget])').size();
            if (avatarListSize == 0 && swappedItem != null) {
                swappedItem.appendTo('#avatar-preview');
                swappedItem = null;
                return;
            }
            if (avatarListSize <= 1)
                return;
            var placeholder = $('#avatar-preview li[data-placeholder]');
            swappedItem = placeholder.next().size() > 0 ? placeholder.next() : placeholder.prev();
            swappedItem.appendTo('#images-preview');
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('#images-preview').mouseup(function () {
            swappedItem = null;
        });

        /*
         * author: HuyTCD
         * method: [EVENT] 
         */
        $('.images-preview').on('click', '.btn-remove', function () {
            var $me = $(this);
            $me.closest('li').remove();
            if ($me.hasClass('avatar') && $me.filter(':not(avatar)').find('li').length) {
                $me.append($me.filter(':not(avatar)').find('li:first-child'));
            };
        });
        //InitDatatable();
        //RefreshTable();
    }

    /*
    * author: SinhCV
    * method: initElfinderEvent
    */
    var initElfinderEvent = function () {
        $(document).on('click', '#elfinder-selectFile', function () {
            if (selectedFile != null)
                var postData = { values: selectedFile };

            var $avatar = window.opener.$("#avatar-preview"),
                $img = window.opener.$("#images-preview");
            $.ajax({
                type: "POST",
                url: "selectFile",
                data: postData,
                success: function (data) {
                    var name = window.opener.$('#PicURL').attr('data-name');
                    $.each(data, function (i, e) {
                        var $tmpPlaceholder = getPhotoBlock();
                        $tmpPlaceholder.find('.ratio-item').append($('<span/> ', {
                            'class': 'btn-remove',
                            'html': '<i class="glyphicon glyphicon-remove"></i></span>'
                        }));

                        var $input = $tmpPlaceholder.find('input');
                        $input.attr("name", name);
                        $input.val(e);

                        $tmpPlaceholder.find('img').attr('src', e);

                        if (i == 0) {
                            if ($avatar.find("li").length == 0) {
                                $avatar.append($tmpPlaceholder);
                            } else {
                                $img.append($tmpPlaceholder);
                            }
                        } else {
                            $img.append($tmpPlaceholder);
                        }
                    })
                    window.close();
                },
                dataType: "json",
                traditional: true
            });

        });
    }
    /*
     * author: HuyTV
     * method: Refresh Datatable after any action
     */
    //function RefreshTable() {
    //    var oTable = $('#listWebpageTable').dataTable();
    //    oTable._fnPageChange(0);
    //    oTable._fnAjaxUpdate();
    //}

    /*
     * author: HuyTV
     * method: Init Datatable
     */
    function InitDatatable() {
        $('#listWebpageTable').DataTable({
            'bRetrieve': true,
            'bServerSide': true,
            'bScrollCollapse': true,
            'bSort': true,
            'sAjaxSource': window.urls.WebPage_GetData,
            'bProcessing': true,
            'fnServerParams': function (aoData) {
                //Trước khi chức năng login phân quyền hoàn thành sử dụng 1 số ngẫu nhiên (13)
                //khi hoàn thành rồi thì thay 13 bằng @*@ViewBag.storeId*@
                aoData.push({ 'name': 'id', 'value': 13 });
            },
            'aLengthMenu': [10, 20, 100],
            'oLanguage': {
                'sSearch': 'Tìm kiếm:',
                'sZeroRecords': 'Không có dữ liệu phù hợp',
                'sInfo': 'Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng',
                'sEmptyTable': 'Không có dữ liệu',
                'sInfoFiltered': ' - lọc ra từ _MAX_ dòng',
                'sLengthMenu': 'Hiển thị _MENU_ dòng',
                'sProcessing': 'Đang xử lý...'
            },
            'fnDrawCallback': function (settings) {
                $('.changeStatus').click(function () {
                    $.ajax({
                        url: window.urls.WebPage_ChangeStatus,
                        type: 'POST',
                        data: {
                            'webpageId': $(this).attr('data-id')
                        },
                        dataType: 'json',
                        success: function (result) {
                            ReDrawDatatable('listWebpageTable');
                        }
                    });
                });

                $('#listBlogPostTable_previous').html('<');
                $('#listBlogPostTable_next').html('>');
            },
            'aoColumnDefs': [
                {
                    'sWidth': '60%',
                    'aTargets': [1],
                    'bSortable': false,
                    'mRender': function (data, type, o) {
                        //console.log('Test')
                        if (o[1] == null) {
                            o[1] = '';
                        }
                        var tmp = '' + o[1];
                        if (tmp.length > 135) {
                            return tmp.substr(0, 132) + '...';
                        } else {
                            return tmp;
                        }
                    }
                },
                 {
                     'sWidth': '20%',
                     'aTargets': [0],
                     'bSortable': false,
                     'mRender': function (data, type, o) {
                         return '<a href="' + window.urls.WebPage_Edit + '?id=' + o[3] + '">' + o[0] + '</a>';
                     }
                 },
                 {
                     'sWidth': '20%',
                     'aTargets': [2],
                     'bSortable': false,
                     'mRender': function (data, type, o) {
                         if (o[2]) {
                             return '<a class="btn btn-success btn-icon-text waves-effect changeStatus" data-id="' + o[3] + '">Actived </a>';
                         } else {
                             return '<a class="btn btn-danger btn-icon-text waves-effect changeStatus" data-id="' + o[3] + '">Deactived </a>';
                         }

                     }
                 },
                 {
                     'bVisible': false,
                     'aTargets': [3],
                     'bSortable': false,
                     'mRender': function (data, type, o) {
                         return o[3];

                     }
                 },

            ],
            'bAutoWidth': false

        });
    }

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
        $('#' + tableId).dataTable().fnStandingRedraw();
    }

    /*
     * author: HuyTV
     * method: [EVENT] click on 'Kích hoạt' checkbox
     */
    $('#statusChkBox').change(function () {
        //console.log('a');
        if ($(this).prop('checked') == true) {
            $('#status').val('Active');
        } else {
            $('#status').val('Deactive');
        }
    });

    /*
     * author: HuyTV
     * method: [EVENT] SeoName textbox will get data from Title textbox
     */
    $('#Title').change(function () {
        var seoTitle = $('#Title').val();
        $('#SeoName').val(generateSeoTitle(seoTitle));
    });

    /*
   * author: HuyTV (DatVM)
   * method: Format data from Title textbox to Seo
   */
    function generateSeoTitle(origin) {
        var output = removeUnicode(origin);

        output = output.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, '-').toLowerCase();
        // remove first dash
        if (output.charAt(0) == '-') output = output.substring(1);
        // remove last dash
        var last = output.length - 1;
        if (output.charAt(last) == '-') output = output.substring(0, last);

        // Max Length: 255
        if (output.length > 255) {
            output = output.substr(0, 255);
        }

        return output;
    }

    function removeUnicode(str) {
        str = str.toLowerCase();
        str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, 'a');
        str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, 'e');
        str = str.replace(/ì|í|ị|ỉ|ĩ/g, 'i');
        str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, 'o');
        str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, 'u');
        str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, 'y');
        str = str.replace(/đ/g, 'd');
        str = str.replace(/!|@@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'| |\'|\&|\#|\[|\]|~|$|_/g, '-');

        str = str.replace(/-+-/g, '-'); //thay thế 2- thành 1-
        str = str.replace(/^\-+|\-+$/g, '');

        return str;
    }
    return {
        init: init,
        initElfinderEvent: initElfinderEvent
    }
}();

SKYWEB.Admin.ProductCollection = function () {



    /*
    Author: MinhNT
    Mô tả: chỉnh sửa, khởi tạo DataTable
    Param: collection id
   */
    var initDatatable = function () {
        //console.log('js');

        var t = $('#listCollectionTable').DataTable({
            'bRetrieve': true,
            'bServerSide': true,
            'bScrollCollapse': true,
            'bSort': true,
            'sAjaxSource': window.urls.ProductCollection_GetCollections,
            'bProcessing': true,
            'aLengthMenu': [10, 20, 100],
            'oLanguage': {
                'sSearch': 'Tìm kiếm:',
                'sZeroRecords': 'Không có dữ liệu phù hợp',
                'sInfo': 'Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng',
                'sEmptyTable': 'Không có dữ liệu',
                'sInfoFiltered': ' - lọc ra từ _MAX_ dòng',
                'sLengthMenu': 'Hiển thị _MENU_ dòng',
                'sProcessing': 'Đang xử lý...'
            },

            'fnDrawCallback': function (settings) {
                $('.changeStatus').click(function () {
                    $.ajax({
                        url: window.urls.ProductCollection_ChangeCollectionStatus,
                        type: 'POST',
                        data: {
                            'collectionId': $(this).attr('data-id')
                        },
                        dataType: 'json',
                        success: function (result) {
                            reDrawDatatable('listCollectionTable');
                        }
                    });
                });

                $('#listCollectionTable_previous').html('<i class="fa fa-chevron-left"></i>');
                $('#listCollectionTable_next').html('<i class="fa fa-chevron-right"></i>');
            },

            'aoColumnDefs': [
                 {
                     'aTargets': [1],
                     'bSortable': true
                 },

                 {
                     'aTargets': [2],
                     'bSortable': true,
                     'mRender': function (data, type, o) {
                         var title = o[2] ? 'Activated' : 'Deactivated',
                             cls = o[2] ? 'success' : 'danger';
                         return '<a class="btn btn-' + cls + ' btn-icon-text waves-effect changeStatus" data-id="' + o[0] + '">' + title + '</a>';
                     }
                 },
            ],
            'bAutoWidth': false
        });
        //add index column
        t.on('order.dt search.dt', function () {
            t.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
            });
        }).draw();
    }
    /*
     Author: MinhNT
     Mô tả: hàm tạm chua được sử dụng, dùng để chuyển qua trang CollectionDetail
     Param: collection id
    */
    //var loadCollectionDetail = function (id) {
    //    $.ajax({
    //        type: 'GET',
    //        url: '/Admin/ProductCollection/CollectionDetail',
    //        data: {
    //            'id': id,
    //            success: function () {
    //                return true;
    //            }
    //        },
    //    });
    //}

    /*
     Author: MinhNT
     Mô tả: update lại dataTable mà ko fải load lại trang
     Param: tableId 
    
    */
    var reDrawDatatable = function (tableId) {
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
        $('#' + tableId).dataTable().fnStandingRedraw();
    }

    /*
    Author: MinhNT
    Mô tả: load dữ liệu collection bằng ajax
    Param: tableId 
   */
    var loadCollectionDetail = function () {
        $.ajax({
            type: 'GET',
            url: window.urls.ProductCollection_LoadCollectionDetail,
            data: { collectionId: id },
            success: function (result) {
                if (result != null) {
                    var collection = result.collection;
                    $('#collectionName').val(collection.Name);
                    if (collection.Active) {
                        $('#collectionActive').prop('checked', true);
                    }
                    else {
                        $('#collectionActive').prop('checked', false);
                    }
                    $('#collectionDescription').val(collection.Description);
                    $('#collectionSEO').val(collection.SEO);
                    $('#collectionSEODes').val(collection.SEODescription);
                    $('#collectionLink').val(collection.Link);
                }
            },
            error: function (error) {
                alert('Error occured!!!');
            }
        });
    }

    /*
  Author: MinhNT
  Mô tả: update collection info
  Param: tableId 
 */
    var editCollectionInfo = function () {
        var formData = new FormData();
        var name = $('#collectionName').val();
        var description = $('#collectionDescription').val();
        var active = $('#collectionActive').prop('checked');
        var SEO = $('#collectionSEO').val();
        var SEODes = $('#collectionSEODes').val();
        var link = $('#collectionLink').val();

        formData.append('id', id);
        formData.append('name', name);
        formData.append('des', description);
        formData.append('active', active);
        formData.append('SEO', SEO);
        formData.append('SEODes', SEODes);
        formData.append('link', link);

        $.ajax({
            type: 'POST',
            url: window.urls.ProductCollection_UpdateCollection,
            data: formData,
            success: function (result) {
                $(location).attr('href', 'Collections');
            },
            async: false,
            cache: false,
            contentType: false,
            processData: false,
            error: function (error) {
                alert('Error Occured');
            }
        });
    }

    var collectionDetailInit = function () {
        $('#okButton').click(editCollectionInfo);
        var isNew = $('#isNew').val();
        if (isNew == '') {
            loadCollectionDetail();
        }
    }

    return {
        collectionDetailInit: collectionDetailInit,
        initDatatable: initDatatable
    };
}();

SKYWEB.Admin.GeneralInformatinoPage = function () {

    var loadGeneralInformation = function () {
        $.ajax({
            type: 'GET',
            url: window.urls.WebInformation_LoadGeneralInfo,
            data: {},
            success: function (result) {
                if (result != null) {
                    var info = result.info;
                    $('#address').val(info.Address);
                    $('#phone').val(info.Phone);
                    $('#fax').val(info.Fax);
                    $('#storeName').val(info.Name);
                    $('#storeEmail').val(info.Email);
                    if (result.title != null) {
                        $('#titleBlock').show();
                        $('#title').val(result.title);
                    }


                    if (result.slogan != null) {
                        $('#sloganBlock').show();
                        $('#slogan').val(result.slogan);
                    }


                    if (result.description != null) {
                        $('#desBlock').show();
                        $('#storeDescription').val(result.description);

                    }


                }
            },

        });
    }

    var editGeneralInfo = function () {

        var formData = new FormData();

        var name = $('#storeName').val();
        var address = $('#address').val();
        var phone = $('#phone').val();
        var fax = $('#fax').val();
        var email = $('#storeEmail').val();

        formData.append('name', name);
        formData.append('address', address);
        formData.append('phone', phone);
        formData.append('fax', fax);
        formData.append('email', email);

        if ($('#titleBlock').is(':visible')) {
            var title = $('#title').val();
            formData.append('title', title);
        }

        if ($('#sloganBlock').is(':visible')) {
            var slogan = $('#slogan').val();
            formData.append('slogan', slogan);
        }

        if ($('#desBlock').is(':visible')) {
            var description = $('#storeDescription').val();
            formData.append('description', description);
        }

        $.ajax({
            type: 'POST',
            url: window.urls.WebInformation_UpdateGeneralInfo,
            data: formData,
            success: function (result) {
                $(location).attr('href', 'GeneralInformation');
            },
            async: false,
            cache: false,
            contentType: false,
            processData: false,
            error: function (error) {
                alert('Error Occured');
            }
        });
    }


    var init = function () {
        $('#sloganBlock').hide();
        $('#desBlock').hide();
        $('#titleBlock').hide();
        loadGeneralInformation();
        SKYWEB.Admin.WebSetting.init();
        $('#okButton').click(editGeneralInfo);
    }
    return {
        init: init,
    }
}();

SKYWEB.Admin.SocialNetwork = function () {

    var loadSocialInformation = function () {
        $.ajax({
            type: 'GET',
            url: window.urls.WebInformation_LoadSocialDetail,
            data: {},
            success: function (result) {
                if (result != null) {
                    if (result.facebook != null) {
                        $('#facebookBlock').show();
                        $('#facebookLink').val(result.facebook);
                    }

                    if (result.twitter != null) {
                        $('#twitterBlock').show();
                        $('#twitterLink').val(result.twitter);
                    }

                    if (result.zalo != null) {
                        $('#zaloBlock').show();
                        $('#zaloLink').val(result.zalo);
                    }

                    if (result.youtube != null) {
                        $('#youtubeBlock').show();
                        $('#youtubeLink').val(result.youtube);
                    }
                }
            },

        });
    }

    var editSocialInfo = function () {

        var formData = new FormData();
        if ($('#facebookBlock').is(':visible')) {
            var facebook = $('#facebookLink').val();
            formData.append('facebook', facebook);
        }

        if ($('#zaloBlock').is(':visible')) {
            var zalo = $('#zaloLink').val();
            formData.append('zalo', zalo);
        }
        if ($('#youtubeBlock').is(':visible')) {
            var youtube = $('#youtubeLink').val();
            formData.append('youtube', youtube);
        }
        if ($('#twitterBlock').is(':visible')) {
            var twitter = $('#twitterLink').val();
            formData.append('twitter', twitter);
        }

        $.ajax({
            type: 'POST',
            url: window.urls.WebInformation_UpdateSocialInfo,
            data: formData,
            success: function (result) {
                $(location).attr('href', 'SocialNetwork');
            },
            async: false,
            cache: false,
            contentType: false,
            processData: false,
            error: function (error) {
                alert('Error Occured');
            }
        });
    }


    var init = function () {
        //$('#youtubeBlock').hide();
        //$('#twitterBlock').hide();
        //$('#facebookBlock').hide();
        //$('#zaloBlock').hide();
        loadSocialInformation();
        $('#okButton').click(editSocialInfo);
    };
    return {
        init: init,
    }
}();

SKYWEB.Admin.ViewCounter = function () {

    var loadViewCount = function () {
        $.ajax({
            type: 'GET',
            url: window.urls.WebInformation_LoadViewCount,
            data: {},
            success: function (result) {
                if (result != null) {
                    var info = result.info;
                    $('#today').val(info.TodayCount);
                    $('#online').val(info.OnlineCount);
                    $('#thisWeek').val(info.ThisWeekCount);
                    $('#thisMonth').val(info.ThisMonthCount);
                    $('#total').val(info.TotalCount);
                }
            },

        });
    }

    var editViewCountInfo = function () {

        var formData = new FormData();

        var online = $('#online').val();
        var today = $('#today').val();
        var thisWeek = $('#thisWeek').val();
        var thisMonth = $('#thisMonth').val();
        var total = $('#total').val();

        formData.append('online', online);
        formData.append('today', today);
        formData.append('thisWeek', thisWeek);
        formData.append('thisMonth', thisMonth);
        formData.append('total', total);


        $.ajax({
            type: 'POST',
            url: window.urls.WebInformation_UpdateViewCount,
            data: formData,
            success: function (result) {
                $(location).attr('href', 'ViewCount');
            },
            async: false,
            cache: false,
            contentType: false,
            processData: false,
            error: function (error) {
                alert('Error Occured');
            }
        });
    }



    var init = function () {
        loadViewCount();
        $('#okButton').click(editViewCountInfo);
    }

    return {
        init: init,
    }
}();

SKYWEB.Admin.BlogPostCollection = function () {
    /*
    Author: MinhNT
    Mô tả: chỉnh sửa, khởi tạo DataTable
    Param: collection id
   */
    var initDatatable = function () {

        var t = $('#listCollectionTable').DataTable({
            'bRetrieve': true,
            'bServerSide': true,
            'bScrollCollapse': true,
            'bSort': true,
            'sAjaxSource': window.urls.BlogPostCollection_GetCollections,
            'bProcessing': true,
            'aLengthMenu': [10, 20, 100],
            'oLanguage': {
                'sSearch': 'Tìm kiếm:',
                'sZeroRecords': 'Không có dữ liệu phù hợp',
                'sInfo': 'Hiển thị từ _START_ đến _END_ trên tổng số _TOTAL_ dòng',
                'sEmptyTable': 'Không có dữ liệu',
                'sInfoFiltered': ' - lọc ra từ _MAX_ dòng',
                'sLengthMenu': 'Hiển thị _MENU_ dòng',
                'sProcessing': 'Đang xử lý...'
            },

            'fnDrawCallback': function (settings) {
                $('.changeStatus').click(function () {
                    $.ajax({
                        url: window.urls.BlogPostCollection_ChangeCollectionStatus,
                        type: 'POST',
                        data: {
                            'collectionId': $(this).attr('data-id')
                        },
                        dataType: 'json',
                        success: function (result) {
                            reDrawDatatable('listCollectionTable');
                        }
                    });
                });

                $('#listCollectionTable_first').html('<i class="zmdi zmdi-more"></i>');
                $('#listCollectionTable_last').html('<i class="zmdi zmdi-more"></i>');
                $('#listCollectionTable_previous').html('<i class="zmdi zmdi-chevron-left"></i>');
                $('#listCollectionTable_next').html('<i class="zmdi zmdi-chevron-right"></i>');
            },

            'aoColumnDefs': [
                 {
                     'aTargets': [1],
                     'bSortable': true,
                     'mRender': function (data, type, o) {
                         // return '<a href='#' onclick='loadCollectionDetail(' + o[0] + ')'>' + o[1] + '</a>';
                         //@*return '<a href='#' onclick=\'location.href='@Url.Action('', '', new { id = o[0] })'\'> ' + o[1] + '</a>';*@
                         // @*onclick='location.href='@Url.Action('CreatePerson', 'Person', new { ID = ViewData['ID'] })''*@
                         return '<a href="' + window.urls.BlogPostCollection_CollectionDetail + '?id=' + o[0] + '">' + o[1] + ' </a>';
                     }
                 },

                 {
                     'aTargets': [2],
                     'bSortable': true,
                     'mRender': function (data, type, o) {
                         var title = o[2] ? 'Activated' : 'Deactivated',
                             cls = o[2] ? 'success' : 'danger';
                         return '<a class="btn btn-' + cls + ' btn-icon-text waves-effect changeStatus" data-id="' + o[0] + '">' + title + '</a>';
                     }
                 },
            ],
            "bAutoWidth": false,
            "pagingType": "full_numbers"
        });
        //add index column
        t.on('order.dt search.dt', function () {
            t.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                cell.innerHTML = i + 1;
            });
        }).draw();
    }
    /*
     Author: MinhNT
     Mô tả: hàm tạm chua được sử dụng, dùng để chuyển qua trang CollectionDetail
     Param: collection id
    */
    //var loadCollectionDetail = function (id) {
    //    $.ajax({
    //        type: 'GET',
    //        url: '/Admin/ProductCollection/CollectionDetail',
    //        data: {
    //            'id': id,
    //            success: function () {
    //                return true;
    //            }
    //        },
    //    });
    //}

    /*
     Author: MinhNT
     Mô tả: update lại dataTable mà ko fải load lại trang
     Param: tableId 
    
    */
    var reDrawDatatable = function (tableId) {
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
        $('#' + tableId).dataTable().fnStandingRedraw();
    }

    /*
    Author: MinhNT
    Mô tả: load dữ liệu collection bằng ajax
    Param: tableId 
   */
    var loadCollectionDetail = function () {
        $.ajax({
            type: 'GET',
            url: window.urls.BlogPostCollection_LoadCollectionDetail,
            data: { collectionId: id },
            success: function (result) {
                if (result != null) {
                    var collection = result.collection;
                    $('#collectionName').val(collection.Name);
                    if (collection.Active) {
                        $('#collectionActive').prop('checked', true);
                    }
                    else {
                        $('#collectionActive').prop('checked', false);
                    }
                    $('#collectionDescription').val(collection.Description);
                    $('#collectionSEO').val(collection.SEO);
                    $('#collectionSEODes').val(collection.SEODescription);
                    $('#collectionLink').val(collection.Link);
                }
            },
            error: function (error) {
                alert('Error occured!!!');
            }
        });
    }

    /*
  Author: MinhNT
  Mô tả: update collection info
  Param: tableId 
 */
    var editCollectionInfo = function () {
        var formData = new FormData();
        var name = $('#collectionName').val();
        var description = $('#collectionDescription').val();
        var active = $('#collectionActive').prop('checked');
        var SEO = $('#collectionSEO').val();
        var SEODes = $('#collectionSEODes').val();
        var link = $('#collectionLink').val();

        formData.append('id', id);
        formData.append('name', name);
        formData.append('des', description);
        formData.append('active', active);
        formData.append('SEO', SEO);
        formData.append('SEODes', SEODes);
        formData.append('link', link);

        $.ajax({
            type: 'POST',
            url: window.urls.BlogPostCollection_UpdateCollection,
            data: formData,
            success: function (result) {
                $(location).attr('href', 'Collections');
            },
            async: false,
            cache: false,
            contentType: false,
            processData: false,
            error: function (error) {
                alert('Error Occured');
            }
        });
    }

    var collectionDetailInit = function () {
        $('#okButton').click(editCollectionInfo);
        var isNew = $('#isNew').val();
        if (isNew == '') {
            loadCollectionDetail();
        }
    }

    return {
        collectionDetailInit: collectionDetailInit,
        initDatatable: initDatatable
    };
}();

SKYWEB.Admin.Gallery = function () {
    var init = function () {
        setupDragsort();
        /*
         * author: TrungNDT
         * method: open elfinder window with type is "Gallery"
         */
        $('[data-action="add-gallery-item"]').click(function () {
            window.open(window.urls.FILE_GetImageFromElfinder + '?elementId=Gallery', 'GetImageFromElfinder', 'fullscreen=yes');
        });

        /*
         * author: TrungNDT
         * method: Remove selected item
         */
        $('#galleryContainer').on('click', '.removeable .btn-remove', function () {
            var $target = $(this).closest('.removeable');
            if ($target.length) {
                $target.parent().remove();
            }
        });
    }

    /*
     * author: TrungNDT
     * method: Binding events only for _ElfinderTextBox view
     */
    var initElfinderEvent = function () {
        $(document).on('click', '#elfinder-selectFile', function () {
            if (selectedFile != null)
                var postData = { values: selectedFile };
            $.ajax({
                type: "POST",
                url: "selectFile",
                data: postData,
                success: function (data) {
                    var name = window.opener.$('#PicURL').attr('data-name');
                    $.each(data, function (i, e) {
                        var $galleryItem = $($('#tmpGalleryItem').html().trim());
                        $galleryItem.find('img').attr('src', e);
                        $galleryItem.insertBefore(window.opener.$('.gallery-item.plus-item').parent());
                    });
                    window.close();
                },
                dataType: "json",
                traditional: true
            });
        });
    }

    /*
     * author: TrungNDT
     * method: Add a new gallery item with given image url
     * params: {String} url: image url
     */
    var addGalleryItem = function (url) {
        var $galleryItem = $($('#tmpGalleryItem').html().trim());
        $galleryItem.find('img').attr('src', url);
        $galleryItem.insertBefore($('.gallery-item.plus-item').parent());
    }

    /*
     * author: HuyTCD
     * method: set up dragsort for #images-preview element
     */
    var setupDragsort = function () {
        var $tmpPlaceholder = $($('#tmpGalleryItem').html().trim());
        $tmpPlaceholder.find('.gallery-item').addClass('placeHolder');
        $('#galleryContainer').dragsort({
            dragSelector: '.gallery-item:not(.plus-item)',
            dragEnd: function () { },
            placeHolderTemplate: $tmpPlaceholder[0].outerHTML
        });
    }

    return {
        init: init,
        initElfinderEvent: initElfinderEvent
    }
}();


/*
    author: TrungNDT
    rewrite: GiangHNM
    method: [INIT] Auto show loading-gif when ajax function is called
*/
var bindingAjaxLoader = function () {
    $(document).bind('ajaxStart', function () {
        $('.loading-gif').fadeIn();
    }).bind('ajaxStop', function () {
        $('.loading-gif').fadeOut();
    });
}

var delaySearch = 500

var generateSeoTitle = function (origin) {
    var output = removeUnicode(origin);

    output = output.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, "-").toLowerCase();
    // remove first dash
    if (output.charAt(0) == '-') output = output.substring(1);
    // remove last dash
    var last = output.length - 1;
    if (output.charAt(last) == '-') output = output.substring(0, last);

    // Max Length: 255
    if (output.length > 255) {
        output = output.substr(0, 255);
    }

    return output;
}

function removeUnicode(str) {
    str = str.toLowerCase();
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, 'a');
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, 'e');
    str = str.replace(/ì|í|ị|ỉ|ĩ/g, 'i');
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, 'o');
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, 'u');
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, 'y');
    str = str.replace(/đ/g, 'd');
    str = str.replace(/!|@@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'| |\'|\&|\#|\[|\]|~|$|_/g, '-');

    str = str.replace(/-+-/g, '-'); //thay thế 2- thành 1-
    str = str.replace(/^\-+|\-+$/g, '');

    return str;
}

function reDrawDatatableFilter(id, resetStart) {
    $.fn.dataTableExt.oApi.fnStandingRedraw = function (oSettings) {
        if (oSettings.oFeatures.bServerSide === false) {
            var before = oSettings._iDisplayStart;
            oSettings.oApi._fnReDraw(oSettings);
            //iDisplayStart has been reset to zero - so lets change it back
            if (!resetStart) {
                oSettings._iDisplayStart = before;
            }
            oSettings.oApi._fnCalculateEnd(oSettings);
        }
        else {
            if (resetStart) {
                oSettings._iDisplayStart = 0;
            }
        }

        //draw the 'current' page
        oSettings.oApi._fnDraw(oSettings);
    };
    $(id).dataTable().fnStandingRedraw();
}
//user datatable
function RefreshTableFilter(id, resetStart) {
    reDrawDatatableFilter(id, resetStart);
}

//Author: BaoTD
//lock an element at position top: [offset] when reached determined position
//Note: can only be used after document is in state ready
$.fn.fixedScroll = function (offset) {
    var element = $(this);
    var x = element.offset().left;
    var y = element.offset().top;
    var height = element.outerHeight() + 1; //+1 for ceil
    var width = element.outerWidth() + 1;
    var container = element.parent();
    element.css('position', 'fixed');
    element.offset({ top: y, left: x });
    element.css('z-index', '10', 'width');
    element.css('width', width + 'px');
    element.css('height', height + 'px');
    //element.wrap('<div style="height: ' + height + 'px;width: ' + width + 'px;"></div>');

    if ((y - $(document).scrollTop()) < offset) {
        element.css('top', offset + 'px');
        element.css('left', x + 'px');
    } else {
        element.css('top', (y - $(document).scrollTop()) + 'px');
        element.css('left', x + 'px');
    }

    $(window).scroll(function () {
        if ((y - $(document).scrollTop()) < offset) {
            element.css('top', offset + 'px');
            element.css('left', x + 'px');
        } else {
            element.css('top', (y - $(document).scrollTop()) + 'px');
            element.css('left', x + 'px');
        }
    });
}

//Author: BaoTD
//lock an element at position top: [offset] when reached determined position
//Note: element's position must be absolute before calling this plugin
//Note: can only be used after document is in state ready
$.fn.absoluteScroll = function (offset) {
    var element = $(this);
    element.css("z-index", '1000');

    $(window).scroll(function () {
        if (element.css("position") == "absolute") {
            if ($(window).scrollTop() > offset) {
                element.css("top", ($(window).scrollTop() - offset) + 'px');
            } else {
                element.css("top", 'initial');
            }
        }
    });
}