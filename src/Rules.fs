module DeviantArt.Rules

// ---------------------------------
// Module aliases
// ---------------------------------

module V = Validator.Api
module T = Validator.Types

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

    