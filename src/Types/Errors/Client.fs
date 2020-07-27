namespace DeviantArt.Types.Errors

type Client =
    | ParametersValidation of Set<Validation>
    | ServerResponse of ServerResponse
    | Exception of Exception
    