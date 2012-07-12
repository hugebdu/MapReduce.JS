jQuery.fn.log = function (msg)
{
    var elem = $(this[0]);
    elem.text($(this).text() + '\n' + msg);
};
var url = "ws://127.0.0.1:8080/websocket";

$("#connect").click(function ()
{
    if ("WebSocket" in window)
    {
        $("#log").log("Trying to connect to '" + url + "'...");

        var ws = new WebSocket(url);

        ws.onmessage = function (msg)
        {
            $("#log").log("Message received: " + msg.data);
        };

        ws.onopen = function ()
        {
            $("#log").log("Connection established");
        };

        ws.onerror = function ()
        {
            $("#log").log("Error connecting");
        };
    }
    else
    {
        alert("WebSockets not supported");
    }
})