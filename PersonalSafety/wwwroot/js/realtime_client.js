import { parseJwt } from "../lib/app_lib.js";
import { copyToClipboard } from "../lib/app_lib.js";
"use strict";

var connection;
var token;

$(document).ready(function () {
    $("#btn_connect").click(function () {
        token = $("#ip_token").val();

        startConnection(token);
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
});

function startConnection(token) {
    var role = parseJwt(token).role;
    console.log(parseJwt(token));
    if (role != null) {
        $("#alert_container_role").removeClass("alert-success");
        $("#alert_container_role").addClass("alert-warning");
        $("#samp_role").html(role);

        if (role.indexOf("Agent") !== -1) {

            $("#a_scroll_to_docs").click(function () {
                $("html, body").animate({
                    scrollTop: $("#personnel_docs").offset().top - 100
                }, 1000);
            });

            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/agent", {
                    accessTokenFactory: () => token
                })
                .build();

            connection.on("AgentRequestsChannel", function (message) {
                var parsedMessage = JSON.parse(message);
                var outputMsg = "There is a change in the state of request: " + parsedMessage.requestId + ".";

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

            // TODO: Add docs for rescuer first then enable
            //$("#a_scroll_to_docs").click(function () {
            //    $("html, body").animate({
            //        scrollTop: $("#personnel_docs").offset().top - 100
            //    }, 1000);
            //});

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
        $("#a_scroll_to_docs").click(function () {
            $('html, body').animate({
                scrollTop: $("#client_docs").offset().top -100
            }, 1000);
        });

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
            } else {
                $("#result_msg").removeClass('text-success');
                $("#result_msg").addClass('pb_color-primary');
            }

            $("#result_msg").val(outputMsg);
        });
    }

    
    connection.on("ConnectionInfoChannel", function (message) {
        var parsedMessage = JSON.parse(message);
        var email = parsedMessage.UserEmail;
        var connectionId = parsedMessage.ConnectionId;

        $("#result_email").val(email.toString());
        $("#result_connectionId").val(connectionId.toString());

        $("#hidden_div_till_connected").removeAttr('hidden');
        $("#btn_connect").removeClass('btn-primary');
        $("#btn_connect").removeClass("btn-danger");
        $("#btn_connect").removeClass('btn-shadow-milon');
        $("#btn_connect").addClass('btn-success');
        $("#btn_connect").prop('disabled', true);
        $("#btn_connect").val('Connected');
        $("#ip_token").prop('disabled', true);

        $('html, body').animate({
            scrollTop: $("#start_of_form").offset().top - 20
        }, 500);
    });

    connection.start().then(function () {
        connection.invoke("GetMyConnectionInfo").catch(function (err) {
            return console.error(err.toString());
        });
    }).catch(function (err) {
        return console.error(err.toString());
    });

    connection.onclose(function () {
        $("#result_email").val("");
        $("#result_connectionId").val("");
        $("#result_msg").val("");

        $("#hidden_div_till_connected").attr("hidden", true);
        $("#btn_connect").removeClass("btn-success");
        $("#btn_connect").addClass("btn-danger");
        $("#btn_connect").prop("disabled", false);
        $("#btn_connect").val("Reconnect");
        $("#ip_token").prop("disabled", false);

        $("#btn_copy_to_clipboard").html("Copy");

        $('html, body').animate({
            scrollTop: 0
        }, 500);
    });
}