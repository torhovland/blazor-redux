using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorRedux
{
    public class ReduxDebugger : BlazorComponent
    {
        public RenderFragment Debugger;
        
        protected override void OnInit()
        {
            // ReSharper disable once RedundantAssignment
            Debugger = builder =>
            {
                var seq = 0;
                builder.OpenElement(seq++, "div");
                builder.AddContent(seq++, "Foo");
                builder.CloseElement();
            };
        }
    }
}
