module Json

open Newtonsoft.Json

let private jsonConverter = Fable.JsonConverter() :> JsonConverter

/// Object to Json 
let internal json<'t> (myObj:'t) =   
    JsonConvert.SerializeObject (myObj, [|jsonConverter|])

/// Object from Json 
let internal unjson<'t> (jsonString:string)  : 't =  
    JsonConvert.DeserializeObject<'t> (jsonString, [|jsonConverter|])
