using System;
using System.Collections.Generic;
using BlazorFSharpLib;
using BlazorRedux;
using Chat;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Types = App.Types;

namespace Blazor
{
    internal class Program
    {
        public static void Main()
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.AddReduxStore<Types.Model, Chat.Types.Msg>(
                    new Types.Model("", Chat.Types.ChatState.NotConnected), 
                    MyFuncs.MyReducer,
                    options =>
                    {
                        options.LocationReducer = MyFuncs.LocationReducer;
                        options.GetLocation = state => state.currentPage;
                        options.StateSerializer = MyFuncs.StateSerializer;
                        options.StateDeserializer = MyFuncs.StateDeserializer;
                    });
            });

            RegisteredFunction.Invoke<bool>("openSocket");

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}