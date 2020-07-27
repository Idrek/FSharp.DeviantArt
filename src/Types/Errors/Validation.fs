namespace DeviantArt.Types.Errors

module T = Validator.Types

type Validation = {
    Message: string
    Property: string
    Code: string
} with
    static member OfValidator (invalid: T.Invalid) : Validation =
        {
            Message = invalid.Message
            Property = invalid.Property
            Code = invalid.Code
        }
        