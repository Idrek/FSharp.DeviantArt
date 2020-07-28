module DeviantArt.Types.Browse.Topic

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation
module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Functions
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Topic: string
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
            validateWith "Topic" (fun this -> this.Topic) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<D.Deviation>
}


