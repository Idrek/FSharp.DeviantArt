module DeviantArt.Types.Deviation.Metadata

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module S = DeviantArt.Types.Shared
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    DeviationIds: array<Guid>
    ExtSubmission: Option<bool>
    ExtCamera: Option<bool>
    ExtStats: Option<bool>
    ExtCollection: Option<bool>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateWith "DeviationIds" (fun this -> this.DeviationIds) [
                R.isNotEmptySeq
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                deviationIds: array<Guid>, 
                ?matureContent: bool,
                ?extSubmission: Option<bool>, 
                ?extCamera: Option<bool>, 
                ?extStats: Option<bool>,
                ?extCollection: Option<bool>
            ) : Parameters =
        {
            DeviationIds = deviationIds
            MatureContent = defaultArg matureContent false
            ExtSubmission = defaultArg extSubmission None
            ExtCamera = defaultArg extCamera None
            ExtStats = defaultArg extStats None
            ExtCollection = defaultArg extCollection None
        }

type Tag = {
    TagName: string
    Sponsored: bool
    Sponsor: string
}

type SubmittedWith = {
    App: string
    Url: string
}

type Submission = {
    CreationTime: string
    Category: string
    FileSize: Option<string>
    Resolution: Option<string>
    SubmittedWith: SubmittedWith
}

type Stats = {
    Views: int
    ViewsToday: int
    Favourites: int
    Comments: int
    Downloads: int
    DownloadsToday: int
}

type Collection = {
    Folderid: Guid
    Name: string
}

type Metadata = {
    Deviationid: Guid
    Printid: Option<Guid>
    Author: S.User
    IsWatching: bool
    Title: string
    Description: S.Html
    License: string
    AllowsComments: bool
    Tags: array<Tag>
    IsFavourited: bool
    IsMature: bool
    Submission: Option<Submission>
    Stats: Option<Stats>
    Collections: Option<array<Collection>>
}

type Response = {
    Metadata: array<Metadata>
}