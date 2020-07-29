module DeviantArt.Types.Browse.TopTopics

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module S = DeviantArt.Types.Shared
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

type TopTopic = {
    Name: string
    CanonicalName: string
    ExampleDeviations: Option<array<S.Deviation>>
}

type Response = {
    Results: array<TopTopic>
}

