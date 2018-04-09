using System;
using Microsoft.AspNetCore.Blazor.Components;
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
            builder.AddContent(seq++, @"console.log('Connecting with Redux DevTools');");
            builder.CloseElement();
        }
    }
}
