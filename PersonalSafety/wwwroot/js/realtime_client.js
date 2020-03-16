"use strict";

var connection;

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

$(document).ready(function () {
    $("#btn_connect").click(function () {
        var token = $("#ip_token").val();

        startConnection(token);
    });

    $("#link_logout").click(function () {
        connection.stop();
        location.reload();
    });
});