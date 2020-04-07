import { checkForQuery } from "../lib/app_lib.js";
import { eraseCookie } from "../lib/app_lib.js";

"use strict";

$(document).ready(function () {
    if (checkForQuery("needsUpdate")) {
        $('#updateModal').modal('show');
    }

    $("#btn_recheck").click(function () {
        eraseCookie("update_service_last_checked");
        eraseCookie("update_service_is_updated");
        window.location = "/"; 
    });
});