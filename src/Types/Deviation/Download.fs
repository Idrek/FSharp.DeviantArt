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
}

type Response = {
    Src: string
    Filename: string
    Width: int
    Height: int
    Filesize: int
}