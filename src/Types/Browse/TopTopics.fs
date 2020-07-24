module DeviantArt.Types.Browse.TopTopics

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Deviation
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

type TopTopic = {
    Name: string
    CanonicalName: string
    ExampleDeviations: Option<array<D.Deviation>>
}

type Response = {
    Results: array<TopTopic>
}

