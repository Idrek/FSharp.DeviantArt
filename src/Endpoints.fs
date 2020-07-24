namespace DeviantArt

type Endpoints = {
    CategoryTree: string
    DailyDeviations: string
    HotDeviations: string
} with
    static member WithDefaults () : Endpoints =
        let prefix = "https://www.deviantart.com/api/v1/oauth2"
        let browse = "browse"
        {
            CategoryTree = sprintf "%s/%s/categorytree" prefix browse
            DailyDeviations = sprintf "%s/%s/dailydeviations" prefix browse
            HotDeviations = sprintf "%s/%s/hot" prefix browse
        }

