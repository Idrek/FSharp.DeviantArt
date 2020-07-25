namespace DeviantArt

type Endpoints = {
    CategoryTree: string
    DailyDeviations: string
    HotDeviations: string
    MoreLikeThis: string
    MoreLikeThisPreview: string
    Newest: string
    Popular: string
    Tags: string
    TagsSearch: string
    Topic: string
    Topics: string
    TopTopics: string
    Undiscovered: string
    UserJournals: string
    CollectionsFolderId: string -> string
    Folders: string
    CommentSiblings: string -> string
    DeviationComments: string -> string
    ProfileComments: string -> string
    StatusComments: string -> string
    Countries: string
    Privacy: string
    Submission: string
    Tos: string
} with
    static member WithDefaults () : Endpoints =
        let prefix = "https://www.deviantart.com/api/v1/oauth2"
        let browse = "browse"
        let collections = "collections"
        let comments = "comments"
        let data = "data"
        {
            CategoryTree = sprintf "%s/%s/categorytree" prefix browse
            DailyDeviations = sprintf "%s/%s/dailydeviations" prefix browse
            HotDeviations = sprintf "%s/%s/hot" prefix browse
            MoreLikeThis = sprintf "%s/%s/morelikethis" prefix browse
            MoreLikeThisPreview = sprintf "%s/%s/morelikethis/preview" prefix browse
            Newest = sprintf "%s/%s/newest" prefix browse
            Popular = sprintf "%s/%s/popular" prefix browse
            Tags = sprintf "%s/%s/tags" prefix browse
            TagsSearch = sprintf "%s/%s/tags/search" prefix browse
            Topic = sprintf "%s/%s/topic" prefix browse
            Topics = sprintf "%s/%s/topics" prefix browse
            TopTopics = sprintf "%s/%s/toptopics" prefix browse
            Undiscovered = sprintf "%s/%s/undiscovered" prefix browse
            UserJournals = sprintf "%s/%s/user/journals" prefix browse
            CollectionsFolderId = sprintf "%s/%s/%s" prefix collections
            Folders = sprintf "%s/%s/folders" prefix collections
            CommentSiblings = sprintf "%s/%s/%s/siblings" prefix comments
            DeviationComments = sprintf "%s/%s/deviation/%s" prefix comments
            ProfileComments = sprintf "%s/%s/profile/%s" prefix comments
            StatusComments = sprintf "%s/%s/profile/%s" prefix comments
            Countries = sprintf "%s/%s/countries" prefix data
            Privacy = sprintf "%s/%s/privacy" prefix data
            Submission = sprintf "%s/%s/submission" prefix data
            Tos = sprintf "%s/%s/tos" prefix data
        }

