var dEvent = $('.event');

// var h = dEvent.attr('data-duration');
// var pos = dEvent.attr('data-start');



$('.event').each(function() {
  var $this = $(this);
  var h = $this.attr('data-duration');
  var pos = $this.attr('data-start');
  $this.css({
    'height' : h * 60,
    'top' : pos * 60
  });
});

$(window).resize(function() {
  if ($(window).width() < '650') {
    $('.event-column').css('margin-left', "60px");
  } else {
    $('.event-column').css('margin-left', "0");
    $('.event-column').css('left', '0')
  }
});


  if ($(window).width() < '650') {
    $('.event-column').css('margin-left', "60px");
  }

$("#goRight").click(function(event) {
  event.preventDefault();
  var left = $('.event-column').css('left');
  var leftVal = parseInt(left.replace("px", ""));
  if( leftVal === 0 ) {
    $('.event-column').css('left', "-114px");
  } else if (leftVal > '-570') {
    var newPos = (leftVal - 114);
    $('.event-column').css({
      left : newPos + 'px'
    });
   }
  $("#goLeft").css("opacity", "1");
});

$("#goLeft").click(function(event) {
  event.preventDefault();
  
  var left = $('.event-column').css('left');
  var leftVal = parseInt(left.replace("px", ""));
  if( leftVal === 0 ) {
    event.preventDefault();
  } else if (leftVal >= '-684' ) {
    var newPos = (leftVal + 114);
    $('.event-column').css({
      left : newPos + 'px'
    });
   }
  $("#goLeft").css("opacity", "1");
});