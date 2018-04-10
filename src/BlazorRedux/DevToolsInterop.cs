using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Blazor.Browser.Interop;

namespace BlazorRedux
{
    public static class DevToolsInterop
    {
        private static readonly object SyncRoot = new object();
        private static bool _isReady;
        private static readonly Queue<Tuple<string, string>> Q = new Queue<Tuple<string, string>>();

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
            Console.WriteLine("DevTools ready.");
        }

        public static void TimeTravel(string state)
        {
            Console.WriteLine("Received state from JS:");
            Console.WriteLine(state);
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
            Console.WriteLine($"Submitting {action} to JS.");
            RegisteredFunction.Invoke<bool>("log", action, state);
        }
    }
}
