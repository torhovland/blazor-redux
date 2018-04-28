console.log('Opening socket ...');

var socket = new WebSocket("ws://localhost:8083/api/socket");
window.chatSocket = socket;
socket.onopen = function(evt) { onOpen(evt); };
socket.onclose = function(evt) { onClose(evt); };
socket.onmessage = function(evt) { onMessage(evt); };
socket.onerror = function(evt) { onError(evt); };

function onOpen(evt)
{
    console.log("CONNECTED");
    console.log(window.chatSocket);
    window.chatSocket.send('"Greets"');
    console.log(window.chatSocket);
    console.log("Greeting sent.");
}
function onClose(evt)
{
    console.log("DISCONNECTED");
}
function onMessage(evt)
{
    console.log('RESPONSE: ' + evt.data);
}
function onError(evt)
{
    console.log('ERROR ' + evt.data);
}
