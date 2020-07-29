module DeviantArt.Types.Deviation.WhoFaved

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module S = DeviantArt.Types.Shared
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    DeviationId: Guid
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
                R.inRange (1, 50)
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                deviationId: Guid, 
                ?matureContent: bool,
                ?offset: Option<int>, 
                ?limit: Option<int>
            ) : Parameters =
        {
            DeviationId = deviationId
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Who = {
    User: S.User
    Time: int
}

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<Who>
}