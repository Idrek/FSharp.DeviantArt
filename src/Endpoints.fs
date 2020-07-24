namespace DeviantArt

type Endpoints = {
    CategoryTree: string
    DailyDeviations: string
    HotDeviations: string
    MoreLikeThis: string
    MoreLikeThisPreview: string
    Newest: string
    Popular: string
} with
    static member WithDefaults () : Endpoints =
        let prefix = "https://www.deviantart.com/api/v1/oauth2"
        let browse = "browse"
        {
            CategoryTree = sprintf "%s/%s/categorytree" prefix browse
            DailyDeviations = sprintf "%s/%s/dailydeviations" prefix browse
            HotDeviations = sprintf "%s/%s/hot" prefix browse
            MoreLikeThis = sprintf "%s/%s/morelikethis" prefix browse
            MoreLikeThisPreview = sprintf "%s/%s/morelikethis/preview" prefix browse
            Newest = sprintf "%s/%s/newest" prefix browse
            Popular = sprintf "%s/%s/popular" prefix browse
        }

