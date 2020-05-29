import { parseJwt } from "../lib/app_lib.js";
import { copyToClipboard } from "../lib/app_lib.js";
import { loginViaAjax } from "../lib/app_lib.js";
"use strict";

var connection;
var locationConnection;
var token;
var scrollTo = "";
var dpt = ""

var textarea = document.getElementById('location_result_msg');

$(document).ready(function () {
    $("#btn_connect").click(function () {
        loginAndStartConnection();
    });

    $("#btn_copy_to_clipboard").click(function () {
        copyToClipboard($("#result_connectionId").val());
        $(this).html("Copied");
    });

    $("#btn_clear").click(function () {
        $("#result_msg").val("");
    });

    $("#link_reconnect").click(function () {
        connection.stop();
        connection = null;

        locationConnection.stop();
        locationConnection = null;

        toggleSpinnerAnimation(true);
        loginAndStartConnection();

        $("#location_result_msg").val("");
        $("#send_msg").val("");
    });

    $("#link_disconnect").click(function () {
        connection.stop();
        //connection = null;

        locationConnection.stop();
        //locationConnection = null;

        $("#location_result_msg").val("");
        $("#send_msg").val("");
    });

    //Allow for enter keypress to submit entries
    $('#ip_email, #ip_password').keypress(function (e) {
        if (e.keyCode === 13)
            $('#btn_connect').click();
    });

    $("#a_scroll_to_docs").click(function () {
        $("html, body").animate({
            // Remember to update the variable for different entities.
            scrollTop: $(`#${scrollTo}`).offset().top - 100
        }, 1000);
    });
});

function loginAndStartConnection() {
    var inputEmail = $("#ip_email").val();
    var inputPassword = $("#ip_password").val();

    loginViaAjax(inputEmail, inputPassword, null, false)
        .then(function (resultToken, resultRefreshToken) {
            token = resultToken;

            toggleSpinnerAnimation(true);
            startConnection(token);

            $("#login_result").attr("hidden", true);
        }).catch(function (err) {
            console.log(err);
            $("#login_result").removeAttr('hidden');
        });
}

function startConnection(token) {
    var role;
    try {
        role = parseJwt(token).role;
    } catch (ex) {
        toggleSpinnerAnimation(false);

        $("#login_result").html("Problem parsing the token");
        $("#login_result").removeAttr('hidden');
    }
    
    console.log(parseJwt(token));
    if (role != null) {
        $("#alert_container_role").removeClass("alert-success");
        $("#alert_container_role").addClass("alert-warning");
        $("#samp_role").html(role);

        if (role.indexOf("Agent") !== -1) {

            scrollTo = "personnel_docs";
            $("#location_chat_div").attr("hidden", true);
            $("#location_response_div").attr("hidden", false);

            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/agent", {
                    accessTokenFactory: () => token
                })
                .build();

            connection.on("AgentRequestsChannel", function (message) {
                var parsedMessage = JSON.parse(message);
                var outputMsg = "A request of Id: " + parsedMessage.requestId + " state was changed to " + parsedMessage.requestState +".";

                $("#result_msg").addClass('pb_color-primary');
                $("#result_msg").val(outputMsg);
            });

            connection.on("AgentRescuersChannel", function () {
                var outputMsg = "Rescuers in your department state has changed.";

                $("#result_msg").addClass('pb_color-primary');
                $("#result_msg").val(outputMsg);
            });

            locationConnection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/location", {
                    accessTokenFactory: () => token
                })
                .build();

            locationConnection.start().catch(function (err) {
                return console.error(err.toString());
            });

            locationConnection.on("LocationChannel", function (message) {
                appendLocationMsg(message)
            });

            locationConnection.on("InfoChannel", function (message) {
                appendLocationMsg(message)
            });

            locationConnection.on("AlertsChannel", function (message) {
                appendLocationMsg(message)
            });
        }
        else if (role.indexOf("Rescuer") !== -1) {

            scrollTo = "rescuer_docs";
            $("#location_chat_div").attr("hidden", false);
            $("#location_response_div").attr("hidden", false);

            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/rescuer", {
                    accessTokenFactory: () => token
                })
                .build();

            connection.on("RescuerChannel", function (message) {
                var outputMsg = "There is a change in the state of request: " + message + ".";

                $("#result_msg").addClass('pb_color-primary');
                $("#result_msg").val(outputMsg);
            });

            locationConnection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/location", {
                    accessTokenFactory: () => token
                })
                .build();

            locationConnection.start().catch(function (err) {
                return console.error(err.toString());
            });

            locationConnection.on("LocationChannel", function (email, lat, long) {
                appendLocationMsg(email + " | " + lat + ", " + long);
            });

            

            locationConnection.on("InfoChannel", function (message) {
                dpt = message;
                appendLocationMsg(message)
            });

            locationConnection.on("AlertsChannel", function (message) {
                appendLocationMsg(message)
            });

            $("#btn_send").click(function () {
                locationConnection.invoke("ShareLocation", dpt, $("#result_email").val(), 30.12345, 30.12345);
            });

        }

    } else {
        $("#samp_role").html("GeneralUser");
        
        scrollTo = "client_docs";
        $("#location_chat_div").attr("hidden", true);
        $("#location_response_div").attr("hidden", true);

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/client", {
                accessTokenFactory: () => token
            })
            .build();

        connection.on("ClientChannel", function (requestId, requestState) {
            console.log(requestId);
            console.log(requestState);

            var outputMsg = "The state of the request with Id " + requestId + " was changed to " + requestState + ".";

            var isAccepted = requestState == "Accepted";
            var isSolved = requestState == "Solved";
            if (isAccepted || isSolved) {
                $("#result_msg").removeClass('pb_color-primary');
                $("#result_msg").addClass('text-success');

                if (isSolved) {
                    $("#alert_container_client_solved").attr("hidden", false);
                }

            } else {
                $("#result_msg").removeClass('text-success');
                $("#result_msg").addClass('pb_color-primary');
            }

            $("#result_msg").val(outputMsg);
        });
    }

    
    connection.on("ConnectionInfoChannel", function (connectionId, clientEmail) {
        $("#result_email").val(clientEmail);
        $("#result_connectionId").val(connectionId);

        $("#hidden_div_till_connected").removeAttr('hidden');
        $("#btn_connect").removeClass('btn-primary');
        $("#btn_connect").removeClass("btn-danger");
        $("#btn_connect").removeClass('btn-shadow-milon');
        $("#btn_connect").addClass('btn-success');
        $("#btn_connect").addClass('disabled');
        $("#btn_connect_label").html('Connected');
        $("#ip_email").prop('disabled', true);
        $("#ip_password").prop('disabled', true);

        toggleSpinnerAnimation(false);

        $('html, body').animate({
            scrollTop: $("#start_of_form").offset().top - 20
        }, 500);
    });

    connection.start().catch(function (err) {
        $("#login_result").html("Error while trying to connect.");
        $("#login_result").removeAttr('hidden');

        toggleSpinnerAnimation(false);
        console.error("this was the value of the token " + token);
        return console.error(err.toString());
    });

    connection.onclose(function () {
        $("#result_email").val("");
        $("#result_connectionId").val("");
        $("#result_msg").val("");

        $("#location_result_msg").val("");
        $("#send_msg").val("");

        $("#hidden_div_till_connected").attr("hidden", true);
        $("#btn_connect").removeClass("btn-success");
        $("#btn_connect").addClass("btn-danger");
        $("#btn_connect").removeClass("disabled");
        $("#btn_connect_label").html("Reconnect");
        $("#ip_email").prop("disabled", false);
        $("#ip_password").prop("disabled", false);

        $("#btn_copy_to_clipboard").html("Copy");

        $("#alert_container_client_solved").attr("hidden", true);

        $('html, body').animate({
            scrollTop: 0
        }, 500);
    });
}

function appendLocationMsg(message) {
    var current = $("#location_result_msg").val();
    if (current.length != 0) {
        current += "\n";
    }
    var newMsg = current + new Date().toLocaleTimeString() + " | " + message;
    $("#location_result_msg").val(newMsg);

    textarea.scrollTop = textarea.scrollHeight;
}

function toggleSpinnerAnimation(startAnimation)
{
    if (startAnimation) {
        $("#btn_connect_label").attr("hidden", true);
        $("#btn_connect_animation").removeAttr("hidden");
    } else {
        $("#btn_connect_label").removeAttr("hidden");
        $("#btn_connect_animation").attr("hidden", true);
    }
}
