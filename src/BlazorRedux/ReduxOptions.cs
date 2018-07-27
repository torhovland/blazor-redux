using System;
using Microsoft.AspNetCore.Blazor;
using Microsoft.JSInterop;

namespace BlazorRedux
{
    public class ReduxOptions<TState>
    {
        public ReduxOptions()
        {
            // Defaults
            StateSerializer = state => Json.Serialize(state);
            StateDeserializer = Json.Deserialize<TState>;
        }

        public Reducer<TState, NewLocationAction> LocationReducer { get; set; }
        public Func<TState, string> GetLocation { get; set; }
        public Func<TState, string> StateSerializer { get; set; }
        public Func<string, TState> StateDeserializer { get; set; }
    }
}