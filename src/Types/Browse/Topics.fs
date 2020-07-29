module DeviantArt.Types.Browse.Topics

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
    NumDeviationsPerTopic: Option<int>
    Offset: Option<int>
    Limit: Option<int>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateOptional "Offset" (fun this -> this.Offset) [
                R.inRange (0, 50000)
            ]
            validateOptional "Limit" (fun this -> this.Limit) [
                R.inRange (1, 10)
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                ?numDeviationsPerTopic: Option<int>,
                ?matureContent: bool,
                ?offset: Option<int>,
                ?limit: Option<int>
            ) : Parameters =
        {
            NumDeviationsPerTopic = defaultArg numDeviationsPerTopic None
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type TopicWithDeviations = {
    Name: string
    CanonicalName: string
    ExampleDeviations: Option<array<S.Deviation>>
    Deviations: Option<array<S.Deviation>>
}

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<TopicWithDeviations>
}

