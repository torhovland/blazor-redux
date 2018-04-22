module InputArea

open canopy

let all () =
    context "Input area tests"

    before (fun _ ->
        Routines.loginAnonymous "InputAreaTester"
    )

    after (fun _ ->
        Routines.logout()
    )

    "Input area is visible in channel view" &&& fun _ ->

        notDisplayed Selectors.messageInputPanel

        Routines.switchChannel "Demo"

        displayed Selectors.messageInputPanel

    "Type and send text, input gets clean" &&& fun _ ->

        Routines.joinChannel "Test"

        Selectors.messageInputText << "Hello world"
        Selectors.messageInputText == "Hello world"

        click Selectors.messageInputPanel
        press enter
        sleep()
        
        Selectors.messageInputText == ""

    "Input message stored per channel" &&& fun _ ->

        Routines.joinChannel "Test"
        Selectors.messageInputText << "test channel"

        Routines.joinChannel "Demo"
        Selectors.messageInputText == ""
        Selectors.messageInputText << "hi demo"

        Routines.switchChannel "Test"
        Selectors.messageInputText == "test channel"

        Routines.switchChannel "Demo"
        Selectors.messageInputText == "hi demo"

    "Type and send text" &&& fun _ ->

        Routines.joinChannel "Test"

        Selectors.messageInputText << "Hello world"
        click Selectors.messageSendBtn

        read ".fs-chat .fs-messages div[class*='fs-message user'] div p" |> contains "Hello world"

        sleep()
        Selectors.messageInputText == ""