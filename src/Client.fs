namespace DeviantArt

// ---------------------------------
// Module aliases
// ---------------------------------

module C = HttpFs.Client
module MRequest = C.Request

// ---------------------------------
// Type aliases
// ---------------------------------

type ContentType = HttpFs.Client.ContentType
type TRequest = C.Request

// ---------------------------------
// Functions
// ---------------------------------

type Client = {
    AccessToken: string
    TokenType: string
} with
    static member BuildTokenRequest (clientId: int) (clientSecret: string) (tokenUrl: string) : TRequest =
        let request = 
            MRequest.createUrl (C.HttpMethod.Post) tokenUrl
            |> MRequest.setHeader (
                ContentType.create ("application", "x-www-form-urlencoded")
                |> C.RequestHeader.ContentType)
            |> MRequest.body (C.RequestBody.BodyForm [
                C.FormData.NameValue ("client_id", string clientId)
                C.FormData.NameValue ("client_secret", clientSecret)
                C.FormData.NameValue ("grant_type", "client_credentials")])
        request


