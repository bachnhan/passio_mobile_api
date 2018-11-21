HMS.skin = function () {
    var getSkin = function (skinId) {
        var skin = {
            passio: {
                '@color-main': '#97b22b',
                '@color-success': '#2a9fd6',
                '@color-info': '#9933cc',
                '@color-warning': '#ff8800',
                '@color-danger': '#cc0000'
            },
            united: {
                '@color-main': '#dd4814',
                '@color-success': '#38b44a',
                '@color-info': '#772953',
                '@color-warning': '#efb73e',
                '@color-danger': '#df382c'
            },
            kfc: {
                '@color-main': '#990000',
                '@color-success': '#38b44a',
                '@color-info': '#68adde',
                '@color-warning': '#efb73e',
                '@color-danger': '#debb27'
            },
            amelia: {
                '@color-main': '#ad1d28',
                '@color-success': '#48ca3b',
                '@color-info': '#4d3a7d',
                '@color-warning': '#df6e1e',
                '@color-danger': '#debb27'
            },
            journal: {
                '@color-main': '#eb6864',
                '@color-success': '#22b24c',
                '@color-info': '#336699',
                '@color-warning': '#f5e625',
                '@color-danger': '#f57a00'
            }
        }
        return skin[skinId];
    }

    var configSkyplusSkin = function (skinId) {
        //$.ajax({
        //    url: '/Home/GetSkin',
        //    dataType: 'JSON',
        //    success: function (result) {
        //        var skinConfig = getSkin(result.skinId);
        //        less.modifyVars(skinConfig);
        //    }
        //});
        var skinConfig = getSkin(skinId);
        less.modifyVars(skinConfig);
    }

    return {
        configSkyplusSkin: configSkyplusSkin
    }
}();

//HMS.skin.configSkyplusSkin('kfc');