module DeviantArt.Types.Comments.Siblings

// ---------------------------------
// Module aliases
// ---------------------------------


module D = DeviantArt.Types.Common.Deviation
module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid
type ThreadComment = DeviantArt.Types.Common.ThreadComment.ThreadComment

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

type Comment = {
    CommentId: Guid
    ParentId: Option<Guid>
    Posted: string
    Replies: int
    Hidden: Option<string>
    Body: D.Html
    User: D.User
}

type Status = {
    Statusid: Option<Guid>
    Body: Option<D.Html>
    Ts: Option<string>
    Url: Option<string>
    CommentsCount: Option<int>
    IsShare: Option<bool>
    IsDeleted: bool
    Author: Option<D.User>
    Items: Option<array<Item>>
}
and Item = {
    Type: string
    Status: Option<Status>
    Deviation: Option<D.Deviation>
}

type Context = {
    Parent: Option<Comment>
    ItemProfile: Option<D.User>
    ItemDeviation: Option<D.Deviation>
    ItemStatus: Option<Status>
}

type Response = {
    HasMore: Option<bool>
    NextOffset: Option<int>
    HasLess: Option<bool>
    PrevOffset: Option<int>
    Thread: array<ThreadComment>
    Context: Option<array<Context>>
}

