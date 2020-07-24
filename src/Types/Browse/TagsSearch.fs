module DeviantArt.Types.Browse.TagsSearch

[<CLIMutable>]
type Parameters = {
    TagName: string
    MatureContent: bool
}

type TagName = {
    TagName: string
}

type Response = {
    Results: array<TagName>
}

