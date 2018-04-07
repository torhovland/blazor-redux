namespace BlazorRedux

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Blazor.Components

type Dispatcher<'TAction> = delegate of 'TAction -> 'TAction
type Reducer<'TState, 'TAction> = delegate of 'TState * 'TAction -> 'TState
type AsyncActionsCreator<'TState, 'TAction> = delegate of Dispatcher<'TAction> * 'TState -> Task
type IAction = interface end

type Store<'TState, 'TAction>(reducer: Reducer<'TState, 'TAction>, initialState: 'TState) =
    let _reducer = reducer
    let mutable _state = initialState
    let _syncRoot = new obj();
    let onChangeEvent = new Event<unit>()
    
    [<CLIEvent>]
    member this.OnChange = onChangeEvent.Publish

    member this.State
        with get() = _state 
        and private set(value) = 
            _state <- value

    member this.Dispatch(action) =
        lock _syncRoot (fun () -> _state <- _reducer.Invoke(_state, action))
        onChangeEvent.Trigger()
        action;

    member this.DispatchAsync(actionsCreator: AsyncActionsCreator<'TState, 'TAction>) =
        let dispatcher = Dispatcher<'TAction>(this.Dispatch)
        actionsCreator.Invoke(dispatcher, _state)

type ReduxComponent<'TState, 'TAction>() =
    inherit BlazorComponent()

    [<Inject>]
    member val Store = Unchecked.defaultof<Store<'TState, 'TAction>> : Store<'TState, 'TAction> with get, set

    // Necessary due to FS0491
    member this.StateHasChanged() =
        base.StateHasChanged()

    member this.OnChangeHandler =
        Handler<unit>(fun _ _ -> this.StateHasChanged())

    override this.OnInit() =
        this.Store.add_OnChange this.OnChangeHandler
        base.OnInit()
    
    interface IDisposable with
        member this.Dispose() =
            this.Store.remove_OnChange this.OnChangeHandler

    member this.State = this.Store.State

    member this.Dispatch(action) =
        this.Store.Dispatch(action)

    member this.DispatchAsync(actionsCreator) =
        this.Store.DispatchAsync(actionsCreator)
