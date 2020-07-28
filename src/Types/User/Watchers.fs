module DeviantArt.Types.User.Watchers

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Type aliases
// ---------------------------------

type User = DeviantArt.Types.Common.Deviation.User
type Watch = DeviantArt.Types.User.Friends.Watch

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Username: string
    Offset: Option<int>
    Limit: Option<int>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateOptional "Offset" (fun this -> this.Offset) [
                R.inRange (0, 50000)
            ]
            validateOptional "Limit" (fun this -> this.Limit) [
                R.inRange (1, 50)
            ]
            validateWith "Username" (fun this -> this.Username) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                username: string,
                ?matureContent: bool,
                ?offset: Option<int>, 
                ?limit: Option<int>
            ) : Parameters =
        {
            Username = username
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Watcher = {
    User: User
    IsWatching: bool
    Lastvisit: Option<string>
    Watch: Watch
}

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<Watcher>
}
