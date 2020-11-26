function getDirections(address) {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            window.location.href = 'https://maps.google.com/maps?saddr=' +
                position.coords.latitude.toString() + ', ' + position.coords.longitude.toString() + '&daddr=' + encodeURIComponent(address);
        });
    } else {
        x.innerHTML = "Geolocation is not supported by this browser.";
    }
}

$(document).ready(
    $(".distance").map(function () {
        jQuery.support.cors = true;
        var id = $(this).attr('id');
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function (position) {
                $.ajax({
                    url: "https://zkhiphava.co.za/api/GetDistance?lat1="
                        + position.coords.latitude.toString() + '&lon1=' + position.coords.longitude.toString()
                        + '&indawoId=' + id.toString(),
                    type: "GET",
                    error: function (request, status, error) {
                        console.error(request.responseText);
                    },
                    success: function (data) {
                        document.getElementById(id).innerHTML = data + 'KM';
                    }
                });
            });
        } else {
            x.innerHTML = "Geolocation is not supported by this browser.";
        }
    })
);