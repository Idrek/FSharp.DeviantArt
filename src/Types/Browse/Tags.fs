module DeviantArt.Types.Browse.Tags

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
    Tag: string
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
            validateWith "Tag" (fun this -> this.Tag) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this) 

    static member Initialize 
            (
                tag: string,
                ?matureContent: bool,
                ?offset: Option<int>,
                ?limit: Option<int>
            ) : Parameters =
        {
            Tag = tag
            MatureContent = defaultArg matureContent false
            Offset = defaultArg offset None
            Limit = defaultArg limit None
        }

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    EstimatedTotal: Option<int>
    Results: array<S.Deviation>
}   

