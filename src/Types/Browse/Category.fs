module DeviantArt.Types.Browse.Category

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    CategoryPath: string
    MatureContent: bool
}

type Details = {
    Catpath: string
    Title: string
    HasSubcategory: bool
    ParentCatpath: string
}

type Response = {
    Categories: array<Details>
}

