import { checkForQuery } from "../lib/app_lib.js";

"use strict";

$(document).ready(function () {
    if (checkForQuery("needsUpdate")) {
        $('#updateModal').modal('show');
    }
});