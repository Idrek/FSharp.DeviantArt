module DeviantArt.Types.Browse.TagsSearch

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
    TagName: string
    MatureContent: bool
} with
    member this.Validate () : Result<Parameters, Set<T.Invalid>> =
        let v = V.validator<Parameters>() {
            validateWith "Tag name" (fun this -> this.TagName) [
                R.stringIsLongerThan 2
            ]
        }
        v this |> Result.map (fun _ -> this)

    static member Initialize 
            (
                tagName: string,
                ?matureContent: bool
            ) : Parameters =
        {
            TagName = tagName
            MatureContent = defaultArg matureContent false
        }

type TagName = {
    TagName: string
}

type Response = {
    Results: array<TagName>
}

