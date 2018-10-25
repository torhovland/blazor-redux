using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;

namespace BlazorRedux
{
    public static class DevToolsInterop
    {
        private static readonly object SyncRoot = new object();
        private static bool _isReady;
        private static readonly Queue<Tuple<string, string>> Q = new Queue<Tuple<string, string>>();

        public static event EventHandler Reset;
        public static event StringEventHandler TimeTravel;

        private static void OnReset(EventArgs e)
        {
            var handler = Reset;
            handler?.Invoke(null, e);
        }

        private static void OnTimeTravel(StringEventArgs e)
        {
            var handler = TimeTravel;
            handler?.Invoke(null, e);
        }

        public static void DevToolsReady()
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

        public static void DevToolsReset()
        {
            OnReset(new EventArgs());
        }

        public static void TimeTravelFromJs(string state)
        {
            OnTimeTravel(new StringEventArgs(state));
        }

        public static void Log(string action, string state)
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

        static void LogToJs(string action, string state)
        {
            ((IJSInProcessRuntime)JSRuntime.Current).Invoke<bool>("Blazor.log", action, state);
        }
    }
}
