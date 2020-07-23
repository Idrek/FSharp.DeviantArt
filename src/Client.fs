namespace DeviantArt

open FSharp.Json
open Hopac

// ---------------------------------
// Module aliases
// ---------------------------------

module C = HttpFs.Client
module MRequest = C.Request
module S = Settings

// ---------------------------------
// Type aliases
// ---------------------------------

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

    static member BuildTokenResponseJob
            (choice: Choice<TResponse, exn>)
            (readBodyAsString: TResponse -> Job<string>)
            : Job<Result<AccessTokenResponse, FailureTokenResponse>> =
        let buildSuccessResponse (json: string) : AccessTokenResponse =
            json |> Json.deserializeEx<AccessTokenResponse> S.jsonConfig
        let buildFailureResponse (json: string) : FailureTokenResponse =
            json |> Json.deserializeEx<FailureTokenResponse> S.jsonConfig
        let buildClientErrorResponse (description: string) : FailureTokenResponse =
            {
                Error = "client_error"
                ErrorDescription = description
                ErrorDetails = None
                ErrorCode = None
            }                   
        job {
            try            
                match choice with
                | Choice2Of2 exn -> 
                    let error = Error <| buildClientErrorResponse exn.Message
                    return error
                | Choice1Of2 response ->  
                    let! bodyJson = readBodyAsString response
                    match response.statusCode with
                    | 200 ->
                        let successResponse = Ok <| buildSuccessResponse bodyJson
                        return successResponse
                    | _ -> 
                        let failureResponse = Error <| buildFailureResponse bodyJson
                        return failureResponse
            with
            | e ->
                let error = Error <| buildClientErrorResponse e.Message
                return error                
        }      

