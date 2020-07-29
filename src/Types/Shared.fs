module DeviantArt.Types.Shared

// ---------------------------------
// Module aliases
// ---------------------------------

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid

// ---------------------------------
// Functions
// --------------------------------

type Deviation = {
    Deviationid: Guid
    Printid: Option<Guid>
    Url: Option<string>
    Title: Option<string>      
    Category: Option<string>
    CategoryPath: Option<string>
    IsFavourited: Option<bool>
    IsDeleted: bool      
    Author: Option<User>        
    Stats: Option<Stats>
    // TODO: Use `DateTimeOffset` ?
    PublishedTime: Option<string>
    AllowsComments: Option<bool>
    Preview: Option<Preview>        
    Content: Option<Content>      
    Thumbs: Option<array<Thumb>>
    Videos: Option<array<Video>>
    Flash: Option<Flash>
    DailyDeviation: Option<DailyDeviation>
    Excerpt: Option<Html>
    IsMature: Option<bool>
    IsDownloadable: Option<bool>
    DownloadFileSize: Option<int>
    // TODO: Missed new elements: `challenge`, `challenge_entry` and `motion_book`
}
and User = {
    Userid: Guid
    Username: string
    Usericon: string
    Type: string
    IsWatching: Option<bool>
    Details: Option<UserDetails>
    Geo: Option<UserGeo>
    Profile: Option<UserProfile>
    Stats: Option<UserStats>
}
and UserDetails = {
    Sex: Option<string>
    Age: Option<int>
    // TODO: Use `DateTimeOffset` ?
    Joindate: string
}
and UserGeo = {
    Country: string
    Countryid: int
    Timezone: string
}
and UserProfile = {
    UserIsArtist: bool
    ArtistLevel: Option<string>
    ArtistSpeciality: Option<string>
    RealName: string
    Tagline: string
    Website: string
    CoverPhoto: string
    ProfilePic: Deviation
}
and UserStats = {
    Watchers: int
    Friends: int
}
and Stats = {
    Comments: int
    Favourites: int
}
and Preview = {
    Src: string
    Height: int
    Width: int
    Transparency: bool
}
and Content = {
    Src: string
    Height: int
    Width: int
    Transparency: bool
    Filesize: int
}
and Thumb = {
    Src: string
    Height: int
    Width: int
    Transparency: bool
}

and Video = {
    Src: string
    Quality: string
    Filesize: int
    Duration: int
}

and Flash = {
    Src: string
    Height: int
    Width: int
}
and DailyDeviation = {
    Body: Html
    // TODO: Use `DateTimeOffset` ?
    Time: string
    Giver: User
    Suggester: Option<User>
}
and Html = string

type Watch = {
    Friend: bool
    Deviations: bool
    Journals: bool
    ForumThreads: bool
    Critiques: bool
    Scraps: bool
    Activity: bool
    Collections: bool
}

type Status = {
    Statusid: Option<Guid>
    Body: Option<Html>
    Ts: Option<string>
    Url: Option<string>
    CommentsCount: Option<int>
    IsShare: Option<bool>
    IsDeleted: bool
    Author: Option<User>
    Items: Option<array<Item>>
}
and Item = {
    Type: string
    Status: Option<Status>
    Deviation: Option<Deviation>
}

type ThreadComment = {
    Commentid: Option<Guid>
    Parentid: Option<Guid>
    Posted: string
    Replies: int
    Hidden: Option<string>
    Body: Html
    User: User
}