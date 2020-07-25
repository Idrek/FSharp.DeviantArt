module DeviantArt.Types.Common.ThreadComment

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation

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