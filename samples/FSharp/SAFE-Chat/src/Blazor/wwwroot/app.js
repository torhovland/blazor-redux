Blazor.registerFunction('openSocket', () => {
    console.log('Opening socket ...');

    var socket = new WebSocket("ws://localhost:8083/api/socket");
    socket.onopen = function(evt) { onOpen(evt); };
    socket.onclose = function(evt) { onClose(evt); };
    socket.onmessage = function(evt) { onMessage(evt); };
    socket.onerror = function(evt) { onError(evt); };

    var messageFromJs =
        Blazor.platform.findMethod('BlazorFSharpLib', 'BlazorFSharpLib', 'SocketInterop', 'MessageFromJs');

    function onOpen(evt) {
        console.log("CONNECTED");
        socket.send('"Greets"');
        console.log("Greeting sent.");
    }

    function onClose(evt) {
        console.log("DISCONNECTED");
    }

    function onMessage(evt) {
        console.log('RESPONSE: ' + evt.data);
        var s = JSON.stringify(evt.data);
        Blazor.platform.callMethod(messageFromJs, null, [Blazor.platform.toDotNetString(s)]);
    }

    function onError(evt) {
        console.log('ERROR ' + evt.data);
    }

    return true;
});
