module DeviantArt.Types.Data.Countries

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation
module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    MatureContent: bool
}

type Country = {
    Countryid: int
    Name: string
}

type Response = {
    Results: array<Country>
}

