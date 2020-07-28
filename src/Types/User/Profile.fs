module DeviantArt.Types.User.Profile

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Type aliases
// ---------------------------------

type Deviation = DeviantArt.Types.Common.Deviation.Deviation
type Guid = System.Guid
type Html = DeviantArt.Types.Common.Deviation.Html
// TODO: Move `Status` to common.
type Status = DeviantArt.Types.Comments.Siblings.Status
type User = DeviantArt.Types.Common.Deviation.User

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Username: string
    ExtCollections: Option<bool>
    ExtGalleries: Option<bool>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateWith "Username" (fun this -> this.Username) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                username: string,
                ?extCollections: Option<bool>,
                ?extGalleries: Option<bool>,
                ?matureContent: bool
            ) : Parameters =
        {
            Username = username
            ExtCollections = defaultArg extCollections None
            ExtGalleries = defaultArg extGalleries None
            MatureContent = defaultArg matureContent false
        }

type Stats = {
    UserDeviations: int
    UserFavourites: int
    UserComments: int
    ProfilePageviews: int
    ProfileComments: int
}

type Collection = {
    Folderid: Guid
    Name: string
}

type Gallery = {
    Folderid: Guid
    Parent: Option<Guid>
    Name: string
}

type Response = {
    User: User
    IsWatching: bool
    ProfileUrl: string
    UserIsArtist: bool
    ArtistLevel: Option<string>
    ArtistSpecialty: Option<string>
    RealName: string
    Tagline: string
    Countryid: int
    Country: string
    Website: string
    Bio: Html
    CoverPhoto: Option<string>
    ProfilePic: Option<Deviation>
    LastStatus: Option<Status>
    Stats: Stats
    Collections: Option<array<Collection>>
    Galleries: Option<array<Gallery>>
}