module DeviantArt.Types.Browse.Daily

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Date: Option<string>
    MatureContent: bool
} with
    static member Initialize 
            (
                ?matureContent: bool, 
                ?date: Option<string>
            ) : Parameters =
        {
            Date = defaultArg date None
            MatureContent = defaultArg matureContent false
        }

type Response = {
    Results: array<D.Deviation>
}

