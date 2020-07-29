module DeviantArt.Types.Deviation.EmbeddedContent

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Type aliases
// ---------------------------------

type Deviation = DeviantArt.Types.Common.Deviation.Deviation
type Guid = System.Guid

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    DeviationId: Guid
    OffsetDeviationId: Option<Guid>
    Offset: Option<int>
    Limit: Option<int>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateOptional "Offset" (fun this -> this.Offset) [
                R.inRange (-50000, 50000)
            ]
            validateOptional "Limit" (fun this -> this.Limit) [
                R.inRange (1, 50)
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                deviationId: Guid, 
                ?matureContent: bool,
                ?offsetDeviationId: Option<Guid>, 
                ?offset: Option<int>, 
                ?limit: Option<int>
            ) : Parameters =
        {
            DeviationId = deviationId
            MatureContent = defaultArg matureContent false
            OffsetDeviationId = defaultArg offsetDeviationId None
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    HasLess: bool
    PrevOffset: Option<int>
    Results: array<Deviation>
}