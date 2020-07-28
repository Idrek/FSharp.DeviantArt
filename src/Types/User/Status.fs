module DeviantArt.Types.User.Status

// ---------------------------------
// Module aliases
// ---------------------------------

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid
// TODO: Move `Status` to common or shared.
type Status = DeviantArt.Types.Comments.Siblings.Status

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    StatusId: Guid
    MatureContent: bool
} with
    static member Initialize 
            (
                statusId: Guid,
                ?matureContent: bool
            ) : Parameters =
        {
            StatusId = statusId
            MatureContent = defaultArg matureContent false
        }

type Response = Status
