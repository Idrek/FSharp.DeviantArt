module DeviantArt.Types.Browse.Daily

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Deviation

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Date: Option<string>
    MatureContent: bool
}

type Response = {
    Results: array<D.Deviation>
}
