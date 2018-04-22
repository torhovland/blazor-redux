module NavigationPane

open canopy
open Expecto

let all() =

    context "Navigation panel tests"

    before (fun _ ->
        Routines.loginAnonymous "Tester-tester"
    )

    after (fun _ ->
        Routines.logout ()
    )

    "Join channel" &&& fun _ ->

        click <| Selectors.switchChannel "Demo"
        on "http://localhost:8083/#channel"

        // ensure there a title and input area
        "Demo" === read Selectors.channelTitle
        read Selectors.channelTopic |> contains "Channel for testing"

        displayed ".fs-message-input"

    "Leave channel channel" &&& fun _ ->

        click <| Selectors.switchChannel "Demo"
        on "http://localhost:8083/#channel"

        displayed Selectors.messageInputPanel
        displayed Selectors.channelLeaveBtn

        click Selectors.channelLeaveBtn
        on "http://localhost:8083/#about"

    "Create channel" &&& fun _ ->

        let height selector = (element selector).Size.Height

        sleep()
        0 === (height Selectors.newChannelInput)
        
        click Selectors.newChannelPlus
        sleep()

        Expect.isGreaterThan (height Selectors.newChannelInput) 30 "input is visible"

        // enter text
        Selectors.newChannelInput << "Harvest"
        click Selectors.newChannelInput
        press enter

        on "http://localhost:8083/#channel"
        "Harvest" === read Selectors.channelTitle

    "Select channel" &&& fun _ ->

        Routines.joinChannel "Demo"
        Routines.joinChannel "Test"

        // ensure there a title and input area
        sleep()
        (element Selectors.selectedChanBtn).Text |> contains "Test"
        
        Routines.switchChannel "Demo"

        sleep()
        (element Selectors.selectedChanBtn).Text |> contains "Demo"
