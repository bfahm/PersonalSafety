/*General Functionality */
export function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(jsonPayload);
};

export function syntaxHighlight(json) {
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

export function animateProgressBar(barId) {
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

export function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function () {
        console.log('Async: Copying to clipboard was successful!');
    }, function (err) {
        console.error('Async: Could not copy text: ', err);
    });
}

export function checkForQuery(field) {
    var url = window.location.href;
    if (url.indexOf('?' + field + '=') != -1)
        return true;
    else if (url.indexOf('&' + field + '=') != -1)
        return true;
    return false
}

/*Cookie Related: */
export function setCookie(name, value, hours) {
    var expires = "";
    if (hours) {
        var date = new Date();
        date.setTime(date.getTime() + (hours * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
export function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
export function eraseCookie(name) {
    document.cookie = name + '=; Max-Age=-99999999;';
}

/*Account Related and Token management: */
// Logs user in and return token results in callback, also sets cookies
export function loginViaAjax(email, password, cookieDetails, requireAdmin) {
    return new Promise(function (resolve, reject) {
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

                // If user is admin, let him through
                if (role === "Admin" && requireAdmin === true) {

                    if (cookieDetails != null) {
                        //save user login state to cookie
                        setCookie(cookieDetails.cookieTokenKey, jwtTokenResult, 4320);
                        setCookie(cookieDetails.cookieRefreshTokenKey, refreshTokenResult, 4320);
                    }

                    resolve(jwtTokenResult, refreshTokenResult);
                } else if (requireAdmin === false) {
                    resolve(jwtTokenResult, refreshTokenResult);
                } else {
                    reject();
                }
            },
            error: function () {
                reject();
            }
        });
    });
}

export function simpleGet(url, jwt) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: url,
            type: 'GET',
            beforeSend: function (xhr) {   //Include the bearer token in header
                xhr.setRequestHeader("Authorization", 'Bearer ' + jwt);
            },
            success: function (result) {
                resolve(result);
            },
            error: function () {
                reject();
            }
        });
    });
}

export function simplePut(url, jwt) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: url,
            type: 'PUT',
            beforeSend: function (xhr) {   //Include the bearer token in header
                xhr.setRequestHeader("Authorization", 'Bearer ' + jwt);
            },
            success: function (result) {
                resolve(result);
            },
            error: function () {
                reject();
            }
        });
    });
}

// Erases cookies and refreshes the page
export function logoutUser(cookieDetails) {
    eraseCookie(cookieDetails.cookieTokenKey);
    eraseCookie(cookieDetails.cookieRefreshTokenKey);
    location.reload();
}

// Requests a new token and refresh token pair, and returns them in callback, also sets cookies
export function refreshTokenViaAjax(token, refreshToken, cookieDetails) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: 'api/account/refreshToken',
            type: 'POST',
            dataType: 'json',
            data: JSON.stringify({
                "token": token,
                "refreshToken": refreshToken
            }),
            contentType: "application/json",
            success: function (result) {
                // Parse JWT to check if user roles are valid
                var jwtTokenResult = result.result.token;
                var refreshTokenResult = result.result.refreshToken;

                if (cookieDetails) {
                    //save user login state to cookie
                    setCookie(cookieDetails.cookieTokenKey, jwtTokenResult, 4320);
                    setCookie(cookieDetails.cookieRefreshTokenKey, refreshTokenResult, 4320);
                }

                resolve(jwtTokenResult, refreshTokenResult);
            },
            error: function () {
                reject();
            }
        });
    });
}

export function tokenFactory(cookieDetails) {
    //Log use in directly if he has a valid token
    var currentToken = getCookie(cookieDetails.cookieTokenKey);
    var currentRefreshToken = getCookie(cookieDetails.cookieRefreshTokenKey);

    return new Promise(function (resolve, reject) {
        if (currentToken != null && currentRefreshToken != null) {
            var parsedToken = parseJwt(currentToken);
            var tokenExpiry = parsedToken.exp;

            if (Date.now() >= tokenExpiry * 1000) {
                refreshTokenViaAjax(currentToken, currentRefreshToken, cookieDetails)
                    .then(function (token, refreshToken) {
                        resolve(token);
                    }).catch(function () {
                        reject(true);
                    });
            } else {
                resolve(currentToken);
            }
        } else {
            reject(false);
        }
    });
}