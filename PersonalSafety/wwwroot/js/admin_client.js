"use strict";

var connection;
var token = '';
var json_result;

function startConnection(token) {

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/main", {
            accessTokenFactory: () => token
        })
        .build();


    connection.on("ConnectionInfoChannel", function (message) {
        var sytaxedJson = syntaxHighlight(message);
        animateProgressBar("retrieve_connection_bar");
        $("#json_response").html(sytaxedJson);
        setTimeout(function () {
            $("#json_response").attr("hidden", false);
        }, 2000); //time before animation starts execution
    });

    connection.start().then(function () {

        $("#btn_retrieve").click(function () {
            connection.invoke("GetConnectionInfo").catch(function (err) {
                return console.error(err.toString());
            });
        });

    }).catch(function (err) {
        return console.error(err.toString());
    });
}


$(document).ready(function () {

    var currentToken = getCookie("token");
    var currentEmail = getCookie("email");
    if (currentToken != null) {
        token = currentToken;
        logUserIn(token, currentEmail);
    }

    $("#btn_login").click(function () {
        var email = $("#input_email").val();
        var password = $("#input_password").val();

        loginViaAjax(email, password)
    });

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
            var parsedJwt = parseJwt(result.result);
            var role = parsedJwt.role;
            var email = parsedJwt.email;
            if (role == "Admin") {
                token = result.result;
                setCookie("token", result.result, 2);
                setCookie("email", email, 2);
                logUserIn(token, email);

                setTimeout(function () {
                    $([document.documentElement, document.body]).animate({
                        scrollTop: $("#section-tools").offset().top
                    }, 100); //time for animation to execute
                }, 500); //time before animation starts execution

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

function logUserIn(token, email) {
    startConnection(token);
    $("#current_email").html(email);
    $("#login_form").attr("hidden", true);
    $("#logged_in_notifier").attr("hidden", false);
    $("#section-tools").attr("hidden", false);
}

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
};

function syntaxHighlight(json) {
    if (typeof json != 'string') {
        json = JSON.stringify(json, undefined, 2);
    }
    json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
        var cls = 'number';
        if (/^"/.test(match)) {
            if (/:$/.test(match)) {
                cls = 'key';
            } else {
                cls = 'string';
            }
        } else if (/true|false/.test(match)) {
            cls = 'boolean';
        } else if (/null/.test(match)) {
            cls = 'null';
        }
        return '<span class="' + cls + '">' + match + '</span>';
    });
}

function animateProgressBar(barId) {
    var i = 0;
    function move() {
        if (i == 0) {
            i = 1;
            var elem = document.getElementById(barId);
            var width = 1;
            var id = setInterval(frame, 10);
            function frame() {
                if (width >= 100) {
                    clearInterval(id);
                    i = 0;
                } else {
                    width++;
                    elem.style.width = width + "%";
                }
            }
        }
    }
    move();
}


function setCookie(name, value, hours) {
    var expires = "";
    if (hours) {
        var date = new Date();
        date.setTime(date.getTime() + (hours * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
function eraseCookie(name) {
    document.cookie = name + '=; Max-Age=-99999999;';
}