var url = "api/Gallery/Logo";
var logo = document.getElementById("logo");
var loaded = true;

logo.onload = function () {
    loaded = true;
};
logo.onclick = function () {
    if (loaded) {
        loaded = false;
        logo.src = url + "?t=" + new Date().getTime();
    }
};
