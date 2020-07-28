module DeviantArt.Types.Browse.Popular

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

type Timerange = 
    | Hours8
    | Hours24
    | Days3
    | Week1
    | Month1
    | AllTime
with
    override this.ToString () : string =
        match this with
        | Hours8 -> "8hr"
        | Hours24 -> "24hr"
        | Days3 -> "3days"
        | Week1 -> "1week"
        | Month1 -> "1month"
        | AllTime -> "alltime"

[<CLIMutable>]
type Parameters = {
    CategoryPath: Option<string>
    Q: Option<string>
    Timerange: Option<Timerange>
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

type Response = {
    HasMore: bool
    NextOffset: Option<int>
    EstimatedTotal: Option<int>
    Results: array<D.Deviation>
}   

