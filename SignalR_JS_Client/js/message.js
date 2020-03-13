"use strict";

var connection;

function startConnection(token) {

    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5566/hubs/sosparrot", {
            accessTokenFactory: () => token
        })
        .build();

    connection.on("ReceiveMessage", function (message) {
        var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        var div = document.createElement("div");
        div.innerHTML = msg + "<hr/>";
        document.getElementById("messages").appendChild(div);
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });

}

$(document).ready(function () {
    $("#btn_connect").click(function () {
        var token = $("#ip_token").val();
        startConnection(token);

        document.getElementById("sendButton").addEventListener("click", function (event) {
            var message = document.getElementById("message").value;
            var groupElement = document.getElementById("group");
            var groupValue = groupElement.options[groupElement.selectedIndex].value;

            var method = "";

            if (groupValue === "GetConnectionInfo") {
                var method = groupValue;
                connection.invoke(method).catch(function (err) {
                    return console.error(err.toString());
                });
            }

            event.preventDefault();
        });
    });
});