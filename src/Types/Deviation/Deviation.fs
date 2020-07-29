module DeviantArt.Types.Deviation.Deviation

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
    DeviationId: Guid
    MatureContent: bool
} with
    static member Initialize 
            (
                deviationId: Guid,
                ?matureContent: bool
            ) : Parameters =
        {
            DeviationId = deviationId
            MatureContent = defaultArg matureContent false
        }

type Response = S.Deviation