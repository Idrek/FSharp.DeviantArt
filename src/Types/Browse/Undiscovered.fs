module DeviantArt.Types.Browse.Undiscovered

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module S = DeviantArt.Types.Shared
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    CategoryPath: Option<string>
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
                R.inRange (1, 120)
            ]
            validateOptional "Category path" (fun this -> this.CategoryPath) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                ?categoryPath: Option<string>,
                ?matureContent: bool,
                ?offset: Option<int>,
                ?limit: Option<int>
            ) : Parameters =
        {
            CategoryPath = defaultArg categoryPath None
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }
            
type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<S.Deviation>
}            


