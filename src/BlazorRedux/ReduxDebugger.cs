using System.Globalization;
using Microsoft.AspNetCore.Blazor;

namespace BlazorRedux
{
    public class ReduxDebugger<TModel, TAction> : ReduxComponent<TModel, TAction>
    {
        public RenderFragment Debugger;
        
        protected override void OnInit()
        {
            base.OnInit();

            // ReSharper disable once RedundantAssignment
            Debugger = builder =>
            {
                var seq = 0;

                foreach (var entry in Store.History)
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddContent(seq++, entry.Time.ToString(CultureInfo.InvariantCulture));
                    builder.CloseElement();

                    builder.OpenElement(seq++, "div");
                    builder.AddContent(seq++, entry.Action?.ToString() ?? "Initial state");
                    builder.CloseElement();
                }
            };
        }
    }
}
