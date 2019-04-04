using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    public class DevToolsInterop
    {
        private readonly IServiceProvider _services;
        private readonly object SyncRoot = new object();
        private bool _isReady;
        private readonly Queue<Tuple<string, string>> Q = new Queue<Tuple<string, string>>();

        public event EventHandler Reset;
        public event StringEventHandler TimeTravel;

        public DevToolsInterop(IServiceProvider services)
        {
            _services = services;
        }

        private void OnReset(EventArgs e)
        {
            var handler = Reset;
            handler?.Invoke(null, e);
        }

        private void OnTimeTravel(StringEventArgs e)
        {
            var handler = TimeTravel;
            handler?.Invoke(null, e);
        }

        public void DevToolsReady()
        {
            lock (SyncRoot)
            {
                while (Q.Any())
                {
                    var entry = Q.Dequeue();
                    LogToJs(entry.Item1, entry.Item2);
                }
            }

            _isReady = true;
        }

        public void DevToolsReset()
        {
            OnReset(new EventArgs());
        }

        public void TimeTravelFromJs(string state)
        {
            OnTimeTravel(new StringEventArgs(state));
        }

        public void Log(string action, string state)
        {
            if (!_isReady)
            {
                lock (SyncRoot)
                {
                    Q.Enqueue(new Tuple<string, string>(action, state));
                }
            }
            else
            {
                LogToJs(action, state);
            }
        }

        void LogToJs(string action, string state)
        {
            _services.GetRequiredService<IJSRuntime>().InvokeAsync<bool>("Blazor.log", action, state).WaitAndUnwrapException();
        }
    }
}
