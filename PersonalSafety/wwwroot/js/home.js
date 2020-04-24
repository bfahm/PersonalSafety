import { eraseCookie } from "../lib/app_lib.js";
import { setCookie } from "../lib/app_lib.js";
import { getCookie } from "../lib/app_lib.js";

$(document).ready(function () {
    setTimeout(function () {
        $(".new_content").removeClass("new_content_animation");
        $(".new_content").addClass("new_content_static");
    }, 5000);

    
    var lastBuildDateClientSide = getCookie("LastBuildDateClientSide");
    var lastBuildDateServerSide = getCookie("LastBuildDateServerSide");
    var lastBuildDateNotifyServerSide = getCookie("LastBuildDateNotifyServerSide");
    var lastBuildDateNotifyClientSide = getCookie("LastBuildDateNotifyClientSide");

    if (((lastBuildDateClientSide !== lastBuildDateServerSide) || (lastBuildDateNotifyServerSide !== lastBuildDateNotifyClientSide))
        && (lastBuildDateNotifyServerSide === "True")) {
        $("#btn-new-contents").click();
        setCookie("LastBuildDateClientSide", getCookie("LastBuildDateServerSide"), null);
        setCookie("LastBuildDateNotifyClientSide", getCookie("LastBuildDateNotifyServerSide"), null);
    }
});