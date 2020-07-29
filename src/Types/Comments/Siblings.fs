module DeviantArt.Types.Comments.Siblings

// ---------------------------------
// Module aliases
// ---------------------------------

module S = DeviantArt.Types.Shared
module R = DeviantArt.Rules
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
    CommentId: Guid
    ExtItem: Option<bool>
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
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                commentId: Guid,
                ?extItem: Option<bool>,
                ?matureContent: bool,
                ?offset: Option<int>,
                ?limit: Option<int>
            ) : Parameters =
        {
            CommentId = commentId
            ExtItem = defaultArg extItem None
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Comment = {
    CommentId: Guid
    ParentId: Option<Guid>
    Posted: string
    Replies: int
    Hidden: Option<string>
    Body: S.Html
    User: S.User
}

type Context = {
    Parent: Option<Comment>
    ItemProfile: Option<S.User>
    ItemDeviation: Option<S.Deviation>
    ItemStatus: Option<S.Status>
}

type Response = {
    HasMore: Option<bool>
    NextOffset: Option<int>
    HasLess: Option<bool>
    PrevOffset: Option<int>
    Thread: array<S.ThreadComment>
    Context: Option<array<Context>>
}

