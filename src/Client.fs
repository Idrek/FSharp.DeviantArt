namespace DeviantArt

open FSharp.Json
open Hopac

// ---------------------------------
// Module aliases
// ---------------------------------

module C = HttpFs.Client
module Category = DeviantArt.Types.Browse.Category
module Daily = DeviantArt.Types.Browse.Daily
module HotDeviations = DeviantArt.Types.Browse.HotDeviations
module MoreLikeThis = DeviantArt.Types.Browse.MoreLikeThis
module MoreLikeThisPreview = DeviantArt.Types.Browse.MoreLikeThisPreview
module MRequest = C.Request
module MResponse = C.Response
module Newest = DeviantArt.Types.Browse.Newest
module Popular = DeviantArt.Types.Browse.Popular
module S = Settings
module T = Validator.Types
module Tags = DeviantArt.Types.Browse.Tags
module TagsSearch = DeviantArt.Types.Browse.TagsSearch
module Topic = DeviantArt.Types.Browse.Topic
module Topics = DeviantArt.Types.Browse.Topics

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

    static member Create (clientId: int) (clientSecret: string) : Async<Result<Client, FailureTokenResponse>> =
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

    member this.RunRequestJob (request: TRequest) : Job<Result<string, Set<string>>> = 
        job {
            try 
                use! response = this.Dependencies.GetResponse request
                let! bodyJson = this.Dependencies.ReadBodyAsString response
                let result = Ok bodyJson
                return result
            with
            | e ->
                let result = Error (Set [|e.Message|])
                return result
        }

    member this.CategoryTree (parameters: Category.Parameters) : Async<Result<Category.Response, Set<string>>> =
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

    member this.DailyDeviations (parameters: Daily.Parameters) : Async<Result<Daily.Response, Set<string>>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.DailyDeviations
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
            |> Client.AddOptionalQueryString "date" parameters.Date
        job {
            let! json = this.RunRequestJob request
            let deviations = Result.bind (Json.deserializeEx<Daily.Response> S.jsonConfig >> Ok) json
            return deviations
        } |> Job.toAsync 


    member this.HotDeviations (parameters: HotDeviations.Parameters) : Async<Result<HotDeviations.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors -> 
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters -> 
                let request : TRequest = 
                    this.CreateRequest this.Endpoints.HotDeviations
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let deviations = Result.bind (Json.deserializeEx<HotDeviations.Response> S.jsonConfig >> Ok) json
                return deviations
        } |> Job.toAsync

    member this.MoreLikeThis (parameters: MoreLikeThis.Parameters) : Async<Result<MoreLikeThis.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.MoreLikeThis
                    |> Client.AddQueryString "seed" (string validatedParameters.Seed)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let deviations = Result.bind (Json.deserializeEx<MoreLikeThis.Response> S.jsonConfig >> Ok) json
                return deviations                
        } |> Job.toAsync

    member this.MoreLikeThisPreview 
            (parameters: MoreLikeThisPreview.Parameters)
            : Async<Result<MoreLikeThisPreview.Response, Set<string>>> =
        let request : TRequest =
            this.CreateRequest this.Endpoints.MoreLikeThisPreview
            |> Client.AddQueryString "seed" (string parameters.Seed)
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)        
        job {
            let! json = this.RunRequestJob request
            let moreLikeThisPreview = 
                json |> Result.bind (Json.deserializeEx<MoreLikeThisPreview.Response> S.jsonConfig >> Ok)
            return moreLikeThisPreview            
        } |> Job.toAsync

    member this.Newest (parameters: Newest.Parameters) : Async<Result<Newest.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Newest
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "q" validatedParameters.Q
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let newest = Result.bind (Json.deserializeEx<Newest.Response> S.jsonConfig >> Ok) json
                return newest                
        } |> Job.toAsync

    member this.Popular (parameters: Popular.Parameters) : Async<Result<Popular.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Popular
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "q" validatedParameters.Q
                    |> Client.AddOptionalQueryString "timerange" 
                        (Option.map (fun tr -> tr.ToString()) validatedParameters.Timerange)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let popular = Result.bind (Json.deserializeEx<Popular.Response> S.jsonConfig >> Ok) json
                return popular                
        } |> Job.toAsync

    member this.Tags (parameters: Tags.Parameters) : Async<Result<Tags.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Tags
                    |> Client.AddQueryString "tag" validatedParameters.Tag
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let tags = Result.bind (Json.deserializeEx<Tags.Response> S.jsonConfig >> Ok) json
                return tags                
        } |> Job.toAsync

    member this.TagsSearch (parameters: TagsSearch.Parameters) : Async<Result<TagsSearch.Response, Set<string>>> =
        let request : TRequest =
            this.CreateRequest this.Endpoints.TagsSearch
            |> Client.AddQueryString "tag_name" parameters.TagName
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! json = this.RunRequestJob request
            let tagsSearch = Result.bind (Json.deserializeEx<TagsSearch.Response> S.jsonConfig >> Ok) json
            return tagsSearch
        } |> Job.toAsync

    member this.Topic (parameters: Topic.Parameters) : Async<Result<Topic.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Topic
                    |> Client.AddQueryString "topic" validatedParameters.Topic
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let topic = Result.bind (Json.deserializeEx<Topic.Response> S.jsonConfig >> Ok) json
                return topic 
        } |> Job.toAsync

    member this.Topics (parameters: Topics.Parameters) : Async<Result<Topics.Response, Set<string>>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Topics
                    |> Client.AddOptionalQueryString "num_deviations_per_topic" (Option.map string validatedParameters.NumDeviationsPerTopic)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! json = this.RunRequestJob request
                let topics = Result.bind (Json.deserializeEx<Topics.Response> S.jsonConfig >> Ok) json
                return topics 
        } |> Job.toAsync

        

