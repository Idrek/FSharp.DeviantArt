module DeviantArt.Types.Browse.MoreLikeThisPreview

// ---------------------------------
// Module aliases
// ---------------------------------

module S = DeviantArt.Types.Shared

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Seed: Guid
    MatureContent: bool
} with
    static member Initialize 
            (
                seed: Guid,
                ?matureContent: bool
            ) : Parameters =
        {
            Seed = seed
            MatureContent = defaultArg matureContent false
        }

type Response = {
    Seed: Guid
    Author: S.User
    MoreFromArtist: array<S.Deviation>
    MoreFromDa: array<S.Deviation>
}
