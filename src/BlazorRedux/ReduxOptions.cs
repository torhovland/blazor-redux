using System;
using Microsoft.AspNetCore.Blazor;

namespace BlazorRedux
{
    public class ReduxOptions<TState>
    {
        public ReduxOptions()
        {
            // Defaults
            StateSerializer = state => JsonUtil.Serialize(state);
            StateDeserializer = JsonUtil.Deserialize<TState>;
        }

        public Reducer<TState, LocationAction> LocationReducer { get; set; }
        public Func<TState, string> GetLocation { get; set; }
        public Func<TState, string> StateSerializer { get; set; }
        public Func<string, TState> StateDeserializer { get; set; }
    }
}