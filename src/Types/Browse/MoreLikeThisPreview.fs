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
}

type Response = {
    Seed: Guid
    Author: D.User
    MoreFromArtist: array<D.Deviation>
    MoreFromDa: array<D.Deviation>
}
