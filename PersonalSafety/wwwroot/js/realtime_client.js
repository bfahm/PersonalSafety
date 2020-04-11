import { parseJwt } from "../lib/app_lib.js";
import { copyToClipboard } from "../lib/app_lib.js";
"use strict";

var connection;
var token;
var scrollTo = "";

$(document).ready(function () {
    $("#btn_connect").click(function () {
        token = $("#ip_token").val();

        toggleSpinnerAnimation(true);

        setTimeout(function () {
            startConnection(token);
        }, 2000);

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
        startConnection(token);
    });

    $("#link_disconnect").click(function () {
        connection.stop();
        connection = null;
    });

    $("#a_scroll_to_docs").click(function () {
        $("html, body").animate({
            // Remember to update the variable for different entities.
            scrollTop: $(`#${scrollTo}`).offset().top - 100
        }, 1000);
    });
});

function startConnection(token) {
    var role;
    try {
        role = parseJwt(token).role;
    } catch (ex) {
        toggleSpinnerAnimation(false);

        $("#ip_token").val("");
        $("#ip_token").attr("placeholder", "Problem parsing the token");
    }
    
    console.log(parseJwt(token));
    if (role != null) {
        $("#alert_container_role").removeClass("alert-success");
        $("#alert_container_role").addClass("alert-warning");
        $("#samp_role").html(role);

        if (role.indexOf("Agent") !== -1) {

            scrollTo = "personnel_docs";

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

        }
        else if (role.indexOf("Rescuer") !== -1) {

            scrollTo = "rescuer_docs";

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


        }

    } else {
        $("#samp_role").html("GeneralUser");
        
        scrollTo = "client_docs";

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
        $("#ip_token").prop('disabled', true);

        toggleSpinnerAnimation(false);

        $("#ip_token").attr("placeholder", "Type your verified token here");

        $('html, body').animate({
            scrollTop: $("#start_of_form").offset().top - 20
        }, 500);
    });

    connection.start().catch(function (err) {
        $("#ip_token").val("");
        $("#ip_token").attr("placeholder", "Wrong or Expired Token");

        toggleSpinnerAnimation(false);
        return console.error(err.toString());
    });

    connection.onclose(function () {
        $("#result_email").val("");
        $("#result_connectionId").val("");
        $("#result_msg").val("");

        $("#hidden_div_till_connected").attr("hidden", true);
        $("#btn_connect").removeClass("btn-success");
        $("#btn_connect").addClass("btn-danger");
        $("#btn_connect").removeClass("disabled");
        $("#btn_connect_label").html("Reconnect");
        $("#ip_token").prop("disabled", false);

        $("#btn_copy_to_clipboard").html("Copy");

        $("#alert_container_client_solved").attr("hidden", true);

        $('html, body').animate({
            scrollTop: 0
        }, 500);
    });
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
