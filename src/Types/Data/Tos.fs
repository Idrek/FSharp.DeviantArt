module DeviantArt.Types.Data.Tos

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

