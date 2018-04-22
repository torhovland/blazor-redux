module rec Fable.ReactMarkdownImport

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.React
open Fable.Import.JS

type ReactMarkdownProps =
//    | ClassName of string option
    | Source of string
    | SourcePos of bool option
    | EscapeHtml of bool option
    | SkipHtml of bool option
    // TODO
    // abstract allowNode: (AllowNode -> float -> NodeType -> bool) option
    // abstract allowedTypes: ResizeArray<NodeType> option
    // abstract disallowedTypes: ResizeArray<NodeType> option
    // abstract transformLinkUri: (string -> ReactNode -> string -> string) option
    // abstract transformImageUri: (string -> ReactNode -> string -> string -> string) option
    | UnwrapDisallowed of bool option

    // TODO
    // abstract renderers: obj option

type [<AllowNullLiteral>] AllowNode =
    abstract ``type``: string
    abstract value: string option
    abstract depth: float option
    abstract children: ResizeArray<ReactNode> option

type [<AllowNullLiteral>] SourcePosition =
    abstract line: float
    abstract column: float
    abstract offset: float

type [<AllowNullLiteral>] NodePosition =
    abstract start: SourcePosition
    abstract ``end``: SourcePosition
    abstract indent: ResizeArray<float>

type [<StringEnum>] [<RequireQualifiedAccess>] NodeType =
    | Root
    | Break
    | Paragraph
    | Emphasis
    | Strong
    | ThematicBreak
    | Blockquote
    | Delete
    | Link
    | Image
    | LinkReference
    | ImageReference
    | Table
    | TableHead
    | TableBody
    | TableRow
    | TableCell
    | List
    | ListItem
    | Definition
    | Heading
    | InlineCode
    | Code
    | Html
    | VirtualHtml


let ReactMarkdown : ComponentClass<obj> = importDefault "react-markdown"
let inline reactMarkdown (props : ReactMarkdownProps list) =
    Fable.Helpers.React.from ReactMarkdown (keyValueList CaseRules.LowerFirst props) []
