namespace DeviantArt.Types

type FailureTokenResponse = {
    Error: string
    ErrorDescription: string
    ErrorDetails: Option<Map<string, string>>
    ErrorCode: Option<int>
} 
