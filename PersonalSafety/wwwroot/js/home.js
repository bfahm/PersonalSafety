import { eraseCookie } from "../lib/app_lib.js";
import { setCookie } from "../lib/app_lib.js";
import { getCookie } from "../lib/app_lib.js";

var newContentState = "";

$(document).ready(function () {
    setTimeout(function () {
        $(".new_content").removeClass("new_content_animation");
        $(".new_content").addClass("new_content_static");
    }, 5000);

    $(".new_content > div > p").each(function () {
        newContentState += $(this).html()
    });

    var ncId = encodeAndReduce(newContentState);

    var newContentsCookieState_Current = getCookie("ncs");

    if (newContentsCookieState_Current == null || newContentsCookieState_Current !== ncId) {
        $("#btn-new-contents").click();
        console.log("NEW UPDATE:")
        console.log("ncId was: " + unescape(newContentsCookieState_Current))
        console.log("new ncId is: " + ncId);
        setCookie("ncs", ncId, 4320);
    }
});

// Warning: This causes loss of information and returns an Identifier of a string
function encodeAndReduce(input) {
    var b64 = btoa(unescape(encodeURIComponent(input)));

    const [even, odd] = [...b64].reduce((r, char, i) => (r[i % 2].push(char), r), [[], []])
    const [evenLv2, oddLv2] = [...even].reduce((r, char, i) => (r[i % 2].push(char), r), [[], []])
    const [evenLv3, oddLv3] = [...even].reduce((r, char, i) => (r[i % 2].push(char), r), [[], []])

    return evenLv3.join('').substring(0, 3000);
}