module DeviantArt.Types.Comments.Status

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
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    StatusId: Guid
    CommentId: Option<Guid>
    MaxDepth: Option<int>
    Offset: Option<int>
    Limit: Option<int>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateOptional "Offset" (fun this -> this.Offset) [
                R.inRange (-10000, 10000)
            ]
            validateOptional "Limit" (fun this -> this.Limit) [
                R.inRange (1, 50)
            ]
            validateOptional "Max depth" (fun this -> this.MaxDepth) [
                R.inRange (0, 5)
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                statusId: Guid,
                ?commentId: Option<Guid>,
                ?maxDepth: Option<int>,
                ?matureContent: bool,
                ?offset: Option<int>,
                ?limit: Option<int>
            ) : Parameters =
        {
            StatusId = statusId
            CommentId = defaultArg commentId None
            MaxDepth = defaultArg maxDepth None
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    HasLess: bool
    PrevOffset: Option<int>
    Total: Option<int>
    Thread: array<S.ThreadComment>
}
