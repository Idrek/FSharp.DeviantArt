module DeviantArt.Types.Browse.MoreLikeThis

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
    Seed: Guid
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
                R.inRange (1, 50)
            ]
        }
        v this |> Result.map (fun _ -> this)
        // match this with
        // | { Offset = offset; Limit = limit } ->
        //     let errors : array<string> = 
        //         [|
        //             Validation.inRange (0, 50000) "Offset" offset
        //             Validation.inRange (1, 50) "Limit" limit
        //         |] 
        //         |> Array.choose (fun vOpt -> vOpt)
        //     if Array.isEmpty errors then Ok this else Error errors

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    Results: array<D.Deviation>
}