var markerList = [];
var createMarker;
var map;
// This example adds a search box to a map, using the Google Place Autocomplete
// feature. People can enter geographical searches. The search box will return a
// pick list containing a mix of places and predicted search terms.
function initialize(search, lati, long, mapId) {
    var lat = parseFloat($('#' + lati).val() || 10.777123);
    var lng = parseFloat($('#' + long).val() || 106.695457);

    //var lat = parseFloat(10.777123);
    //var lng = parseFloat(106.695457);
    jQuery('#' + lati).val(lat);
    jQuery('#' + long).val(lng);

    //var lat = jQuery('#' + lati).val();
    //var lng = jQuery('#' + long).val();
    //console.log(lat + ', ' + lng);
    var mapProp = {
        center: new google.maps.LatLng(lat, lng),
        zoom: 15,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
    };
    map = new google.maps.Map(document.getElementById(mapId), mapProp);

    if (jQuery('#' + lati).val() != "") {
        createDefaultMarker(lat, lng, map);
    }

    // Create the search box and link it to the UI element.
    var input = document.getElementById(search);
    var searchBox = new google.maps.places.SearchBox((input));


    //[START region_getplaces]
    //Listen for the event fired when the user selects an item from the
    //pick list. Retrieve the matching places for that item.
    google.maps.event.addListener(searchBox, 'places_changed', function () {

        var places = searchBox.getPlaces();

        if (places.length == 0) {
            return;
        }
        //for (var i = 0, marker; marker = markerList[i]; i++) {
        //    marker.setMap(null);
        //}
        if (createMarker)
            createMarker.setMap(null);
        // For each place, get the icon, place name, and location.
        //markerList = [];
        var bounds = new google.maps.LatLngBounds();
        for (var j = 0, place; place = places[j]; j++) {
            var image = {
                //url: place.icon,
                url: '../../Content/images/marker.png',
                //size: new google.maps.Size(71, 71),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(17, 34),
                scaledSize: new google.maps.Size(30, 40)
            };
            //console.log(image.url);
            // Create a marker for each place.
            createMarker = new google.maps.Marker({
                map: map,
                //icon: image, // Set icon for marker
                title: place.name,
                position: place.geometry.location
            });
            //markerList.push(marker);
            createMarker.setMap(map);
            bounds.extend(place.geometry.location);
        }

        map.fitBounds(bounds);
        map.setZoom(15);
        google.maps.event.trigger(map, "resize");
        var centerPos = bounds.getCenter();
        jQuery('#' + lati).val(centerPos.lat());
        jQuery('#' + long).val(centerPos.lng());
        jQuery('#submit-location-form-btn').prop('disabled', false);

    });
    // [END region_getplaces]


    /**
     * author: TrungNDT
     * method: Catching clicked point and add a marker to that point
     */
    google.maps.event.addListener(map, "click", function (event) {
        //deleteOverlays();
        if (createMarker)
            createMarker.setMap(null);
        var latitude = event.latLng.lat();
        var longitude = event.latLng.lng();
        //console.log(latitude + ', ' + longitude);
        var myLatlng = new google.maps.LatLng(latitude, longitude);
        createMarker = new google.maps.Marker({
            position: myLatlng
        });
        createMarker.setMap(map);
        jQuery('#' + lati).val(latitude);
        jQuery('#' + long).val(longitude);
    }); //end addListener

    // Bias the SearchBox results towards places that are within the bounds of the
    // current map's viewport.
    google.maps.event.addListener(map, 'bounds_changed', function () {
        var bounds = map.getBounds();
        searchBox.setBounds(bounds);
    });
    google.maps.event.addListenerOnce(map, 'idle', function () {
        lastCenter = map.getCenter();
        google.maps.event.trigger(map, 'resize');
        map.setCenter(lastCenter);
    });

}

function createDefaultMarker(lat, lng, map) {
    var myLatlng = new google.maps.LatLng(lat, lng);
    createMarker = new google.maps.Marker({
        position: myLatlng
    });
    createMarker.setMap(map);
}

function getmarkerList() {
    jQuery.each(markerList, function (i, e) {
        //console.log(e.title);
    });
}

