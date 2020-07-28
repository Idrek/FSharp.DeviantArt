module DeviantArt.Rules

// ---------------------------------
// Module aliases
// ---------------------------------

module V = Validator.Api
module T = Validator.Types

// ---------------------------------
// Type aliases
// ---------------------------------

type Regex = System.Text.RegularExpressions.Regex
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
    
let isFormattedDate (property: string) : string -> T.Validation =
    let invalid : T.Invalid = {
        Message = "Date must have `YYYY-MM-DD` format"
        Property = property
        Code = "isFormattedDate"
    }    
    V.withFunction invalid (fun (target: string) -> Regex.IsMatch(target, @"^\d{4}-\d{2}-\d{2}$"))

let stringIsLongerThan (length: int) (property: string) : string -> T.Validation =
    let invalid : T.Invalid = {
        Message = sprintf "String must be longer than %d characters" length
        Property = property
        Code = "stringIsLongerThan"
    }
    V.withFunction invalid (fun (target: string) -> target.Length > length)
    
    