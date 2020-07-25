module DeviantArt.Types.Comments.Deviation

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Deviation
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
    DeviationId: Guid
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

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    HasLess: bool
    PrevOffset: Option<int>
    Total: Option<int>
    Thread: array<ThreadComment>
}
