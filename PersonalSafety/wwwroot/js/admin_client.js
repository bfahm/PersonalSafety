import { parseJwt } from "../lib/app_lib.js";
import { syntaxHighlight } from "../lib/app_lib.js";
import { animateProgressBar } from "../lib/app_lib.js";
import { setCookie } from "../lib/app_lib.js";
import { getCookie } from "../lib/app_lib.js";
import { eraseCookie } from "../lib/app_lib.js";

"use strict";

var connection;
var token = '';


$(document).ready(function () {

    //Log use in directly if he has a valid token
    var currentToken = getCookie("token");
    var currentEmail = getCookie("email");
    if (currentToken != null) {
        token = currentToken;
        replaceElementsForLoggedInUser(token, currentEmail);
    }

    //Allow for enter keypress to submit entries
    $('#input_password, #input_email').keypress(function (e) {
        if (e.keyCode == 13)
            $('#btn_login').click();
    });

    //Login Button Action
    $("#btn_login").click(function () {
        var email = $("#input_email").val();
        var password = $("#input_password").val();

        loginViaAjax(email, password)
    });

    //Logout Button Action
    $("#link_logout").click(function () {
        eraseCookie("email");
        eraseCookie("token");
        location.reload();
    });
});

function loginViaAjax(email, password) {

    $.ajax({
        url: 'api/account/login',
        type: 'POST',
        dataType: 'json',
        data: JSON.stringify({
            "Email": email,
            "Password": password
        }),
        contentType: "application/json",
        success: function (result) {
            // Parse JWT to check if user roles are valid
            var parsedJwt = parseJwt(result.result);

            // Extract user data from token
            var role = parsedJwt.role;
            var email = parsedJwt.email;

            // If user is admin, let him through
            if (role == "Admin") {
                 //save token to global scope for later usages
                token = result.result;

                //save user login state to cookie
                setCookie("email", email, 2);
                setCookie("token", token, 2);

                //login animations and element swaps
                replaceElementsForLoggedInUser(token, email);

                setTimeout(function () {
                    $([document.documentElement, document.body]).animate({
                        scrollTop: $("#section-tools").offset().top
                    }, 300); //time for animation to execute
                }, 750); //time before animation starts execution

            } else {
                $("#login_result").html("Sorry, you have to be an Admin to access the content of this page.");
            }
        },
        error: function (result) {
            $("#login_result").html("Wrong username or password..");
            console.log('Error in Operation');
            console.log(result);
        }
    })
}

function loadConnectionsTable(parsedJson) {
    // Clear current table entries first
    $("#result_table > tbody > tr").remove();

    for (var i = 0; i < parsedJson.length; i++) {
        var obj = parsedJson[i];
        console.log(obj);

        var email = obj.UserEmail;
        var userId = obj.UserId;
        var connectionId = obj.ConnectionId;

        var currentIndex = '<th>' + (i + 1) + '</th>';
        var emailInRow = '<td>' + email + '</td>';
        var userIdInRow = '<td>' + userId + '</td>';
        var connectionIdInRow = '<td>' + connectionId + '</td>';
        var newRow = '<tr>' + currentIndex + emailInRow + userIdInRow + connectionIdInRow + '</tr>'

        $('#result_table > tbody:last-child').append(newRow);
    }
}

function loadConsoleContents(parsedJson) {
    // Clear current table entries first
    $("#console_container").empty();

    for (var i = 0; i < parsedJson.length; i++) {
        var obj = parsedJson[i];
        console.log(obj);

        $('#console_container').append('<p> > ' + obj + '</p>');
    }

    if (parsedJson.length == 0) {
        $('#console_container').append('<p> > </p>');
    }
}

function replaceElementsForLoggedInUser(token, email) {
    startConnection(token);
    $("#current_email").html(email);
    $("#login_form").attr("hidden", true);
    $("#logged_in_notifier").attr("hidden", false);
    $("#section-tools").attr("hidden", false);
}

// SignalR related code..
function startConnection(token) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/admin", {
            accessTokenFactory: () => token
        })
        .build();

    connection.on("AdminGetConnectionInfo", function (message) {
        animateProgressBar("retrieve_connection_bar");
        var parsedJson = JSON.parse(message);

        loadConnectionsTable(parsedJson);
        
        setTimeout(function () {
            $("#retrieve_result").attr("hidden", false);
        }, 1000); //time before animation starts execution
    });

    connection.on("AdminGetConsoleLines", function (message) {
        animateProgressBar("retrieve_console_bar");
        var parsedJson = JSON.parse(message);

        console.log(parsedJson);
        loadConsoleContents(parsedJson) 

        setTimeout(function () {
            $("#console_container").attr("hidden", false);
        }, 1000); //time before animation starts execution
    });

    // Start connection after finishing all the settings
    connection.start().then(function () {
        // Wait for connection to be established before trying to retreive data

        $("#btn_retrieve").click(function () {
            connection.invoke("GetConnectionInfo").catch(function (err) {
                return console.error(err.toString());
            });
            $(this).removeClass("btn-secondary");
            $(this).addClass("btn-primary");
            $("#retrieve_main_container").addClass("border-primary");
        });

        $("#btn_update").click(function () {
            connection.invoke("GetConsoleLines").catch(function (err) {
                return console.error(err.toString());
            });
            $(this).removeClass("btn-secondary");
            $(this).addClass("btn-primary");
            $("#console_main_container").addClass("border-primary");
        });

        $("#btn_clear").click(function () {
            connection.invoke("ClearConsoleLines").catch(function (err) {
                return console.error(err.toString());
            });
            connection.invoke("GetConsoleLines").catch(function (err) {
                return console.error(err.toString());
            });
        });


    }).catch(function (err) {
        return console.error(err.toString());
    });

    connection.onclose(function () {
        $('html, body').animate({
            scrollTop: 0
        }, 500);

        $("#alert_container_disconnected").removeAttr('hidden');
    });
}