module DeviantArt.Types.Data.Privacy

// ---------------------------------
// Module aliases
// ---------------------------------

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    MatureContent: bool
} with
    static member Initialize 
            (
                ?matureContent: bool
            ) : Parameters =
        {
            MatureContent = defaultArg matureContent false
        }

type Response = {
    Text: string
}

