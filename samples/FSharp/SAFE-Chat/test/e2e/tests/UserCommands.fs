module UserCommands

open canopy

let all () =

    context "User console commands"

    let sendText message =
        click Selectors.messageInputPanel
        Selectors.messageInputText << message
        press enter

    before (fun _ ->
        url "http://localhost:8083"
        onn "http://localhost:8083/logon"

        "#nickname" << "Tester2"
        click Selectors.loginBtn
        on "http://localhost:8083/#"

        Selectors.userNick == "Tester2"
        Selectors.userStatus == ""

        // activate channel for command input bar to appear (not nice thing)
        click Selectors.newChannelPlus
        click Selectors.newChannelInput
        Selectors.newChannelInput << "Test"
        press enter

        on "http://localhost:8083/#channel"
    )

    after (fun _ ->
        url "http://localhost:8083/logoff"
    )

    "Change nick" &&& fun _ ->
        sendText "Hello all"
        sendText "/nick SuperTester"

        "SuperTester" === read Selectors.userNick

    "Change status" &&& fun _ ->
        sendText "/status The first Human Spectre"

        "The first Human Spectre" === read Selectors.userStatus

    "Change avatar" &&& fun _ ->
        sendText "/avatar http://pictures.org/1.png"

        let avaimg = element Selectors.userAvatar

        contains "http://pictures.org/1.png" (avaimg.GetCssValue "background-image")

    "Join channel" &&& fun _ ->
        sendText "/join Harvest"

        // check the chat jumps off the channel
        on "http://localhost:8083/#channel"
        "Harvest" === read Selectors.channelTitle

    "Leave channel" &&& fun _ ->
        "Test" === read Selectors.channelTitle

        sendText "/leave"

        // check the chat jumps off the channel
        on "http://localhost:8083/#about"
