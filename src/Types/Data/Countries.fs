module DeviantArt.Types.Data.Countries

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    MatureContent: bool
} with
    static member Initialize 
            (
                ?matureContent: bool
            ) : Parameters =
        {
            MatureContent = defaultArg matureContent false
        }

type Country = {
    Countryid: int
    Name: string
}

type Response = {
    Results: array<Country>
}

