using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Services;

namespace BlazorRedux
{
    public class Store<TState, TAction> : IDisposable
    {
        private readonly Reducer<TState, TAction> _mainReducer;
        private readonly Reducer<TState, LocationAction> _locationReducer;
        private readonly Func<TState, string> _getLocation;
        private readonly TState _initialState;
        private IUriHelper _uriHelper = null;
        private string _currentLocation = null;
        private readonly object _syncRoot = new object();

        public TState State { get; private set; }
        public IList<HistoricEntry<TState, object>> History { get; }
        public event EventHandler Change;

        public Store(
            Reducer<TState, TAction> mainReducer, 
            Reducer<TState, LocationAction> locationReducer, 
            Func<TState, string> getLocation,
            TState initialState = default(TState))
        {
            _mainReducer = mainReducer;
            _locationReducer = locationReducer;
            _getLocation = getLocation;
            _initialState = initialState;
            State = initialState;

            DevToolsInterop.Reset += OnDevToolsReset;
            DevToolsInterop.TimeTravel += OnDevToolsTimeTravel;

            DevToolsInterop.Log("initial", JsonUtil.Serialize(State));

            History = new List<HistoricEntry<TState, object>>
            {
                new HistoricEntry<TState, object>(State)
            };
        }

        internal void Init(IUriHelper uriHelper)
        {
            if (_uriHelper != null || uriHelper == null) return;

            lock (_syncRoot)
            {
                _uriHelper = uriHelper;
                _uriHelper.OnLocationChanged += OnLocationChanged;
            }

            // TODO: Queue up any other actions, and let this apply to the initial state.
            Dispatch(new NewLocationAction { Location = _uriHelper.GetAbsoluteUri() });

            Console.WriteLine("Store initialized.");
        }

        public void Dispose()
        {
            if (_uriHelper != null)
                _uriHelper.OnLocationChanged -= OnLocationChanged;
            
            DevToolsInterop.Reset -= OnDevToolsReset;
            DevToolsInterop.TimeTravel -= OnDevToolsTimeTravel;
        }

        private void OnLocationChanged(object sender, string newAbsoluteUri)
        {
            if (newAbsoluteUri == _currentLocation) return;

            lock (_syncRoot)
            {
                _currentLocation = newAbsoluteUri;
            }

            Dispatch(new NewLocationAction { Location = newAbsoluteUri });
        }

        private void OnDevToolsReset(object sender, EventArgs e)
        {
            var state = _initialState;
            TimeTravel(state);
        }

        private void OnDevToolsTimeTravel(object sender, StringEventArgs e)
        {
            var state = JsonUtil.Deserialize<TState>(e.String);
            TimeTravel(state);
        }

        private void OnChange(EventArgs e)
        {
            var handler = Change;
            handler?.Invoke(this, e);

            var newLocation = _getLocation(State);
            if (newLocation == _currentLocation) return;

            lock (_syncRoot)
            {
                _currentLocation = newLocation;
            }

            _uriHelper.NavigateTo(newLocation);
        }

        public TAction Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                State = _mainReducer(State, action);
                DevToolsInterop.Log(action.ToString(), JsonUtil.Serialize(State));
                History.Add(new HistoricEntry<TState, object>(State, action));
            }

            OnChange(null);
            return action;
        }

        LocationAction Dispatch(LocationAction action)
        {
            lock (_syncRoot)
            {
                State = _locationReducer(State, action);
                DevToolsInterop.Log(action.ToString(), JsonUtil.Serialize(State));
                History.Add(new HistoricEntry<TState, object>(State, action));
            }

            OnChange(null);
            return action;
        }

        public void TimeTravel(TState state)
        {
            lock (_syncRoot)
            {
                State = state;
            }

            OnChange(null);
        }
    }
}