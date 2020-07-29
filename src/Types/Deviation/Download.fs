module DeviantArt.Types.Deviation.Download

// ---------------------------------
// Module aliases
// ---------------------------------

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

type Response = {
    Src: string
    Filename: string
    Width: int
    Height: int
    Filesize: int
}