module DeviantArt.Rules

// ---------------------------------
// Module aliases
// ---------------------------------

module V = Validator.Api
module T = Validator.Types

// ---------------------------------
// Type aliases
// ---------------------------------

type String = System.String

// ---------------------------------
// Functions
// ---------------------------------

let inRange (rangeMin: int, rangeMax: int) (property: string) : int -> T.Validation =
    let invalid : T.Invalid = {
        Message = sprintf "Value out of range (%d - %d)" rangeMin rangeMax 
        Property = property
        Code = "inRange" 
    }
    V.withFunction invalid (fun (target: int) -> target >= rangeMin && target <= rangeMax)

let isNotEmptySeq (property: string) : seq<'a> -> T.Validation =
    let invalid : T.Invalid = {
        Message = "Collection is empty"
        Property = property
        Code = "isNotEmptySeq"
    }
    V.withFunction invalid (Seq.isEmpty >> not)
    
let isNotEmptyString (property: string) : string -> T.Validation =
    let invalid : T.Invalid = {
        Message = "String is empty"
        Property = property
        Code = "isNotEmptyString"
    }
    V.withFunction invalid (String.IsNullOrWhiteSpace >> not)
    
    