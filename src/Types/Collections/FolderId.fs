module DeviantArt.Types.Collections.FolderId

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation
module R = DeviantArt.Rules
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
    Folderid: Guid
    Username: Option<string>
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
                R.inRange (1, 24)
            ]
            validateOptional "Username" (fun this -> this.Username) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Name: Option<string>
    Results: array<D.Deviation>
}       

