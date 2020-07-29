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
    Html: Option<string>
    Css: Option<string>
    CssFonts: Option<array<string>>
}