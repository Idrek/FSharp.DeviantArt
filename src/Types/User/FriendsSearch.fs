module DeviantArt.Types.User.FriendsSearch

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

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Username: string
    Query: string
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateWith "Username" (fun this -> this.Username) [
                R.isNotEmptyString
            ]
            validateWith "Query" (fun this -> this.Query) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                username: string,
                query: string,
                ?matureContent: bool
            ) : Parameters =
        {
            Username = username
            Query = query
            MatureContent = defaultArg matureContent false
        }

type Response = {
    Results: array<User>
}