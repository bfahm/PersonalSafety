import { parseJwt } from "../lib/app_lib.js";
import { animateProgressBar } from "../lib/app_lib.js";
import { loginViaAjax } from "../lib/app_lib.js";
import { logoutUser } from "../lib/app_lib.js";
import { tokenFactory } from "../lib/app_lib.js";
import { simpleGet } from "../lib/app_lib.js";
import { simplePut } from "../lib/app_lib.js";

"use strict";

var connection;
var consoleIsOnline = false;
var cookieDetails = {
    cookieTokenKey: "token",
    cookieRefreshTokenKey: "refreshToken"
};

$(document).ready(function () {
    initializeButtons();
    tokenFactory(cookieDetails)
        .then(function (token) {
            loginAnimationHandler(false, token);
        })
        .catch(function (fatal) {
            if (fatal === true) {
                logoutUser(cookieDetails);
            }
        });
});

// API Functions
function retrieveTrackers() {
    tokenFactory(cookieDetails)
        .then(function (token) {
            simpleGet('api/Admin/Management/RetrieveTrackers', token)
                .then(function (result) {
                    animateProgressBar("retrieve_connection_bar");

                    $("#table_container").empty();
                    for (var x in result.result) {
                        if (Object.prototype.hasOwnProperty.call(result.result, x)) {
                            loadConnectionsTable(x, result.result[x]);
                        }
                    }
                    $('#table_container').children().last().remove();
                    $('#table_container').children().last().remove();
                });
        });
}

function retrieveConsole() {
    tokenFactory(cookieDetails)
        .then(function (token) {
            simpleGet('api/Admin/Management/RetrieveConsole', token)
                .then(function (result) {
                    animateProgressBar("retrieve_console_bar");
                    loadConsoleContents(result.result);
                    setTimeout(function () {
                        $("#console_container").attr("hidden", false);
                    }, 1000); //time before animation starts execution
                });
        });
}

function clearConsole() {
    tokenFactory(cookieDetails)
        .then(function (token) {
            simplePut('api/Admin/Management/ResetConsole', token);
        });
}

function clearTrackers() {
    tokenFactory(cookieDetails)
        .then(function (token) {
            simplePut('api/Admin/Management/ResetTrackers', token);
        });
}

function resetClientState(email) {
    tokenFactory(cookieDetails)
        .then(function (token) {
            return new Promise(function (resolve, reject) {
                $.ajax({
                    url: `/api/Admin/Management/ResetClientState?clientEmail=${email}`,
                    type: 'PUT',
                    beforeSend: function (xhr) {   //Include the bearer token in header
                        xhr.setRequestHeader("Authorization", 'Bearer ' + token);
                    },
                    success: function () {
                        console.log("done");
                        resolve();
                    },
                    error: function () {
                        console.error("error while resetting client");
                        reject();
                    }
                });
            });
        });
}

function resetRescuerState(email) {
    tokenFactory(cookieDetails)
        .then(function (token) {
            return new Promise(function (resolve, reject) {
                $.ajax({
                    url: `/api/Admin/Management/ResetRescuerState?rescuerEmail=${email}`,
                    type: 'PUT',
                    beforeSend: function (xhr) {   //Include the bearer token in header
                        xhr.setRequestHeader("Authorization", 'Bearer ' + token);
                    },
                    success: function () {
                        console.log("done");
                        resolve();
                    },
                    error: function () {
                        console.error("error while resetting rescuer");
                        reject();
                    }
                });
            });
        });
}

// UI Functions

function initializeButtons() {
    //Allow for enter keypress to submit entries
    $('#input_password, #input_email').keypress(function (e) {
        if (e.keyCode === 13)
            $('#btn_login').click();
    });

    //Login Button Action
    $("#btn_login").click(function () {
        var email = $("#input_email").val();
        var password = $("#input_password").val();

        loginViaAjax(email, password, cookieDetails, true)
            .then(function (token, refreshToken) {
                loginAnimationHandler(true, token);
            }).catch(function() {
                $("#login_result").html("Wrong username or password..");
            });
    });

    //Logout Button Action
    $("#link_logout").click(function () {
        logoutUser(cookieDetails);
    });

    //Logout Button Action
    $("#a_refresh").click(function () {
        location.reload();
    });


    $("#btn_retrieve").click(function () {
        retrieveTrackers();

        $(this).removeClass("btn-secondary");
        $(this).addClass("btn-primary");
        $("#retrieve_main_container").addClass("border-primary");
    });

    $("#btn_console_launch").click(function () {
        retrieveConsole();

        $(this).removeClass("btn-secondary");
        $(this).addClass("btn-primary");
        $("#console_main_container").addClass("border-primary");
    });

    $("#btn_clear").click(function () {
        loadConsoleContents("");
        clearConsole();
    });

    $("#btn_reset_trackers").click(function () {
        loadConnectionsTable("", "");
        clearTrackers();
    });

    $("#btn_reset_client").click(function () {
        var email = $("#field_reset_client").val();
        resetClientState(email);
        animateProgressBar("progress_bar_reset_client");
        $(this).removeClass("btn-secondary");
        $(this).addClass("btn-primary");
        $("#reset_client_main_container").addClass("border-primary");
    });

    $("#btn_reset_rescuer").click(function () {
        var email = $("#field_reset_rescuer").val();
        resetRescuerState(email);
        animateProgressBar("progress_bar_reset_rescuer");
        $(this).removeClass("btn-secondary");
        $(this).addClass("btn-primary");
        $("#reset_rescuer_main_container").addClass("border-primary");
    });
}

function loginAnimationHandler(toAnimate, token) {
    //login animations and element swaps

    var email = parseJwt(token).sub;

    startConnection(token);
    $("#current_email").html(email);
    $("#login_form").attr("hidden", true);
    $("#logged_in_notifier").attr("hidden", false);
    $("#section-tools").attr("hidden", false);

    if (toAnimate) {
        setTimeout(function () {
            $([document.documentElement, document.body]).animate({
                scrollTop: $("#section-tools").offset().top
            }, 300); //time for animation to execute
        }, 750); //time before animation starts execution
    }
}

function loadConnectionsTable(jsonKey, jsonList) {
    $("#retrieve_result").attr("hidden", true);

    var firstItem = jsonList[0];
    if (firstItem) {

        //table header
        var tableHeader = '<h5 class="float-left">' + jsonKey + '</h5>';
        tableHeader += '<table id="table_' + jsonKey + '" class="table table-sm table-hover">';
        tableHeader += '<thead>';
        tableHeader += '<tr>';
        tableHeader += '<th scope="col">#</th>';
        
        for (var x in firstItem) {
            if (Object.prototype.hasOwnProperty.call(firstItem, x)) {
                tableHeader +='<th scope="col">'+x+'</th>';
            }
        }
        tableHeader += '</tr>';
        tableHeader += '</thead>';
        tableHeader += '</table>';

        $('#table_container').append(tableHeader);

        //table data

        var tbody = $("<tbody />"), tr;
        var counter = 1;
        $.each(jsonList, function (_, obj) {
            tr = $("<tr />");
            tr.append("<td>" + counter + "</td>");
            $.each(obj, function (_, text) {
                
                tr.append("<td>" + text + "</td>");
                
            });
            counter += 1;
            tr.appendTo(tbody);
        });

        tbody.appendTo('#table_'+jsonKey); // only DOM insertion

        //splitter
        $('#table_container').append('<br/>');
        $('#table_container').append('<hr/>');

        
        setTimeout(function () {
            $("#retrieve_result").attr("hidden", false);
        }, 1000); //time before animation starts execution
    }
}

function loadConsoleContents(parsedJson) {
    // Clear current table entries first
    $("#console_container").empty();

    for (var i = 0; i < parsedJson.length; i++) {
        var obj = parsedJson[i];
        

        $('#console_container').append('<p> > ' + obj + '</p>');
    }

    if (parsedJson.length === 0 || parsedJson === null) {
        $('#console_container').append('<p id="empty_console_pointer"> > </p>');
    }
}

function addToConsoleContents(text) {
    $('#console_container').append('<p> > ' + text + '</p>');
}

// SignalR related code..
function startConnection(token) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/admin", {
            accessTokenFactory: () => token
        })
        .build();

    connection.on("AdminConsoleChanges", function (message) {
        $("#empty_console_pointer").remove();
        addToConsoleContents(message);
    });

    // Start connection after finishing all the settings
    connection.start()
        .catch(function (err) {
        return console.error(err.toString());
        });

    connection.onclose(function () {
        $('html, body').animate({
            scrollTop: 0
        }, 500);

        $("#alert_container_disconnected").removeAttr('hidden');
    });
}