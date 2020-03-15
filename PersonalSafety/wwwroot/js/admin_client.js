

var obj = JSON.parse('{"Accept-Language": "en-US,en;q=0.8", "Host": "headers.jsontest.com", "Accept-Charset": "ISO-8859-1,utf-8;q=0.7,*;q=0.3", "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"}');
var str = JSON.stringify(obj, null, 2); // spacing level = 2

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



$(document).ready(function () {
    console.log(syntaxHighlight(str))
    $("#json_response").html(syntaxHighlight(str));
    animateProgressBar("retrieve_connection_bar");

});