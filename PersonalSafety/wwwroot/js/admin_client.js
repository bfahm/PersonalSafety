import { parseJwt } from "../lib/app_lib.js";
import { animateProgressBar } from "../lib/app_lib.js";
import { loginViaAjax } from "../lib/app_lib.js";
import { logoutUser } from "../lib/app_lib.js";
import { tokenFactory } from "../lib/app_lib.js";
import { simpleGet } from "../lib/app_lib.js";
import { simplePut } from "../lib/app_lib.js";

"use strict";

var roomConnection;
var cookieDetails = {
    cookieTokenKey: "token",
    cookieRefreshTokenKey: "refreshToken"
};
var consoleLaunched = false;

$(document).ready(function () {
    initializeButtons();

    tokenFactory(cookieDetails)
        .then(function (token) {
            loginUiHandler(token);
        })
        .catch(function (fatal) {
            if (fatal === true) {
                logoutUser(cookieDetails);
            }
            loginUiHandler(null); // Process a failed auto login..
        });
});

// API Functions
function retrieveTrackers() {
    tokenFactory(cookieDetails)
        .then(function (token) {
            simpleGet('api/Admin/Technical/RetrieveTrackers', token)
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
            simpleGet('api/Admin/Technical/RetrieveConsole', token)
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
            simplePut('api/Admin/Technical/ResetConsole', token);
        });
}

function clearTrackers() {
    tokenFactory(cookieDetails)
        .then(function (token) {
            simplePut('api/Admin/Technical/ResetTrackers', token);
        });
}

function resetClientState(email) {
    tokenFactory(cookieDetails)
        .then(function (token) {
            return new Promise(function (resolve, reject) {
                $.ajax({
                    url: `/api/Admin/Technical/ResetClientState?clientEmail=${email}`,
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
                    url: `/api/Admin/Technical/ResetRescuerState?rescuerEmail=${email}`,
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

        $("#btn_connect_animation").attr("hidden", false);
        $("#btn_connect_label").attr("hidden", true);

        loginViaAjax(email, password, cookieDetails, true)
            .then(function (token, refreshToken) {
                loginUiHandler(token);
            }).catch(function() {
                $("#login_result").html("Wrong username or password..");

                $("#btn_connect_animation").attr("hidden", true);
                $("#btn_connect_label").attr("hidden", false);
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

        $(this).val("Refresh");
        consoleLaunched = true;
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

    $("#btn_join").click(function () {
        var roomName = $("#field_department_name").val();
        $("#btn_Leave").click();

        tokenFactory(cookieDetails)
            .then(function (token) {
                joinDepartmentRoom(token, roomName)
            })
    });

    $("#btn_Leave").click(function () {
        $('#chat_container').empty();
        $('#field_department_name').val('');
        $("#chat_container_wrapper").attr("hidden", true);
        leaveDepartmentRoom();
    });
}

function loginUiHandler(token) {
    //login animations and element swaps

    if (token == null) {
        $("#logging_in_div").attr("hidden", true);
        $("#login_form").attr("hidden", false);
        return;
    }

    var email = parseJwt(token).sub;

    startConnection(token);
    $("#current_email").html(email);
    $("#login_form").attr("hidden", true);
    $("#logged_in_notifier").attr("hidden", false);
    $("#section-tools").attr("hidden", false);

    $("#logging_in_div").attr("hidden", true);

    setTimeout(function () {
        $([document.documentElement, document.body]).animate({
            scrollTop: $("#section-tools").offset().top
        }, 300); //time for animation to execute
    }, 750); //time before animation starts execution
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
        var text = parsedJson[i];
        addToConsoleContents(text, false)
    }

    if (parsedJson.length === 0 || parsedJson === null) {
        $('#console_container').append('<p id="empty_console_pointer"> > </p>');
    }
}

function addToConsoleContents(text, toScroll) {
    var splitsplit = text.split("|");
    var datePart = splitsplit[0]
    var datePartSpan = `<span style='color: Chartreuse'> ${datePart} </span>`

    // Only keep the numbers in the date part to be used as an ID
    datePart = datePart.replace(/\s+/g, '');
    datePart = datePart.replace(/:+/g, '');
    datePart = datePart.replace(/-+/g, '');
    datePart = datePart.replace('.', '');

    splitsplit.shift();
    var msgPart = splitsplit.join()

    var pElemenetId = `p_${datePart}`
    $('#console_container').append(`<p id="${pElemenetId}"> > ${datePartSpan} | ${msgPart} </p>`);

    if (toScroll && consoleLaunched) {
        $([document.documentElement, document.body]).animate({
            scrollTop: $(`#${pElemenetId}`).offset().top - 500
        }, 100);
    }
}

function addToRoomContents(text) {
    var datePart = new Date().toLocaleTimeString();
    var datePartSpan = `<span style='color: Chartreuse'> ${datePart} </span>`

    var pElemenetId = `p_${datePart}`
    $('#chat_container').append(`<p id="${pElemenetId}"> > ${datePartSpan} | ${text} </p>`);

    var chatArea = document.getElementById('chat_container');
    chatArea.scrollTop = chatArea.scrollHeight
}

// SignalR related code..
function startConnection(token) {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/admin", {
            accessTokenFactory: () => token
        })
        .build();

    connection.on("AdminConsoleChanges", function (message) {
        $("#empty_console_pointer").remove();
        addToConsoleContents(message, true);
    });

    connection.on("AdminFCMChannel", function (message) {
        if (message === true) {
            $("#fcm_master_switch_on").removeClass("btn-secondary")
            $("#fcm_master_switch_on").addClass("btn-success")
            $("#fcm_master_switch_on").html("Service Up")

            $("#fcm_master_switch_off").removeClass("btn-danger")
            $("#fcm_master_switch_off").addClass("btn-secondary")
            $("#fcm_master_switch_off").html("Turn Off")

        } else if (message === false) {
            $("#fcm_master_switch_on").removeClass("btn-success")
            $("#fcm_master_switch_on").addClass("btn-secondary")
            $("#fcm_master_switch_on").html("Turn On")

            $("#fcm_master_switch_off").removeClass("btn-secondary")
            $("#fcm_master_switch_off").addClass("btn-danger")
            $("#fcm_master_switch_off").html("Service Down")
        }
    });

    connection.on("AdminClientTrackingChannel", function (type, value) {
        if (type === "minutes") {
            $("#minutes_skew_ip").val(value);
        } else if (type === "meters") {
            $("#meters_skew_ip").val(value);
        }
    });

    $("#minutes_skew_btn").click(function () {
        var strValue = $("#minutes_skew_ip").val();
        connection.invoke("SetMinutesSkew", parseInt(strValue));
    });

    $("#meters_skew_btn").click(function () {
        var strValue = $("#meters_skew_ip").val();
        connection.invoke("SetMetersSkew", parseInt(strValue));
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

    $("#btn_send_notification").click(function () {
        animateProgressBar("send_notification_bar");
        var registration = $("#field_device_registration").val();
        var title = $("#field_notification_title").val();
        var body = $("#field_notification_body").val();
        connection.invoke("SendTestNotification", registration, title, body);
    });
}

function joinDepartmentRoom(token, roomName) {
    roomConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/location ", {
            accessTokenFactory: () => token
        })
        .build();

    roomConnection.on("LocationChannel", function (email, lat, long) {
        addToRoomContents(email + " | " + lat + ", " + long)
    });

    roomConnection.on("InfoChannel", function (message) {
        addToRoomContents(message)
    });

    roomConnection.on("AlertsChannel", function (message) {
        addToRoomContents(message)
    });

    // Start connection after finishing all the settings
    roomConnection.start()
        .then(function () {
            animateProgressBar("monitor_rescuers_bar");
            $("#chat_container_wrapper").attr("hidden", false);
            roomConnection.invoke("EnterDepartmentRoom", roomName);
        })
        .catch(function (err) {
            return console.error(err.toString());
        });
}

function leaveDepartmentRoom() {
    if (roomConnection != null) {
        roomConnection.stop();
        roomConnection = null;
    }
}