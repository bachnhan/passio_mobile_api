
/*Bản đồ*/

$(function () {

    $(document).ready(function () {
        autocomplete = new google.maps.places.Autocomplete((document.getElementById('Address')), { types: ['geocode'] });

        google.maps.event.addListener(autocomplete, 'place_changed', function () {
            fillInAddress();
        });

        $(function () {
            //$("#map-canvas").width('100%').height(300);
            google.maps.event.addDomListener(window, 'load', initialize);
        });

    });

    var map;
    var mapOptions;
    var lat, long;
    function initialize() {
        var styles = [{ featureType: "road.highway", stylers: [{ visibility: "off" }] }, { featureType: "landscape", stylers: [{ visibility: "off" }] }, { featureType: "transit", stylers: [{ visibility: "off" }] }, { featureType: "poi", stylers: [{ visibility: "off" }] }, { featureType: "poi.park", stylers: [{ visibility: "on" }] }, { featureType: "poi.park", elementType: "labels", stylers: [{ visibility: "off" }] }, { featureType: "poi.park", elementType: "geometry.fill", stylers: [{ color: "#d3d3d3" }, { visibility: "on" }] }, { featureType: "poi.medical", stylers: [{ visibility: "off" }] }, { featureType: "poi.medical", stylers: [{ visibility: "off" }] }, { featureType: "road", elementType: "geometry.stroke", stylers: [{ color: "#cccccc" }] }, { featureType: "water", elementType: "geometry.fill", stylers: [{ visibility: "on" }, { color: "#cecece" }] }, { featureType: "road.local", elementType: "labels.text.fill", stylers: [{ visibility: "on" }, { color: "#808080" }] }, { featureType: "administrative", elementType: "labels.text.fill", stylers: [{ visibility: "on" }, { color: "#808080" }] }, { featureType: "road", elementType: "geometry.fill", stylers: [{ visibility: "on" }, { color: "#fdfdfd" }] }, { featureType: "road", elementType: "labels.icon", stylers: [{ visibility: "off" }] }, { featureType: "water", elementType: "labels", stylers: [{ visibility: "off" }] }, { featureType: "poi", elementType: "geometry.fill", stylers: [{ color: "#d2d2d2" }] }];

        // Create a new StyledMapType object, passing it the array of styles,
        // as well as the name to be displayed on the map type control.
        var styledMap = new google.maps.StyledMapType(styles,
        { name: "Styled Map" });

        mapOptions = {
            center: new google.maps.LatLng(10.789886817455374, 106.6787300934875),
            zoom: 14,
            mapTypeControlOptions: {
                mapTypeIds: [google.maps.MapTypeId.ROADMAP, 'map_style']
            }
        };

        map = new google.maps.Map(document.getElementById("map-canvas"),
            mapOptions);
        map.mapTypes.set('map_style', styledMap);
        map.setMapTypeId('map_style');



        window.map = map;
        window.geocoder = new google.maps.Geocoder();
        //if ($('#lati').val() != "" && $('#long').val() != "") {
        //    var img = "/Content/images/m_terminal.png";
        //    var marker = new google.maps.Marker({
        //        position: new google.maps.LatLng($('#lati').val(), $('#long').val()),
        //        map: window.map,
        //        title: $('#Name').val(),
        //        icon: img
        //    })
        //    window.map.setCenter(marker.getPosition());
        //getDistrictviaGeocoder($('#lati').val(), $('#long').val());


        //}

    }

    function fillInAddress() {
        // Get the place details from the autocomplete object.
        var place = autocomplete.getPlace();
        locationSearch();
    }

    function getDistrictviaGeocoder(lati, long) {
        var geocoder = new google.maps.Geocoder();
        var latLng = new google.maps.LatLng(lati, long);
        geocoder.geocode({
            latLng: latLng
        },
        function (responses) {
            if (responses && responses.length > 0) {
                var addrComp = responses[0].address_components;
                $.each(addrComp, function (i, address_component) {
                    if (address_component.types[0] == "administrative_area_level_2") {
                        var district = address_component.long_name
                        if (district == null) {
                            $('#District').val('Không rõ');
                        }
                        $('#District').val(district);
                    }
                    if (address_component.types[0] == "administrative_area_level_1") {
                        var city = address_component.long_name;
                        if (city == null) {
                            $('#City').val('Không rõ');
                        }
                        $('#City').val(city);
                    }

                });
            }
            else {
                alert('Không tìm được quận! Xin chọn địa chỉ phù hợp!');
            }
        });

    }

    var resultMarker = null;

    function locationSearch() {
        var address = $("#Address").val();

        window.geocoder.geocode({ 'address': address }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
                map.setCenter(results[0].geometry.location);
                if (resultMarker != null) {
                    resultMarker.setMap(null);
                }
                var latitude = results[0].geometry.location.lat();
                var longitude = results[0].geometry.location.lng();
                getDistrictviaGeocoder(latitude, longitude);
                resultMarker = new google.maps.Marker({
                    map: map,
                    position: results[0].geometry.location
                });
            } else {
                ShowMessage("Geocode không thành công vì: " + status, 1);
            }
        });

    };
});