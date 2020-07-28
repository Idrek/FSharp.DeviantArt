namespace DeviantArt.Types.Errors

type ServerResponse = {
    Error: string
    ErrorDescription: string
    ErrorCode: Option<int>
    ErrorDetails: Option<Map<string, string>>
    Status: string
}

