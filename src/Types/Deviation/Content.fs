module DeviantArt.Types.Deviation.Content

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
    Html: Option<string>
    Css: Option<string>
    CssFonts: Option<array<string>>
}