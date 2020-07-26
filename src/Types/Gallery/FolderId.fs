module DeviantArt.Types.Gallery.FolderId

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
type User = DeviantArt.Types.Common.Deviation.User

// ---------------------------------
// Types
// ---------------------------------

type Mode = Newest | Popular
with
    override this.ToString() : string =
        match this with
        | Newest -> "newest"
        | Popular -> "popular"


[<CLIMutable>]
type Parameters = {
    Username: Option<string>
    FolderId: Option<Guid>
    Mode: Option<Mode>
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
                R.inRange (1, 24)
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                ?username: Option<string>,
                ?folderId: Option<Guid>,
                ?mode: Option<Mode>,
                ?matureContent: bool,
                ?offset: Option<int>, 
                ?limit: Option<int>
            ) : Parameters =
        {
            Username = defaultArg username None
            FolderId = defaultArg folderId None
            Mode = defaultArg mode (Some Popular)
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Name: Option<string>
    Results: array<Deviation>
}