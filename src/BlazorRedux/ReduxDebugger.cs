using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorRedux
{
    public class ReduxDebugger<TState, TAction> : ReduxComponent<TState, TAction>
    {
        private HistoricEntry<TState, object> _selectedEntry;

        public RenderFragment Debugger;
        
        protected override void OnInit()
        {
            base.OnInit();

            // ReSharper disable once RedundantAssignment
            Debugger = builder =>
            {
                var seq = 0;

                builder.OpenElement(seq++, "style");
                builder.AddContent(seq++, 
@".redux-debugger {
    display: flex;
    flex-wrap: wrap;
}

.redux-debugger__historic-entry {
    background-color: WhiteSmoke;
    padding: .5em;
}

.redux-debugger__historic-entry:hover { 
    background-color: Gainsboro; 
}

.redux-debugger__historic-entry--selected, 
.redux-debugger__historic-entry--selected:hover { 
    background-color: #4189C7; 
    color: White;
}

.redux-debugger__historic-entry__action {
    font-weight: bold;
}

.redux-debugger__action-details {
    padding: .5em;
}");
                builder.CloseElement();

                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "redux-debugger");

                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "redux-debugger__action-history");

                foreach (var entry in Store.History)
                {
                    builder.OpenElement(seq++, "div");
                    builder.AddAttribute(seq++, "class", "redux-debugger__historic-entry " + (entry == _selectedEntry ? "redux-debugger__historic-entry--selected" : ""));
                    builder.AddAttribute(seq++, "onclick", () => SelectEntry(entry));

                    builder.OpenElement(seq++, "div");
                    builder.AddAttribute(seq++, "class", "redux-debugger__historic-entry__action");
                    builder.AddContent(seq++, entry.Action?.ToString() ?? "Initial state");
                    builder.CloseElement();

                    builder.OpenElement(seq++, "div");
                    builder.AddAttribute(seq++, "class", "redux-debugger__historic-entry__time");
                    builder.AddContent(seq++, entry.Time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
                    builder.CloseElement();

                    builder.CloseElement(); // historic-entry
                }

                builder.CloseElement(); // action-history

                builder.OpenElement(seq++, "div");
                builder.AddAttribute(seq++, "class", "redux-debugger__action-details");
                seq = RenderActionDetails(builder, seq, _selectedEntry);
                builder.CloseElement(); // action-details

                builder.CloseElement(); // redux-debugger
            };
        }

        void SelectEntry(HistoricEntry<TState, object> entry)
        {
            _selectedEntry = entry;
            Store.TimeTravel(entry.State);
        }

        int RenderActionDetails(RenderTreeBuilder builder, int seq, HistoricEntry<TState, object> entry)
        {
            if (entry == null)
            {
                builder.AddContent(seq++, "No entry selected");
                return seq;
            }

            builder.OpenElement(seq++, "pre");
            builder.AddContent(seq++, entry.State.ToString());
            builder.CloseElement();

            return seq;
        }
    }
}
