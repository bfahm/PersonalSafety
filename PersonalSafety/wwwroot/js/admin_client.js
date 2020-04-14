import { parseJwt } from "../lib/app_lib.js";
import { animateProgressBar } from "../lib/app_lib.js";
import { setCookie } from "../lib/app_lib.js";
import { getCookie } from "../lib/app_lib.js";
import { eraseCookie } from "../lib/app_lib.js";

"use strict";

var connection;

$(document).ready(function () {

    //Log use in directly if he has a valid token
    var currentToken = getCookie("token");
    var currentRefreshToken = getCookie("refreshToken");
    var currentEmail = getCookie("email");

    if (currentToken != null) {
        isLoggedIn(currentToken, function(result) {
            if (result) {
                replaceElementsForLoggedInUser(currentToken, currentEmail);
            } else {
                refreshTokenViaAjax(currentToken, currentRefreshToken, function(newToken) {
                    replaceElementsForLoggedInUser(newToken, currentEmail);
                });
            }
        });
    }

    initializeButtons();
});

function initializeButtons() {
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
        logoutUser();
    });

    //Logout Button Action
    $("#a_refresh").click(function () {
        location.reload();
    });
}

// Logs user in, successful state updates the UI
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
            var jwtTokenResult = result.result.authenticationDetails.token;
            var refreshTokenResult = result.result.authenticationDetails.refreshToken;
            var parsedJwt = parseJwt(jwtTokenResult);

            // Extract user data from token
            var role = parsedJwt.role;
            var email = parsedJwt.email;

            // If user is admin, let him through
            if (role === "Admin") {
                 
                //save user login state to cookie
                setCookie("email", email, 4320);
                setCookie("token", jwtTokenResult, 4320);
                setCookie("refreshToken", refreshTokenResult, 4320);

                //login animations and element swaps
                replaceElementsForLoggedInUser(jwtTokenResult, email);

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
    });
}

function isLoggedIn(token, callback) {

    $.ajax({
        url: 'api/account/ValidateToken',
        type: 'GET',
        headers: {
            "Authorization": "Bearer " + token
        },
        success: function(result) {
            console.log(result);
            callback(true);
        },
        error: function(result) {
            console.error(result);
            callback(false);
        }
    });
}

function logoutUser() {
    eraseCookie("email");
    eraseCookie("token");
    eraseCookie("refreshToken");
    location.reload();
}

function refreshTokenViaAjax(token, refreshToken, callback) {

    $.ajax({
        url: 'api/account/refreshToken',
        type: 'POST',
        dataType: 'json',
        data: JSON.stringify({
            "token": token,
            "refreshToken": refreshToken
        }),
        contentType: "application/json",
        success: function(result) {
            // Parse JWT to check if user roles are valid
            var jwtTokenResult = result.result.token;
            var refreshTokenResult = result.result.refreshToken;

            //save user login state to cookie
            setCookie("token", jwtTokenResult, 4320);
            setCookie("refreshToken", refreshTokenResult, 4320);

            callback(jwtTokenResult);
        },
        error: function(result) {
            console.log(result);
            logoutUser();
        }
    });
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

        $("#btn_reset_trackers").click(function () {
            connection.invoke("ResetTrackers").catch(function (err) {
                return console.error(err.toString());
            });
            connection.invoke("GetConnectionInfo").catch(function (err) {
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