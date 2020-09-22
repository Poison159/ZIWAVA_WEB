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