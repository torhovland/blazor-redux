Blazor.registerFunction('Logger.Log', function (message) {
    console.log(JSON.parse(message));
    return true;
});