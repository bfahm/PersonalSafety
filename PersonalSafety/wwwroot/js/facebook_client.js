import { copyToClipboard } from "../lib/app_lib.js";

//Facebook boiler plate code
window.fbAsyncInit = function () {
    FB.init({
        appId: '2938465296176364',
        cookie: true,
        xfbml: true,
        version: 'v6.0'
    });

    FB.AppEvents.logPageView();

};

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "https://connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));


window.checkLoginState = function checkLoginState() {
    FB.getLoginStatus(function (response) {
        $("#result_access_token").val(response.authResponse.accessToken.toString());

        $("#hidden_div_till_connected").removeAttr('hidden');
    });
}


$(document).ready(function () {
    $("#btn_copy_to_clipboard").click(function () {
        copyToClipboard($("#result_access_token").val());
        $(this).html("Copied")
    });
});