function CloseWindowsFromAjax() {
    $('#WindowsFromAjax').dialog('close');
}




function ConvertDateVNToUS(dateTime) {
    if (dateTime == "" || dateTime == null) {
        return "";
    }
    
    var parts = "";
    if (dateTime.indexOf("/") > -1) {
        parts = dateTime.split("/");
    }
    else if (dateTime.indexOf("-") > -1) {
        parts = dateTime.split("-");
    }
    var newDateTime = parts[1] + "/" + parts[0] + "/" + parts[2];
    return newDateTime;

}

function ConvertDateVNToUS2(dateTime) {

    var parts = "";
    if (dateTime.indexOf("/") > -1) {
        parts = dateTime.split("/");
    }
    else if (dateTime.indexOf("-") > -1) {
        parts = dateTime.split("-");
    }
    var newDateTime = parts[0] + "-" + parts[1] + "-" + parts[2];
    return newDateTime;

}



function addCommas(n) {
    var rx = /(\d+)(\d{3})/;
    return String(n).replace(/\d+/, function (w) {
        while (rx.test(w)) {
            w = w.replace(rx, '$1,$2');
        }
        return w;
    });
}


//function addOrder(data) {
//    var roomNumber = $(data).attr('name');
//    var roomId = $(data).attr('id');
//    $('.formRoomNumber').each(function (index, element) {
//        $(this).text(roomNumber);
//    });
//    $('.txtRoomId').each(function (index, element) {
//        $(this).val(roomId);
//    });
//    $('#formThanhToan').dialog('open');
//}


$(document).ready(function (e) {

   
    $.timepicker.regional['vi'] = {
        timeOnlyTitle: 'Thời gian',
        timeText: 'Thời gian',
        hourText: 'Giờ',
        minuteText: 'Phút',
        secondText: 'Giây',
        millisecText: 'mili Giây',
        currentText: 'Bây giờ',
        closeText: 'Chọn',
        ampm: false
    };
    $.timepicker.setDefaults($.timepicker.regional['vi']);
    $.datepicker.regional['vi'] = {
        closeText: 'Đóng',
        prevText: 'Trước',
        nextText: 'Sau',
        currentText: 'Hôm nay',
        monthNames: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
	'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
        monthNamesShort: ['1', '2', '3', '4', '5', '6',
	'7', '8', '9', '10', '11', '12'],
        dayNames: ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'],
        dayNamesShort: ['вск', 'пнд', 'втр', 'срд', 'чтв', 'птн', 'сбт'],
        dayNamesMin: ['CN', 'H', 'B', 'T', 'N', 'S', 'B'],
        weekHeader: 'Не',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['vi']);
//    $('#Rent_CheckInDate').datetimepicker({ dateFormat: 'dd-mm-yy' });
//    $('#Rent_CheckInDate').datetimepicker('setDate', (new Date()));
//    $('#CheckOutDate').datetimepicker();
});



function addCommas(nStr) {
    nStr += '';
    var x = nStr.split('.');
    var x1 = x[0];
    var x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}













