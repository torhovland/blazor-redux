module Router

open Elmish.Browser.UrlParser

type Route =
    | Overview
    | Channel of string

let route : Parser<Route->Route,Route> =
    oneOf [
        map Overview (s "overview")
        map Channel (s "channel" </> str) ]

let toHash = function
    | Overview -> "#overview"
    | Channel str -> "#channel/" + str
