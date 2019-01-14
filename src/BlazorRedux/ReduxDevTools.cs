﻿using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace BlazorRedux
{
    public class ReduxDevTools : BlazorComponent
    {
        // ReSharper disable once RedundantAssignment
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var seq = 0;

            builder.OpenElement(seq++, "script");
            builder.AddContent(seq++,
@"(function () {
function timeTravel(state) {
    DotNet.invokeMethodAsync('BlazorRedux', 'TimeTravelFromJs', state);
}

window[""Blazor""].log = (action, state) => {
    var json = JSON.parse(state);

    if (action === 'initial')
        window.devTools.init(json);
    else
        window.devTools.send(action, json);

    return true;
};

var config = { name: 'Blazor Redux' }; 
var extension = window.__REDUX_DEVTOOLS_EXTENSION__;

if (!extension) {
    console.log('Redux DevTools not installed.');
    return;
}

var devTools = extension.connect(config);

if (!devTools) {
    console.log('Unable to connect to Redux DevTools.');
    return;
}

devTools.subscribe((message) => {
    if (message.type === 'START') {
        console.log('Connected with Redux DevTools.');
        DotNet.invokeMethodAsync('BlazorRedux', 'DevToolsReady');
    }
    else if (message.type === 'DISPATCH' && message.state) {
        // Time-traveling
        timeTravel(message.state);
    }
    else if (message.type === 'DISPATCH' && message.payload) {
        var payload = message.payload;

        if (payload.type === 'IMPORT_STATE') {
            // Hydration of state from a previous session
            var states = payload.nextLiftedState.computedStates;
            var index = payload.nextLiftedState.currentStateIndex;
            var state = states[index].state;
            timeTravel(JSON.stringify(state));
        }
        else if (payload.type === 'RESET') {
            // Reset state
            DotNet.invokeMethodAsync('BlazorRedux', 'DevToolsReset');
        }
        else {
            console.log('Unhandled payload from Redux DevTools:');
            console.log(payload);
        }
    }
    else {
        console.log('Unhandled message from Redux DevTools:');
        console.log(message);
    }
});

window.devTools = devTools;
}());");

            builder.CloseElement();
        }
    }
}
