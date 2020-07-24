module DeviantArt.Types.Browse.Topics

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

type TopicWithDeviations = {
    Name: string
    CanonicalName: string
    ExampleDeviations: Option<array<D.Deviation>>
    Deviations: Option<array<D.Deviation>>
}

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<TopicWithDeviations>
}

