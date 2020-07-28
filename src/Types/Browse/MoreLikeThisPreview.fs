module DeviantArt.Types.Browse.MoreLikeThisPreview

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation

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
    Author: D.User
    MoreFromArtist: array<D.Deviation>
    MoreFromDa: array<D.Deviation>
}
