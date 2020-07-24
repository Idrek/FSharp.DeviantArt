namespace DeviantArt

open FSharp.Json
open Hopac

// ---------------------------------
// Module aliases
// ---------------------------------

module C = HttpFs.Client
module Category = DeviantArt.Types.Browse.Category
module Daily = DeviantArt.Types.Browse.Daily
module MRequest = C.Request
module MResponse = C.Response
module S = Settings

// ---------------------------------
// Type aliases
// ---------------------------------

// ---------------------------------
// Functions
// ---------------------------------

type Client = {
    ExpiresAt: DateTimeOffset
    AccessToken: string
    TokenType: string
    Dependencies: Dependencies
    Endpoints: Endpoints
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

    static member BuildClient
            (dependencies: Dependencies)
            (response: AccessTokenResponse)
            : Client =
        let endpoints : Endpoints = Endpoints.WithDefaults ()
        let client : Client = { 
            ExpiresAt = response.ExpiresIn |> float |> dependencies.CreationTime.AddSeconds
            AccessToken = response.AccessToken
            TokenType = response.TokenType
            Dependencies = dependencies
            Endpoints = endpoints
        }
        client

    static member CreateWithDependencies 
            (dependencies: Dependencies)
            (clientId: int) 
            (clientSecret: string) 
            : Async<Result<Client, FailureTokenResponse>> =
        job {
            let request : TRequest = Client.BuildTokenRequest clientId clientSecret dependencies.TokenUrl
            let! (tokenChoice : Choice<TResponse, exn>) = dependencies.TryGetResponse request
            let! (tokenResponse : Result<AccessTokenResponse, FailureTokenResponse>) = 
                Client.BuildTokenResponseJob tokenChoice dependencies.ReadBodyAsString            
            let client : Result<Client, FailureTokenResponse> = 
                tokenResponse |> Result.map (Client.BuildClient dependencies) 
            return client              
        } |> Job.toAsync

    static member Create (clientId: int) (clientSecret: string) =
        let dependencies : Dependencies = { 
            TryGetResponse = C.tryGetResponse
            ReadBodyAsString = MResponse.readBodyAsString
            GetResponse = C.getResponse 
            // Used to calculate expiration. Forbid token usage 60 seconds before the time.
            CreationTime = DateTimeOffset.UtcNow.AddSeconds(-60.0)
            TokenUrl = "https://www.deviantart.com/oauth2/token"
        }
        Client.CreateWithDependencies dependencies clientId clientSecret

    member this.CreateRequest (url: string) : TRequest =
        let tokenHeader = sprintf "%s %s" this.TokenType this.AccessToken
        MRequest.createUrl (C.HttpMethod.Get) url
        |> MRequest.setHeader (C.RequestHeader.Authorization tokenHeader)

    static member AddQueryString (name: string) (value: string) (request: TRequest) : TRequest =
        MRequest.queryStringItem name value request

    static member AddOptionalQueryString (name: string) (value: option<string>) (request: TRequest) : TRequest =
        match value with
        | None -> request
        | Some v -> Client.AddQueryString name v request

    member this.RunRequestJob (request: TRequest) : Job<Result<string, array<string>>> = 
        job {
            try 
                use! response = this.Dependencies.GetResponse request
                let! bodyJson = this.Dependencies.ReadBodyAsString response
                let result = Ok bodyJson
                return result
            with
            | e ->
                let result = Error [|e.Message|]
                return result
        }

    member this.CategoryTree (parameters: Category.Parameters) : Async<Result<Category.Response, array<string>>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.CategoryTree 
            |> Client.AddQueryString "catpath" parameters.CategoryPath
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! json = this.RunRequestJob request
            let categories = 
                Result.bind (Json.deserializeEx<Category.Response> S.jsonConfig >> Ok) json
            return categories
        } |> Job.toAsync 

    member this.DailyDeviations (parameters: Daily.Parameters) : Async<Result<Daily.Response, array<string>>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.DailyDeviations
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
            |> Client.AddOptionalQueryString "date" parameters.Date
        job {
            let! json = this.RunRequestJob request
            let deviations = Result.bind (Json.deserializeEx<Daily.Response> S.jsonConfig >> Ok) json
            return deviations
        } |> Job.toAsync 

