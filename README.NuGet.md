# Blazor Redux

This library offers [Redux](https://redux.js.org)-style state management for [Blazor](https://github.com/aspnet/Blazor), with support for both C# and F#. The implementation is based on [Redux.NET](https://github.com/GuillaumeSalles/redux.NET).

The combination of Blazor and Redux becomes an incredibly compelling platform for frontend development&mdash;more compelling than any other alternative, if you ask me:

- Blazor uses .NET, thus comes with a strong type-system built-in, like [Elm](http://elm-lang.org), [Purescript](http://www.purescript.org), [OCaml](https://bucklescript.github.io), and to some degree [Typescript](https://www.typescriptlang.org).
- With ASP.NET already such a viable option for the backend, this opens up for a very strong [isomorphic apps model](https://hackernoon.com/isomorphic-universal-boilerplate-react-redux-server-rendering-tutorial-example-webpack-compenent-6e22106ae285) with shared .NET code on the frontend and backend.
- The Razor view engine combines the power of a templating engine with the familiarity of HTML, like [JSX](https://reactjs.org/docs/introducing-jsx.html). With Redux, the Blazor pages themselves become very simple, with just presentational content, references to state in the store, and dispatching of actions.
- When it comes to productivity, Blazor can feel like one of the specialized proprietary platforms such as Silverlight and Flash, but it does in fact produce web standard-compliant code compatible with all major browsers and devices without any plugins.
- Blazor is actual .NET assemblies running on Mono compiled for WebAssembly. While this may sound excessive in terms of download size, both Mono and the .NET library are less than a megabyte in size.
- Blazor comes with strong backing from a solid organization (Microsoft).

## Advantages over vanilla Blazor

- Implements a one-way model-update-view architecture, by many considered to be [more robust and easier to reason about](https://www.exclamationlabs.com/blog/the-case-for-unidirectional-data-flow/) than a two-way data binding as found in Angular.
- Application state is kept in a single state store, facilitating advanced features such as [undo/redo](https://github.com/elm-community/undo-redo), [hydration of application state](https://github.com/rt2zz/redux-persist), and [time-traveling debuggers](http://debug.elm-lang.org).
- Any Blazor component upgraded to a Redux component will subscribe to changes in the state store and automatically update its view, so you don't have to worry about calling `StateHasChanged()`.
- Blazor Redux supports F#, which means you can take advantage of some advanced language features when designing your types, actions and reducer logic. The [discriminated union types](https://fsharpforfunandprofit.com/posts/discriminated-unions) are perfect for designing type-safe application messages, and [the `with` keyword in record types](https://fsharpforfunandprofit.com/posts/records/) makes it simple to work with immutable types in your reducer logic. Not to mention that a model with many small types can be created with much less ceremony. F# lends itself well to [type driven development](https://fsharpforfunandprofit.com/series/designing-with-types.html). However, the Blazor project itself and the Razor pages must be C#.

## More info

More documentation is available on [GitHub](https://github.com/torhovland/blazor-redux).

## Contributing

Blazor Redux is at an experimental stage, and you should expect breaking changes. But I'd be very interested in discussing the design and potential features. Please open an issue if you have any particular topic in mind.
