module DeviantArt.Types.Common

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Deviation

// ---------------------------------
// Type aliases
// ---------------------------------

type Guid = System.Guid

// ---------------------------------
// Types
// ---------------------------------

type ThreadComment = {
    Commentid: Option<Guid>
    Parentid: Option<Guid>
    Posted: string
    Replies: int
    Hidden: Option<string>
    Body: D.Html
    User: D.User
}