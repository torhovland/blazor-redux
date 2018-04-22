module Routines

open canopy

// Performs server login. Fails if user is already logged out
let loginAnonymous name =
    url "http://localhost:8083"
    onn "http://localhost:8083/logon"

    "#nickname" << name

    click Selectors.loginBtn
    on "http://localhost:8083/#"

    Selectors.userNick == name

let logout () =
    url "http://localhost:8083/logoff"

// Switches to existing channel, fails if no such channel exists
let switchChannel name =
    click <| Selectors.switchChannel name
    on "http://localhost:8083/#channel"
    (element Selectors.selectedChanBtn).Text |> contains name

// Ensures the channel exists (creates if it does not) and switches to this channel
let joinChannel name =
    click Selectors.newChannelPlus
    click Selectors.newChannelInput
    Selectors.newChannelInput << name
    press enter

    on "http://localhost:8083/#channel"
