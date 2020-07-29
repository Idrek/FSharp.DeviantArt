module DeviantArt.Types.Gallery.Folders

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
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Username: Option<string>
    CalculateSize: Option<bool>
    ExtPreload: Option<bool>
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
            validateOptional "Username" (fun this -> this.Username) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                ?username: Option<string>,
                ?calculateSize: Option<bool>,
                ?extPreload: Option<bool>,
                ?matureContent: bool,
                ?offset: Option<int>, 
                ?limit: Option<int>
            ) : Parameters =
        {
            Username = defaultArg username None
            CalculateSize = defaultArg calculateSize None
            ExtPreload = defaultArg extPreload None
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Folder = {
    Folderid: Guid
    Parent: Option<Guid>
    Name: string
    Size: Option<int>
    Deviations: Option<array<S.Deviation>>
}

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<Folder>
}