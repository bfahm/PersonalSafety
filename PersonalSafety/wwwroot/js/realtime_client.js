import { parseJwt } from "../lib/app_lib.js";
"use strict";

var connection;

$(document).ready(function () {
    $("#btn_connect").click(function () {
        var token = $("#ip_token").val();

        var role = parseJwt(token).role;
        console.log(parseJwt(token));
        if (role != null) {
            $("#samp_role").html(role);
        } else {
            $("#samp_role").html("GeneralUser");
        }
        
        startConnection(token);
    });

    $("#link_logout").click(function () {
        connection.stop();
        location.reload();
    });
});

function startConnection(token) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/main", {
            accessTokenFactory: () => token
        })
        .build();

    connection.on("ReceiveMessage", function (message) {
        var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        $("#result_msg").val(msg);
    });

    connection.on("ConnectionInfoChannel", function (message) {
        var parsedMessage = JSON.parse(message);
        var email = parsedMessage.UserEmail;
        var connectionId = parsedMessage.ConnectionId;

        $("#result_email").val(email.toString());
        $("#result_connectionId").val(connectionId.toString());

        $("#hidden_div_till_connected").removeAttr('hidden');
        $("#btn_connect").removeClass('btn-primary');
        $("#btn_connect").removeClass('btn-shadow-blue');
        $("#btn_connect").addClass('btn-success');
        $("#btn_connect").prop('disabled', true);
        $("#btn_connect").val('Connected');
        $("#ip_token").prop('disabled', true);
    });

    connection.start().then(function () {
        connection.invoke("GetMyConnectionInfo").catch(function (err) {
            return console.error(err.toString());
        });
    }).catch(function (err) {
        return console.error(err.toString());
    });
}