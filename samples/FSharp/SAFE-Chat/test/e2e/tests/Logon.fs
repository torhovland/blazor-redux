module Logon

open canopy

let all() =

    context "Logon screen tests"

    before (fun _ ->
        url "http://localhost:8083"
        onn "http://localhost:8083/logon"
    )

    after (fun _ ->
        url "http://localhost:8083/logoff"
    )

    "Jump to logon screen" &&& fun _ ->
        url "http://localhost:8083/#about"
        on "http://localhost:8083/logon"

    "Regular user login" &&& fun _ ->
        Selectors.loginNickname << "Hacker"

        click Selectors.loginBtn
        on "http://localhost:8083/#"

        Selectors.userNick == "Hacker"
        Selectors.userStatus == ""

    "Reload page after login restores session" &&& fun _ ->
        Selectors.loginNickname << "fish"

        click Selectors.loginBtn
        on "http://localhost:8083/#"

        reload()
        on "http://localhost:8083/#"

    "Nick contains blank" &&& fun _ ->
        Selectors.loginNickname << "Kaidan Alenko"

        click Selectors.loginBtn
        on "http://localhost:8083/#"

        "Kaidan Alenko" === read Selectors.userNick

    "Nick contains non-ascii characters" &&& fun _ ->
        Selectors.loginNickname << "Иван Петров"

        click Selectors.loginBtn
        on "http://localhost:8083/#"

        "Иван Петров" === read Selectors.userNick


    "Logoff button is functioning" &&& fun _ ->
        Selectors.loginNickname << "Godzilla"

        click Selectors.loginBtn
        on "http://localhost:8083/#"

        click "#logout"
        on "http://localhost:8083/logon"
        

    // TODO does not accept the user with the same name