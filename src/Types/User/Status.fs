module DeviantArt.Types.User.Status

// ---------------------------------
// Module aliases
// ---------------------------------

module S = DeviantArt.Types.Shared

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

type Response = S.Status
