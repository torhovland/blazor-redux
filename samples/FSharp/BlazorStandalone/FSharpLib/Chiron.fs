module Chiron

open System
open System.Globalization
open System.Text
open Aether
open FParsec

(* RFC 7159

   Types, parsers, formatters and other utilities implemented to mirror the
   specification of JSON (JavaScript Object Notation Data Interchange Format)
   as defined in RFC 7159.

   Taken from [http://tools.ietf.org/html/rfc7159] *)

(* Types

   Simple AST for JSON, with included isomorphisms and lenses in Aether form for
   lens/isomorphism based modification of complex JSON structures. *)

type Json =
    | Array of Json list
    | Bool of bool
    | Null of unit
    | Number of decimal
    | Object of Map<string, Json>
    | String of string

    (* Epimorphisms *)

    static member internal Array__ =
        (function | Array x -> Some x
                  | _ -> None), Array

    static member internal Bool__ =
        (function | Bool x -> Some x
                  | _ -> None), Bool

    static member internal Null__ =
        (function | Null () -> Some ()
                  | _ -> None), Null

    static member internal Number__ =
        (function | Number x -> Some x
                  | _ -> None), Number

    static member internal Object__ =
        (function | Object x -> Some x
                  | _ -> None), Object

    static member internal String__ =
        (function | String x -> Some x
                  | _ -> None), String

    (* Prisms *)

    static member Array_ =
        Prism.ofEpimorphism Json.Array__

    static member Bool_ =
        Prism.ofEpimorphism Json.Bool__

    static member Null_ =
        Prism.ofEpimorphism Json.Null__

    static member Number_ =
        Prism.ofEpimorphism Json.Number__

    static member Object_ =
        Prism.ofEpimorphism Json.Object__

    static member String_ =
        Prism.ofEpimorphism Json.String__

(* Functional

   Functional signatures for working with Json types, implying a monadic
   approach to working with Json where appropriate.

   Additionally includes common functions for combining and creating
   functions of type Json<'a> which may be used via operator based
   combinators or a computation expression (both provided later). *)

[<AutoOpen>]
module Functional =

    type Json<'a> =
        Json -> JsonResult<'a> * Json

    and JsonResult<'a> =
        | Value of 'a
        | Error of string

    (* Functions

       Common functions for combining Json<'a> functions in to new
       forms, and for creating new Json<'a> functions given suitable
       initial data. *)

    [<RequireQualifiedAccess>]
    module Json =

        let inline init (a: 'a) : Json<'a> = 
            fun json ->
                Value a, json

        let inline error (e: string) : Json<'a> =
            fun json ->
                Error e, json

        let inline internal ofResult result =
            fun json ->
                result, json

        let inline bind (m: Json<'a>) (f: 'a -> Json<'b>) : Json<'b> =
            fun json ->
                match m json with
                | Value a, json -> (f a) json
                | Error e, json -> Error e, json

        let inline apply (f: Json<'a -> 'b>) (m: Json<'a>) : Json<'b> =
            bind f (fun f' ->
                bind m (fun m' ->
                    init (f' m')))

        let inline map (f: 'a -> 'b) (m: Json<'a>) : Json<'b> =
            bind m (fun m' ->
                init (f m'))

        let inline map2 (f: 'a -> 'b -> 'c) (m1: Json<'a>) (m2: Json<'b>) : Json<'c> =
            apply (apply (init f) m1) m2

(* Operators

   Symbolic operators for working with Json<'a> functions, providing
   an operator based concise alternative to the primitive Json<'a> combinators
   given as part of Functional.
   
   This module is not opened by default, as symbolic operators are a matter
   of taste and may also clash with other operators from other libraries. *)

module Operators =

    let inline (>>=) m f =
        Json.bind m f

    let inline (=<<) f m =
        Json.bind m f

    let inline (<*>) f m =
        Json.apply f m

    let inline (<!>) f m =
        Json.map f m

    let inline (>>.) m f =
        Json.bind m (fun _ -> f)

    let inline (.>>) m f =
        Json.bind (fun _ -> m) f

    let inline ( *>) m1 m2 =
        Json.map2 (fun _ x -> x) m1 m2

    let inline ( <*) m1 m2 =
        Json.map2 (fun x _ -> x) m1 m2

    let inline (>=>) m1 m2 =
        Json.bind (fun x -> m1 x) m2

    let inline (<=<) m1 m2 =
        Json.bind (fun x -> m2 x) m1

(* Builder

   Computation expression (builder) for working with JSON structures in a
   simple way, including lensing, morphisms, etc. using the Aether
   library. *)

[<AutoOpen>]
module Builder =

    type JsonBuilder () =

        member __.Bind (m1, m2) : Json<_> =
            Json.bind m1 m2

        member __.Combine (m1, m2) : Json<_> =
            Json.bind m1 (fun () -> m2)

        member __.Delay (f) : Json<_> =
            Json.bind (Json.init ()) f

        member __.Return (x) : Json<_> =
            Json.init x

        member __.ReturnFrom (f) : Json<_> =
            f

        member __.Zero () : Json<_> =
            Json.init ()

    let json =
        JsonBuilder ()

(* Optics

   Functional optics based access to nested Json data structures,
   using Aether format lenses/prisms/etc. Uses Json<'a> based functions, so
   can be used monadically. *)

[<AutoOpen>]
module Optics =

    (* Functions *)

    [<RequireQualifiedAccess>]
    module Json =

        [<RequireQualifiedAccess>]
        module Optic =

            type Get =
                | Get with

                static member (^.) (Get, l: Lens<Json,'b>) : Json<_> =
                    fun json ->
                        Value (Optic.get l json), json

                static member (^.) (Get, p: Prism<Json,'b>) : Json<_> =
                    fun json ->
                        match Optic.get p json with
                        | Some x -> Value x, json
                        | _ -> Error (sprintf "Couldn't use Prism %A on JSON: '%A'" p json), json

            let inline get o : Json<_> =
                (Get ^. o)

            type TryGet =
                | TryGet with

                static member (^.) (TryGet, l: Lens<Json,'b>) : Json<_> =
                    fun json ->
                        Value (Some (Optic.get l json)), json

                static member (^.) (TryGet, p: Prism<Json,'b>) : Json<_> =
                    fun json ->
                        Value (Optic.get p json), json

            let inline tryGet o : Json<_> =
                (TryGet ^. o)

            let inline set o v : Json<_> =
                fun json ->
                    Value (), Optic.set o v json

            let inline map o f : Json<_> =
                fun json ->
                    Value (), Optic.map o f json

        (* Obsolete

           Backwards compatibility shims to make the 5.x-> 6.x transition
           less painful, providing functionally equivalent options where possible.

           To be removed for 7.x releases. *)

        [<RequireQualifiedAccess>]
        module Lens =

            [<Obsolete ("Use Json.Optic.get instead.")>]
            let inline get l =
                Optic.get l

            [<Obsolete ("Use Json.Optic.set instead.")>]
            let inline set l =
                Optic.get l

            [<Obsolete ("Use Json.Optic.map instead.")>]
            let inline map l =
                Optic.map l

        [<RequireQualifiedAccess>]
        module Prism =

            [<Obsolete ("Use Json.Optic.get instead.")>]
            let inline get l =
                Optic.get l

            [<Obsolete ("Use Json.Optic.tryGet instead.")>]
            let inline tryGet p =
                Optic.tryGet p

            [<Obsolete ("Use Json.Optic.set instead.")>]
            let inline set l =
                Optic.get l

            [<Obsolete ("Use Json.Optic.map instead.")>]
            let inline map l =
                Optic.map l

        [<Obsolete ("Use Json.Optic.get instead.")>]
        let inline getLens l =
            Optic.get l

        [<Obsolete ("Use Json.Optic.get instead.")>]
        let inline getLensPartial l =
            Optic.get l

        [<Obsolete ("Use Json.Optic.tryGet instead.")>]
        let inline tryGetLensPartial l =
            Optic.tryGet l

        [<Obsolete ("Use Json.Optic.set instead.")>]
        let inline setLens l =
            Optic.set l

        [<Obsolete ("Use Json.Optic.set instead.")>]
        let inline setLensPartial l =
            Optic.set l

        [<Obsolete ("Use Json.Optic.map instead.")>]
        let inline mapLens l =
            Optic.map l

        [<Obsolete ("Use Json.Optic.map instead.")>]
        let inline mapLensPartial l =
            Optic.map l

(* Escaping

   Functions for escaped string parsing and formatting, as a
   minimal encoding function (escaping only disallowed codepoints,
   but normalizing any input). *)

[<RequireQualifiedAccess>]
module internal Escaping =

    let private digit i =
            (i >= 0x30 && i <= 0x39)

    let private hexdig i =
            (digit i)
         || (i >= 0x41 && i <= 0x46)
         || (i >= 0x61 && i <= 0x66)

    let private unescaped i =
            i >= 0x20 && i <= 0x21
         || i >= 0x23 && i <= 0x5b
         || i >= 0x5d && i <= 0x10ffff

    let private unescapedP =
        satisfy (int >> unescaped)

    let private hexdig4P =
        manyMinMaxSatisfy 4 4 (int >> hexdig)
        |>> fun s ->
            char (Int32.Parse (s, NumberStyles.HexNumber))

    let private escapedP =
            skipChar '\\'
        >>. choice [
                pchar '"'
                pchar '\\'
                pchar '/'
                skipChar 'b' >>% '\u0008'
                skipChar 'f' >>% '\u000c'
                skipChar 'n' >>% '\u000a'
                skipChar 'r' >>% '\u000d'
                skipChar 't' >>% '\u0009'
                skipChar 'u' >>. hexdig4P ]

    let private charP =
        choice [
            unescapedP
            escapedP ]

    let parse =
        many charP

    let private escapeChars =
        [| '"'; '\\'; '\n'; '\r'; '\t'; '\b'; '\f'
           '\u0000'; '\u0001'; '\u0002'; '\u0003'
           '\u0004'; '\u0005'; '\u0006'; '\u0007'
           '\u000B'; '\u000E'; '\u000F'
           '\u0010'; '\u0011'; '\u0012'; '\u0013'
           '\u0014'; '\u0015'; '\u0016'; '\u0017'
           '\u0018'; '\u0019'; '\u001A'; '\u001B'
           '\u001C'; '\u001D'; '\u001E'; '\u001F' |]

    let escape (s: string) : string =
        let mutable nextEscapeIndex = s.IndexOfAny (escapeChars)
        if nextEscapeIndex = -1 then s else
        let sb = System.Text.StringBuilder (String.length s)
        let mutable lastIndex = 0
        while (nextEscapeIndex <> -1) do
            if nextEscapeIndex > lastIndex then
                sb.Append (s, lastIndex, nextEscapeIndex - lastIndex) |> ignore
            match s.[nextEscapeIndex] with
            | '"' -> sb.Append @"\"""
            | '\\' -> sb.Append @"\\"
            | '\n' -> sb.Append @"\n"
            | '\r' -> sb.Append @"\r"
            | '\t' -> sb.Append @"\t"
            | '\f' -> sb.Append @"\f"
            | '\b' -> sb.Append @"\b"
            | '\u0000' -> sb.Append @"\u0000"
            | '\u0001' -> sb.Append @"\u0001"
            | '\u0002' -> sb.Append @"\u0002"
            | '\u0003' -> sb.Append @"\u0003"
            | '\u0004' -> sb.Append @"\u0004"
            | '\u0005' -> sb.Append @"\u0005"
            | '\u0006' -> sb.Append @"\u0006"
            | '\u0007' -> sb.Append @"\u0007"
            | '\u000B' -> sb.Append @"\u000B"
            | '\u000E' -> sb.Append @"\u000E"
            | '\u000F' -> sb.Append @"\u000F"
            | '\u0010' -> sb.Append @"\u0010"
            | '\u0011' -> sb.Append @"\u0011"
            | '\u0012' -> sb.Append @"\u0012"
            | '\u0013' -> sb.Append @"\u0013"
            | '\u0014' -> sb.Append @"\u0014"
            | '\u0015' -> sb.Append @"\u0015"
            | '\u0016' -> sb.Append @"\u0016"
            | '\u0017' -> sb.Append @"\u0017"
            | '\u0018' -> sb.Append @"\u0018"
            | '\u0019' -> sb.Append @"\u0019"
            | '\u001A' -> sb.Append @"\u001A"
            | '\u001B' -> sb.Append @"\u001B"
            | '\u001C' -> sb.Append @"\u001C"
            | '\u001D' -> sb.Append @"\u001D"
            | '\u001E' -> sb.Append @"\u001E"
            | '\u001F' -> sb.Append @"\u001F"
            | c -> sb.Append(@"\u").Append((int c).ToString("X4"))
            |> ignore
            lastIndex <- nextEscapeIndex + 1
            nextEscapeIndex <- s.IndexOfAny (escapeChars, lastIndex)
        if lastIndex < String.length s then
            sb.Append (s, lastIndex, String.length s - lastIndex) |> ignore
        sb.ToString ()

(* Parsing

   Functions for parsing string JSON data to Json types, using
   FParsec.

   Functions parse and tryParse are effectively static,
   while import parses the provided string JSON and replaces the
   current state of a Json<'a> function. *)

[<AutoOpen>]
module Parsing =

    (* Helpers

       Utlility functions for working with intermediate states of
       parsers, minimizing boilerplate and unpleasant code. *)

    let private emp =
        function | Some x -> x
                 | _ -> ""

    (* Grammar

       Common grammatical elements forming parts of other parsers as
       as defined in RFC 1759. The elements are implemented slightly
       differently due to the design of parser combinators used, chiefly
       concerning whitespace, which is always implemented as trailing.

       Taken from RFC 7159, Section 2 Grammar
       See [http://tools.ietf.org/html/rfc7159#section-2] *)

    let private wsp i =
            i = 0x20
         || i = 0x09
         || i = 0x0a
         || i = 0x0d

    let private wspP =
        skipManySatisfy (int >> wsp)

    let private charWspP c =
        skipChar c .>> wspP

    let private beginArrayP =
        charWspP '['

    let private beginObjectP =
        charWspP '{'

    let private endArrayP =
        charWspP ']'

    let private endObjectP =
        charWspP '}'

    let private nameSeparatorP =
        charWspP ':'

    let private valueSeparatorP =
        charWspP ','

    (* JSON

       As the JSON grammar is recursive in various forms, we create a
       reference parser which will be assigned later, allowing for recursive
       definition of parsing rules. *)

    let private jsonP, jsonR =
        createParserForwardedToRef ()

    (* Values

       Taken from RFC 7159, Section 3 Values
       See [http://tools.ietf.org/html/rfc7159#section-3] *)

    let private boolP =
            stringReturn "true" true
        <|> stringReturn "false" false
        .>> wspP

    let private nullP =
        stringReturn "null" () .>> wspP

    (* Numbers

       The numbers parser is implemented by parsing the JSON number value
       in to a known representation valid as string under Double.Parse
       natively (invoked as the float conversion function on the eventual
       string).

       Taken from RFC 7159, Section 6 Numbers
       See [http://tools.ietf.org/html/rfc7159#section-6] *)

    let private digit1to9 i =
            i >= 0x31 && i <= 0x39

    let private digit i =
            digit1to9 i
         || i = 0x30

    let private e i =
            i = 0x45 
         || i = 0x65

    let private minusP =
        charReturn '-' "-"

    let private intP =
        charReturn '0' "0" <|> (satisfy (int >> digit1to9) .>>. manySatisfy (int >> digit)
        |>> fun (h, t) -> string h + t)

    let private fracP =
        skipChar '.' >>.  many1Satisfy (int >> digit)
        |>> fun i -> "." + i

    let private expP =
            skipSatisfy (int >> e)
        >>. opt (charReturn '-' "-" <|> charReturn '+' "+")
        .>>. many1Satisfy (int >> digit)
        |>> function | Some s, d -> "e" + s + d
                     | _, d -> "e" + d

    let private numberP =
        pipe4 (opt minusP) intP (opt fracP) (opt expP) (fun m i f e ->
            decimal (emp m + i + emp f + emp e)) .>> wspP

    (* Strings

       Taken from RFC 7159, Section 7 Strings
       See [http://tools.ietf.org/html/rfc7159#section-7] *)

    let private quotationMarkP =
        skipChar '"'

    let private stringP =
        between quotationMarkP quotationMarkP Escaping.parse .>> wspP
        |>> fun cs -> new string (List.toArray cs)

    (* Objects

       Taken from RFC 7159, Section 4 Objects
       See [http://tools.ietf.org/html/rfc7159#section-4] *)

    let private memberP =
        stringP .>> nameSeparatorP .>>. jsonP

    let private objectP =
        between beginObjectP endObjectP (sepBy memberP valueSeparatorP)
        |>> Map.ofList

    (* Arrays

       Taken from RFC 7159, Section 5 Arrays
       See [http://tools.ietf.org/html/rfc7159#section-5] *)

    let private arrayP =
        between beginArrayP endArrayP (sepBy jsonP valueSeparatorP)

    (* JSON *)

    do jsonR :=
            wspP
        >>. choice [
                arrayP  |>> Array
                boolP   |>> Bool
                nullP   |>> Null
                numberP |>> Number
                objectP |>> Object
                stringP |>> String ]

    (* Functions

       Functions for parsing (or attempting to parse) JSON data as strings,
       returning data as Json types when successful (or as a wrapped Json type
       in the case of attempt-based parsing). *)

    [<RequireQualifiedAccess>]
    module Json =

        let internal parseJson s =
            if String.IsNullOrWhiteSpace s then
                Error "Input is null or whitespace"
            else
                match run jsonP s with
                | Success (json, _, _) -> Value json
                | Failure (e, _, _) -> Error e

        let tryParse =
               parseJson
            >> function | Value json -> Choice1Of2 json
                        | Error e -> Choice2Of2 e

        let parse =
               parseJson
            >> function | Value json -> json
                        | Error e -> failwith e

        let import s =
            fun json ->
                match parseJson s with
                | Value json -> Value (), json
                | Error e -> Error e, json

(* Formatting *)

[<AutoOpen>]
module Formatting =

    (* Helpers *)

    type private Formatter<'a> =
        'a -> StringBuilder -> StringBuilder

    type private Separator =
        StringBuilder -> StringBuilder

    let private append (s: string) (b: StringBuilder) =
        b.Append s

    let private appendf (s: string) (v1: obj) (b: StringBuilder) =
        b.AppendFormat (s, v1)

    let private join<'a> (f: Formatter<'a>) (s: Separator) =
        let rec join values (b: StringBuilder) =
            match values with
            | [] -> b
            | [v] -> f v b
            | v :: vs -> (f v >> s >> join vs) b

        join

    (* Options

       Options for formatting, defined as functions for spacing and newline
       formatting appenders. Predefined formats are given as static members
       as a shorthand. *)

    type JsonFormattingOptions =
      { Spacing : StringBuilder -> StringBuilder
        NewLine : int -> StringBuilder -> StringBuilder }

      static member Compact =
        { Spacing = id
          NewLine = fun _ x -> x }

      static member SingleLine =
        { Spacing = append " "
          NewLine = fun _ -> append " " }

      static member Pretty =
        { Spacing = append " "
          NewLine = fun level -> append "\n" >> append (String.replicate level "  ") }

    (* Formatters *)

    let rec private formatJson level options =
        function | Array x -> formatArray level options x
                 | Bool x -> formatBool x
                 | Number x -> formatNumber x
                 | Null _ -> formatNull ()
                 | Object x -> formatObject level options x
                 | String x -> formatString x

    and private formatArray level options =
        function | x ->
                       append "["
                    >> options.NewLine (level + 1)
                    >> join (formatJson (level + 1) options) (append "," >> options.NewLine (level + 1)) x
                    >> options.NewLine level
                    >> append "]"

    and private formatBool =
        function | true -> append "true"
                 | _ -> append "false"

    and private formatNumber =
        function | x -> append (string x)

    and private formatNull =
        function | () -> append "null"

    and private formatObject level options =
        function | x -> 
                       append "{" 
                    >> options.NewLine (level + 1)
                    >> join (fun (k, v) -> appendf "\"{0}\":" (Escaping.escape k) >> options.Spacing >> formatJson (level + 1) options v)
                            (append "," >> options.NewLine (level + 1))
                            (Map.toList x)
                    >> options.NewLine level
                    >> append "}"

    and private formatString =
        function | x -> appendf "\"{0}\"" (Escaping.escape x)

    (* Functions *)

    [<RequireQualifiedAccess>]
    module Json =

        let format json =
            StringBuilder ()
            |> formatJson 0 JsonFormattingOptions.Compact json
            |> string

        let formatWith options json =
            StringBuilder ()
            |> formatJson 0 options json
            |> string

    (* Error Message Formatters *)

    [<RequireQualifiedAccess>]
    module Errors =

        let missingMember key =
            sprintf "Error deserializing JSON object; Missing required member '%s'" key

        let missingMemberWithJson key =
            function | Some format -> Json.formatWith format >> (+) (missingMember key + ": ")
                     | None -> fun _ -> missingMember key

(* Mapping

   Functional mapping between Json and native F# data structures,
   through statically inferred types. Types providing FromJson and
   ToJson static members with appropriate signatures can be
   seamlessly serialized and deserialized.

   This approach is the same as that taken by the Fleece library,
   credit for which is due to Mauricio Scheffer. *)

[<AutoOpen>]
module Mapping =

    open Operators

    (* From

       Default conversion functions (static members on FromJsonDefaults)
       and statically inferred inline conversion functions for conversion
       from Json to F# data structures. *)

    (* Defaults *)

    type FromJsonDefaults = FromJsonDefaults with

        (* Basic Types *)

        static member inline FromJson (_: unit) =
            Json.Optic.get Json.Null_

        static member inline FromJson (_: bool) =
            Json.Optic.get Json.Bool_

        static member inline FromJson (_: decimal) =
            id <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: float) =
            float <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: int) =
            int <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: int16) =
            int16 <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: int64) =
            int64 <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: single) =
            single <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: string) =
            Json.Optic.get Json.String_

        static member inline FromJson (_: uint16) =
            uint16 <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: uint32) =
            uint32 <!> Json.Optic.get Json.Number_

        static member inline FromJson (_: uint64) =
            uint64 <!> Json.Optic.get Json.Number_

        (* Common Types *)

        static member inline FromJson (_: DateTime) =
                fun x ->
                    match DateTime.TryParseExact (x, [| "s"; "r"; "o" |], null, DateTimeStyles.AdjustToUniversal) with
                    | true, x -> Json.init x
                    | _ -> Json.error "datetime"
            =<< Json.Optic.get Json.String_

        static member inline FromJson (_: DateTimeOffset) =
                fun x ->
                    match DateTimeOffset.TryParseExact (x, [| "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'"; "o"; "r" |], null, DateTimeStyles.AssumeUniversal) with
                    | true, x -> Json.init x
                    | _ -> Json.error "datetimeoffset"
            =<< Json.Optic.get Json.String_

        static member inline FromJson (_: Guid) =
                fun x ->
                    match Guid.TryParse x with
                    | true, x -> Json.init x
                    | _ -> Json.error "guid"
            =<< Json.Optic.get Json.String_

        (* Json Type *)

        static member inline FromJson (_: Json) =
            Json.Optic.get id_

    (* Mapping Functions

       Functions for applying the FromJson function to Json to produce
       new instances of 'a where possible, including folding the FromJson
       function across a list of Json objects. *)

    let inline internal fromJsonDefaults (a: ^a, _: ^b) =
        ((^a or ^b) : (static member FromJson: ^a -> ^a Json) a)

    let inline internal fromJson x =
        fst (fromJsonDefaults (Unchecked.defaultof<'a>, FromJsonDefaults) x)

    let inline internal fromJsonFold init fold xs =
        List.fold (fun r x ->
            match r with
            | Error e ->
                Error e
            | Value xs ->
                match fromJson x with
                | Value x -> Value (fold x xs)
                | Error e -> Error e) (Value init) (List.rev xs)

    (* Defaults *)

    type FromJsonDefaults with

        (* Arrays *)

        static member inline FromJson (_: 'a array) : Json<'a array> =
                fromJsonFold [||] (fun x xs -> Array.append [| x |] xs) >> Json.ofResult
            =<< Json.Optic.get Json.Array_

        (* Lists *)

        static member inline FromJson (_: 'a list) : Json<'a list> =
                fromJsonFold [] (fun x xs -> x :: xs) >> Json.ofResult
            =<< Json.Optic.get Json.Array_

        (* Maps *)

        static member inline FromJson (_: Map<string,'a>) : Json<Map<string,'a>> =
                fun x ->
                    let k, v = (Map.toList >> List.unzip) x
                    List.zip k >> Map.ofList <!> Json.ofResult (fromJsonFold [] (fun x xs -> x :: xs) v)
            =<< Json.Optic.get Json.Object_

        (* Sets *)

        static member inline FromJson (_: Set<'a>) : Json<Set<'a>> =
                fromJsonFold Set.empty Set.add >> Json.ofResult
            =<< Json.Optic.get Json.Array_

        (* Options *)

        static member inline FromJson (_: 'a option) : Json<'a option> =
                function | Null _ -> Json.init None
                         | x -> Some <!> Json.ofResult (fromJson x)
            =<< Json.Optic.get id_

        (* Tuples *)

        static member inline FromJson (_: 'a * 'b) : Json<'a * 'b> =
                function | a :: [b] ->
                                fun a b -> a, b
                            <!> Json.ofResult (fromJson a)
                            <*> Json.ofResult (fromJson b)
                         | _ ->
                            Json.error "tuple2"
            =<< Json.Optic.get Json.Array_

        static member inline FromJson (_: 'a * 'b * 'c) : Json<'a * 'b * 'c> =
                function | a :: b :: [c] ->
                                fun a b c -> a, b, c
                            <!> Json.ofResult (fromJson a)
                            <*> Json.ofResult (fromJson b)
                            <*> Json.ofResult (fromJson c)
                         | _ ->
                            Json.error "tuple3"
            =<< Json.Optic.get Json.Array_

        static member inline FromJson (_: 'a * 'b * 'c * 'd) : Json<'a * 'b * 'c * 'd> =
                function | a :: b :: c :: [d] ->
                                fun a b c d -> a, b, c, d
                            <!> Json.ofResult (fromJson a)
                            <*> Json.ofResult (fromJson b)
                            <*> Json.ofResult (fromJson c)
                            <*> Json.ofResult (fromJson d)
                         | _ ->
                            Json.error "tuple4"
            =<< Json.Optic.get Json.Array_

        static member inline FromJson (_: 'a * 'b * 'c * 'd * 'e) : Json<'a * 'b * 'c * 'd * 'e> =
                function | a :: b :: c :: d :: [e] ->
                                fun a b c d e -> a, b, c, d, e
                            <!> Json.ofResult (fromJson a)
                            <*> Json.ofResult (fromJson b)
                            <*> Json.ofResult (fromJson c)
                            <*> Json.ofResult (fromJson d)
                            <*> Json.ofResult (fromJson e)
                         | _ ->
                            Json.error "tuple5"
            =<< Json.Optic.get Json.Array_

    (* To
    
        *)

    (* Defaults *)

    type ToJsonDefaults = ToJsonDefaults with

        (* Basic Types *)

        static member inline ToJson (x: unit) =
            Json.Optic.set Json.Null_ x

        static member inline ToJson (x: bool) =
            Json.Optic.set Json.Bool_ x

        static member inline ToJson (x: decimal) =
            Json.Optic.set Json.Number_ x

        static member inline ToJson (x: float) =
            match x with
            | x when Double.IsInfinity x -> failwith "Serialization of Infinite Numbers Invalid."
            | x when Double.IsNaN x -> failwith "Serialization of NaN Invalid."
            | x -> Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: int) =
            Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: int16) =
            Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: int64) =
            Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: single) =
            match x with
            | x when Single.IsInfinity x -> failwith "Serialization of Infinite Numbers Invalid."
            | x when Single.IsNaN x -> failwith "Serialization of NaN Invalid."
            | x -> Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: string) =
            Json.Optic.set Json.String_ x

        static member inline ToJson (x: uint16) =
            Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: uint32) =
            Json.Optic.set Json.Number_ (decimal x)

        static member inline ToJson (x: uint64) =
            Json.Optic.set Json.Number_ (decimal x)

        (* Common Types *)

        static member inline ToJson (x: DateTime) =
            Json.Optic.set Json.String_ (x.ToUniversalTime().ToString("o"))
        
        static member inline ToJson (x: DateTimeOffset) =
            Json.Optic.set Json.String_ (x.ToString("o"))

        static member inline ToJson (x: Guid) =
            Json.Optic.set Json.String_ (string x)

        (* Json Type *)

        static member inline ToJson (x: Json) =
            Json.Optic.set id_ x

    (* Mapping Functions

       Functions for applying the ToJson function to data structures to produce
       new Json instances. *)

    let inline internal toJsonDefaults (a: ^a, _: ^b) =
        ((^a or ^b) : (static member ToJson: ^a -> unit Json) a)

    let inline internal toJson (x: 'a) =
        snd (toJsonDefaults (x, ToJsonDefaults) (Object (Map.empty)))

    let inline internal toJsonWith (f:'a -> unit Json) (x: 'a) = 
        snd (f x (Object (Map.empty))) 

    (* Defaults *)

    type ToJsonDefaults with

        (* Arrays *)

        static member inline ToJson (x: 'a array) =
            Json.Optic.set id_ (Array ((Array.toList >> List.map toJson) x))

        (* Lists *)

        static member inline ToJson (x: 'a list) =
            Json.Optic.set id_ (Array (List.map toJson x))

        (* Maps *)

        static member inline ToJson (x: Map<string,'a>) =
            Json.Optic.set id_ (Object (Map.map (fun _ a -> toJson a) x))

        (* Options *)

        static member inline ToJson (x: 'a option) =
            Json.Optic.set id_ ((function | Some a -> toJson a 
                                          | _ -> Null ()) x)

        (* Sets *)

        static member inline ToJson (x: Set<'a>) =
            Json.Optic.set id_ (Array ((Set.toList >> List.map toJson) x))

        (* Tuples *)

        static member inline ToJson ((a, b)) =
            Json.Optic.set id_ (Array [ toJson a; toJson b ])

        static member inline ToJson ((a, b, c)) =
            Json.Optic.set id_ (Array [ toJson a; toJson b; toJson c ])

        static member inline ToJson ((a, b, c, d)) =
            Json.Optic.set id_ (Array [ toJson a; toJson b; toJson c; toJson d ])

        static member inline ToJson ((a, b, c, d, e)) =
            Json.Optic.set id_ (Array [ toJson a; toJson b; toJson c; toJson d; toJson e ])

    (* Functions

        *)

    [<RequireQualifiedAccess>]
    module Json =

        (* Read/Write *)

        let missingMember key =
            fun json ->
                Errors.missingMemberWithJson key (Some JsonFormattingOptions.SingleLine) json
                |> fun e -> Error e, json

        let readMemberWith fromJson key onMissing =
                Json.Optic.tryGet (Json.Object_ >?> Map.key_ key)
            >>= function | Some json -> Json.ofResult (fromJson json)
                         | None -> onMissing ()

        let inline readWith fromJson key =
            readMemberWith fromJson key <| fun () -> missingMember key

        let inline read key =
            readWith fromJson key

        let inline readWithOrDefault fromJson key def =
            readMemberWith fromJson key <| fun () -> Json.init def

        let inline readOrDefault key def =
            readWithOrDefault fromJson key def

        let inline tryReadWith fromJson key =
            readMemberWith fromJson key <| fun () -> Json.init None

        let inline tryRead key =
            tryReadWith fromJson key

        let writeWith toJson key value =
            Json.Optic.set (Json.Object_ >?> Map.value_ key) (Some (toJson value))

        let inline write key value =
            writeWith toJson key value

        let writeWithUnlessDefault toJson key def value =
            match value with
            | v when v = def -> Json.ofResult <| Value ()
            | _ -> writeWith toJson key value

        let inline writeUnlessDefault key def value =
            writeWithUnlessDefault toJson key def value

        let writeNone key =
            Json.Optic.set (Json.Object_ >?> Map.value_ key) (Some (Json.Null ()))

        (* Serialization/Deserialization *)

        let inline deserialize json =
            fromJson json
            |> function | Value a -> a
                        | Error e -> failwith e

        let inline tryDeserialize json =
            fromJson json
            |> function | Value a -> Choice1Of2 a
                        | Error e -> Choice2Of2 e

        let inline serialize a =
            toJson a

        let inline serializeWith f a = 
            toJsonWith f a

(* Patterns

   Active patterns for working with Json data structures, making it
   easier to write code for matching against unions, etc. *)

[<AutoOpen>]
module Patterns =

    open Aether.Operators

    /// Parse a Property from a Json Object token using a supplied fromJson,
    /// and try to deserialize it to the inferred type.
    let inline (|PropertyWith|) fromJson key =
            Optic.get (Json.Object_ >?> Map.key_ key)
         >> Option.bind (fromJson >> function | Value a, _ -> Some a
                                              | _ -> None)

    /// Parse a Property from a Json Object token, and try to deserialize it to the
    /// inferred type.
    let inline (|Property|_|) key =
            Optic.get (Json.Object_ >?> Map.key_ key)
         >> Option.bind (Json.tryDeserialize >> function | Choice1Of2 a -> Some a
                                                         | _ -> None)

[<assembly:System.Runtime.CompilerServices.InternalsVisibleTo ("Chiron.Tests")>]
()