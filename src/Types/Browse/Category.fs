module DeviantArt.Types.Browse.Category

// ---------------------------------
// Module aliases
// ---------------------------------

module R = DeviantArt.Rules
module T = Validator.Types
module V = Validator.Api

// ---------------------------------
// Types
// ---------------------------------

[<CLIMutable>]
type Parameters = {
    CategoryPath: string
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateWith "CategoryPath" (fun this -> this.CategoryPath) [
                R.isNotEmptyString
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                categoryPath: string,
                ?matureContent: bool
            ) : Parameters =
        {
            CategoryPath = categoryPath
            MatureContent = defaultArg matureContent false
        }

type Details = {
    Catpath: string
    Title: string
    HasSubcategory: bool
    ParentCatpath: string
}

type Response = {
    Categories: array<Details>
}

