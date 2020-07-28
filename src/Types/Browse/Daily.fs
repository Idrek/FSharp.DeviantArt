module DeviantArt.Types.Browse.Daily

// ---------------------------------
// Module aliases
// ---------------------------------

module D = DeviantArt.Types.Common.Deviation
module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    Date: Option<string>
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateOptional "Date" (fun this -> this.Date) [
                R.isFormattedDate
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                ?matureContent: bool, 
                ?date: Option<string>
            ) : Parameters =
        {
            Date = defaultArg date None
            MatureContent = defaultArg matureContent false
        }

type Response = {
    Results: array<D.Deviation>
}

