module DeviantArt.Types.Browse.Popular

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
            validateOptional "Query" (fun this -> this.Q) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this) 

    static member Initialize 
            (
                ?categoryPath: Option<string>,
                ?q: Option<string>,
                ?timerange: Option<Timerange>,
                ?matureContent: bool,
                ?offset: Option<int>,
                ?limit: Option<int>
            ) : Parameters =
        {
            CategoryPath = defaultArg categoryPath None
            Q = defaultArg q None
            Timerange = defaultArg timerange None
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

