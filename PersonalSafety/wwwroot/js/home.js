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
    var lastBuildDateNotify = getCookie("LastBuildDateNotify");

    if ((lastBuildDateClientSide !== lastBuildDateServerSide) && (lastBuildDateNotify === "True")) {
        $("#btn-new-contents").click();
        console.log("NEW UPDATE:")
        console.log("Server Last Build was on: " + unescape(str_esc))
        console.log("Client Last Build was on: " + unescape(str_esc))
        console.log("Cookie updated.")
        setCookie("LastBuildDateClientSide", getCookie("LastBuildDateServerSide"), 4320);
    }
});