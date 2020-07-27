namespace DeviantArt

open FSharp.Json
open Hopac

// ---------------------------------
// Module aliases
// ---------------------------------

module C = HttpFs.Client
module Category = DeviantArt.Types.Browse.Category
module Content = DeviantArt.Types.Deviation.Content
module Countries = DeviantArt.Types.Data.Countries
module Daily = DeviantArt.Types.Browse.Daily
module Deviation = DeviantArt.Types.Deviation.Deviation
module DeviationComments = DeviantArt.Types.Comments.Deviation
module Download = DeviantArt.Types.Deviation.Download
module EmbeddedContent = DeviantArt.Types.Deviation.EmbeddedContent
module FolderId = DeviantArt.Types.Collections.FolderId
module Folders = DeviantArt.Types.Collections.Folders
module GalleryAll = DeviantArt.Types.Gallery.All
module GalleryFolderId = DeviantArt.Types.Gallery.FolderId
module GalleryFolders = DeviantArt.Types.Gallery.Folders
module HotDeviations = DeviantArt.Types.Browse.HotDeviations
module Metadata = DeviantArt.Types.Deviation.Metadata
module MoreLikeThis = DeviantArt.Types.Browse.MoreLikeThis
module MoreLikeThisPreview = DeviantArt.Types.Browse.MoreLikeThisPreview
module MRequest = C.Request
module MResponse = C.Response
module Newest = DeviantArt.Types.Browse.Newest
module Popular = DeviantArt.Types.Browse.Popular
module Privacy = DeviantArt.Types.Data.Privacy
module ProfileComments = DeviantArt.Types.Comments.Profile
module S = Settings
module Siblings = DeviantArt.Types.Comments.Siblings
module StatusComments = DeviantArt.Types.Comments.Status
module Submission = DeviantArt.Types.Data.Submission
module T = Validator.Types
module Tags = DeviantArt.Types.Browse.Tags
module TagsSearch = DeviantArt.Types.Browse.TagsSearch
module Topic = DeviantArt.Types.Browse.Topic
module Topics = DeviantArt.Types.Browse.Topics
module TopTopics = DeviantArt.Types.Browse.TopTopics
module Tos = DeviantArt.Types.Data.Tos
module Undiscovered = DeviantArt.Types.Browse.Undiscovered
module UserJournals = DeviantArt.Types.Browse.UserJournals
module WhoFaved = DeviantArt.Types.Deviation.WhoFaved

// ---------------------------------
// Type aliases
// ---------------------------------

type ErrorClient = DeviantArt.Types.Errors.Client
type ErrorException = DeviantArt.Types.Errors.Exception
type ErrorServerResponse = DeviantArt.Types.Errors.ServerResponse
type ErrorValidation = DeviantArt.Types.Errors.Validation
type StreamReader = System.IO.StreamReader

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

    static member AddMultipleValueQueryString 
            (name: string) 
            (values: seq<string>) 
            (request: TRequest) 
            : TRequest =
        (request, values) 
        ||> Seq.fold (fun request value -> Client.AddQueryString name value request)

    member this.RunRequestJob (request: TRequest) : Job<Result<string, ErrorClient>> = 
        job {
            try 
                use! response = this.Dependencies.GetResponse request
                match response.statusCode with
                | statusCode when statusCode < 400 -> 
                    let! bodyJson = this.Dependencies.ReadBodyAsString response
                    let result = Ok bodyJson
                    return result
                | _ -> 
                    use reader : StreamReader = new StreamReader(response.body)
                    let body : string = reader.ReadToEnd()
                    let error = Json.deserializeEx<ErrorServerResponse> S.jsonConfig body
                    let result = Error (ErrorClient.ServerResponse error)
                    return result
            with
            | e ->
                let ex : ErrorException = { Name = e.GetType().ToString(); Message = e.Message }
                let result = Error (ErrorClient.Exception ex)
                return result
        }

    member this.CategoryTree 
            (parameters: Category.Parameters) 
            : Async<Result<Category.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.CategoryTree 
            |> Client.AddQueryString "catpath" parameters.CategoryPath
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let categories : Result<Category.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Category.Response> S.jsonConfig >> Ok) json
            return categories
        } |> Job.toAsync 

    member this.DailyDeviations 
            (parameters: Daily.Parameters) 
            : Async<Result<Daily.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.DailyDeviations
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
            |> Client.AddOptionalQueryString "date" parameters.Date
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let deviations : Result<Daily.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Daily.Response> S.jsonConfig >> Ok) json
            return deviations
        } |> Job.toAsync 

    member this.HotDeviations 
            (parameters: HotDeviations.Parameters) 
            : Async<Result<HotDeviations.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors -> 
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters -> 
                let request : TRequest = 
                    this.CreateRequest this.Endpoints.HotDeviations
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let deviations : Result<HotDeviations.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<HotDeviations.Response> S.jsonConfig >> Ok) json
                return deviations
        } |> Job.toAsync

    member this.MoreLikeThis 
            (parameters: MoreLikeThis.Parameters) 
            : Async<Result<MoreLikeThis.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.MoreLikeThis
                    |> Client.AddQueryString "seed" (string validatedParameters.Seed)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let deviations : Result<MoreLikeThis.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<MoreLikeThis.Response> S.jsonConfig >> Ok) json
                return deviations                
        } |> Job.toAsync

    member this.MoreLikeThisPreview 
            (parameters: MoreLikeThisPreview.Parameters)
            : Async<Result<MoreLikeThisPreview.Response, ErrorClient>> =
        let request : TRequest =
            this.CreateRequest this.Endpoints.MoreLikeThisPreview
            |> Client.AddQueryString "seed" (string parameters.Seed)
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)        
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let moreLikeThisPreview : Result<MoreLikeThisPreview.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<MoreLikeThisPreview.Response> S.jsonConfig >> Ok) json
            return moreLikeThisPreview            
        } |> Job.toAsync

    member this.Newest 
            (parameters: Newest.Parameters) 
            : Async<Result<Newest.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Newest
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "q" validatedParameters.Q
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let newest : Result<Newest.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Newest.Response> S.jsonConfig >> Ok) json
                return newest                
        } |> Job.toAsync

    member this.Popular 
            (parameters: Popular.Parameters) 
            : Async<Result<Popular.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
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
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let popular : Result<Popular.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Popular.Response> S.jsonConfig >> Ok) json
                return popular                
        } |> Job.toAsync

    member this.Tags 
            (parameters: Tags.Parameters) 
            : Async<Result<Tags.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Tags
                    |> Client.AddQueryString "tag" validatedParameters.Tag
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let tags : Result<Tags.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Tags.Response> S.jsonConfig >> Ok) json
                return tags                
        } |> Job.toAsync

    member this.TagsSearch 
            (parameters: TagsSearch.Parameters) 
            : Async<Result<TagsSearch.Response, ErrorClient>> =
        let request : TRequest =
            this.CreateRequest this.Endpoints.TagsSearch
            |> Client.AddQueryString "tag_name" parameters.TagName
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let tagsSearch : Result<TagsSearch.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<TagsSearch.Response> S.jsonConfig >> Ok) json
            return tagsSearch
        } |> Job.toAsync

    member this.Topic 
            (parameters: Topic.Parameters) 
            : Async<Result<Topic.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Topic
                    |> Client.AddQueryString "topic" validatedParameters.Topic
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let topic : Result<Topic.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Topic.Response> S.jsonConfig >> Ok) json
                return topic 
        } |> Job.toAsync

    member this.Topics 
            (parameters: Topics.Parameters) 
            : Async<Result<Topics.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Topics
                    |> Client.AddOptionalQueryString "num_deviations_per_topic" (Option.map string validatedParameters.NumDeviationsPerTopic)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let topics : Result<Topics.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Topics.Response> S.jsonConfig >> Ok) json
                return topics 
        } |> Job.toAsync

    member this.TopTopics 
            (parameters: TopTopics.Parameters) 
            : Async<Result<TopTopics.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.TopTopics
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let topTopics : Result<TopTopics.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<TopTopics.Response> S.jsonConfig >> Ok) json
            return topTopics
        } |> Job.toAsync

    member this.Undiscovered 
            (parameters: Undiscovered.Parameters) 
            : Async<Result<Undiscovered.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Undiscovered
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "category_path" validatedParameters.CategoryPath
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let undiscovered : Result<Undiscovered.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Undiscovered.Response> S.jsonConfig >> Ok) json
                return undiscovered
        } |> Job.toAsync

    member this.UserJournals 
            (parameters: UserJournals.Parameters) 
            : Async<Result<UserJournals.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.UserJournals
                    |> Client.AddQueryString "username" validatedParameters.Username
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "featured" (Option.map string validatedParameters.Featured)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let userJournals : Result<UserJournals.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<UserJournals.Response> S.jsonConfig >> Ok) json
                return userJournals                
        } |> Job.toAsync

    member this.CollectionsFolderId 
            (parameters: FolderId.Parameters)
            : Async<Result<FolderId.Response, ErrorClient>> =
        job {
            match parameters.Validate () with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest (validatedParameters.Folderid |> string |> this.Endpoints.CollectionsFolderId)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "username" validatedParameters.Username
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let collectionsFolderId : Result<FolderId.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<FolderId.Response> S.jsonConfig >> Ok) json
                return collectionsFolderId                
        } |> Job.toAsync

    member this.Folders 
            (parameters: Folders.Parameters)
            : Async<Result<Folders.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Folders
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "username" validatedParameters.Username
                    |> Client.AddOptionalQueryString "calculate_size" (Option.map string validatedParameters.CalculateSize)
                    |> Client.AddOptionalQueryString "ext_preload" (Option.map string validatedParameters.ExtPreload)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let folders : Result<Folders.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Folders.Response> S.jsonConfig >> Ok) json
                return folders                                
        } |> Job.toAsync

    member this.CommentSiblings
            (parameters: Siblings.Parameters)
            : Async<Result<Siblings.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest (validatedParameters.CommentId |> string |> this.Endpoints.CommentSiblings)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "ext_item" (Option.map string validatedParameters.ExtItem)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let siblings : Result<Siblings.Response, ErrorClient> = 
                    Result.bind (Json.deserializeEx<Siblings.Response> S.jsonConfig >> Ok) json
                return siblings
        } |> Job.toAsync

    member this.DeviationComments
            (parameters: DeviationComments.Parameters)
            : Async<Result<DeviationComments.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest (validatedParameters.DeviationId |> string |> this.Endpoints.DeviationComments)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "commentid" (Option.map string validatedParameters.CommentId)
                    |> Client.AddOptionalQueryString "maxdepth" (Option.map string validatedParameters.MaxDepth)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let comments : Result<DeviationComments.Response, ErrorClient> =
                    Result.bind (Json.deserializeEx<DeviationComments.Response> S.jsonConfig >> Ok) json
                return comments
        } |> Job.toAsync

    member this.ProfileComments
            (parameters: ProfileComments.Parameters)
            : Async<Result<ProfileComments.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest (validatedParameters.Username |> string |> this.Endpoints.ProfileComments)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "commentid" (Option.map string validatedParameters.CommentId)
                    |> Client.AddOptionalQueryString "maxdepth" (Option.map string validatedParameters.MaxDepth)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let comments : Result<ProfileComments.Response, ErrorClient> =
                    Result.bind (Json.deserializeEx<ProfileComments.Response> S.jsonConfig >> Ok) json
                return comments
        } |> Job.toAsync

    member this.StatusComments
            (parameters: StatusComments.Parameters)
            : Async<Result<StatusComments.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest (validatedParameters.StatusId |> string |> this.Endpoints.ProfileComments)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddOptionalQueryString "commentid" (Option.map string validatedParameters.CommentId)
                    |> Client.AddOptionalQueryString "maxdepth" (Option.map string validatedParameters.MaxDepth)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let comments : Result<StatusComments.Response, ErrorClient> =
                    Result.bind (Json.deserializeEx<StatusComments.Response> S.jsonConfig >> Ok) json
                return comments
        } |> Job.toAsync

    member this.Countries 
            (parameters: Countries.Parameters) 
            : Async<Result<Countries.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.Countries
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let countries : Result<Countries.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Countries.Response> S.jsonConfig >> Ok) json
            return countries
        } |> Job.toAsync

    member this.Privacy 
            (parameters: Privacy.Parameters) 
            : Async<Result<Privacy.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.Privacy
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let privacy : Result<Privacy.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Privacy.Response> S.jsonConfig >> Ok) json
            return privacy
        } |> Job.toAsync

    member this.Submission 
            (parameters: Submission.Parameters) 
            : Async<Result<Submission.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.Submission
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let submission : Result<Submission.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Submission.Response> S.jsonConfig >> Ok) json
            return submission
        } |> Job.toAsync

    member this.Tos 
            (parameters: Tos.Parameters) 
            : Async<Result<Tos.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.Tos
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let tos : Result<Tos.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Tos.Response> S.jsonConfig >> Ok) json
            return tos
        } |> Job.toAsync

    member this.Deviation 
            (parameters: Deviation.Parameters) 
            : Async<Result<Deviation.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest (parameters.DeviationId |> string |> this.Endpoints.Deviation)
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let deviation : Result<Deviation.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Deviation.Response> S.jsonConfig >> Ok) json
            return deviation
        } |> Job.toAsync

    member this.Content 
            (parameters: Content.Parameters) 
            : Async<Result<Content.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest this.Endpoints.Content
            |> Client.AddQueryString "deviationid" (string parameters.DeviationId)
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let content : Result<Content.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Content.Response> S.jsonConfig >> Ok) json
            return content
        } |> Job.toAsync

    member this.Download 
            (parameters: Download.Parameters) 
            : Async<Result<Download.Response, ErrorClient>> =
        let request : TRequest = 
            this.CreateRequest (parameters.DeviationId |> string |> this.Endpoints.Download)
            |> Client.AddQueryString "mature_content" (string parameters.MatureContent)
        job {
            let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
            let download : Result<Download.Response, ErrorClient> = 
                Result.bind (Json.deserializeEx<Download.Response> S.jsonConfig >> Ok) json
            return download
        } |> Job.toAsync

    member this.EmbeddedContent
            (parameters: EmbeddedContent.Parameters)
            : Async<Result<EmbeddedContent.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.EmbeddedContent
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                    |> Client.AddQueryString "deviationid" (string parameters.DeviationId)
                    |> Client.AddOptionalQueryString "offset_deviationid" (Option.map string validatedParameters.OffsetDeviationId)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let embeddedContent : Result<EmbeddedContent.Response, ErrorClient> =
                    Result.bind (Json.deserializeEx<EmbeddedContent.Response> S.jsonConfig >> Ok) json
                return embeddedContent
        } |> Job.toAsync

    member this.Metadata 
            (parameters: Metadata.Parameters) 
            : Async<Result<Metadata.Response, ErrorClient>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return
                    errors 
                    |> Set.map ErrorValidation.OfValidator 
                    |> ErrorClient.ParametersValidation
                    |> Error
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.Metadata
                    |> Client.AddMultipleValueQueryString "deviationids" (Seq.map string validatedParameters.DeviationIds)
                    |> Client.AddOptionalQueryString "ext_submission" (Option.map string validatedParameters.ExtSubmission)
                    |> Client.AddOptionalQueryString "ext_camera" (Option.map string validatedParameters.ExtCamera)
                    |> Client.AddOptionalQueryString "ext_stats" (Option.map string validatedParameters.ExtStats)
                    |> Client.AddOptionalQueryString "ext_collection" (Option.map string validatedParameters.ExtCollection)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                let! (json : Result<string, ErrorClient>) = this.RunRequestJob request
                let metadata : Result<Metadata.Response, ErrorClient> =
                    Result.bind (Json.deserializeEx<Metadata.Response> S.jsonConfig >> Ok) json
                return metadata
        } |> Job.toAsync

    member this.WhoFaved (parameters: WhoFaved.Parameters) : Async<Result<WhoFaved.Response, Set<string>>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.WhoFaved
                    |> Client.AddQueryString "deviationid" (string parameters.DeviationId)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                let! json = this.RunRequestJob request
                let whoFaved =
                    Result.bind (Json.deserializeEx<WhoFaved.Response> S.jsonConfig >> Ok) json
                return whoFaved
        } |> Job.toAsync

    member this.GalleryFolderId 
            (parameters: GalleryFolderId.Parameters)
            : Async<Result<GalleryFolderId.Response, Set<string>>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest (
                        Option.map string parameters.FolderId 
                        |> Option.defaultValue String.Empty 
                        |> this.Endpoints.GalleryFolderId)
                    |> Client.AddOptionalQueryString "username" validatedParameters.Username
                    |> Client.AddOptionalQueryString "mode" 
                        (Option.map (fun m -> m.ToString()) validatedParameters.Mode)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                let! json = this.RunRequestJob request
                let folderId =
                    Result.bind (Json.deserializeEx<GalleryFolderId.Response> S.jsonConfig >> Ok) json
                return folderId
        } |> Job.toAsync

    member this.GalleryAll 
            (parameters: GalleryAll.Parameters)
            : Async<Result<GalleryAll.Response, Set<string>>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.GalleryAll
                    |> Client.AddOptionalQueryString "username" validatedParameters.Username
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                let! json = this.RunRequestJob request
                let all =
                    Result.bind (Json.deserializeEx<GalleryAll.Response> S.jsonConfig >> Ok) json
                return all
        } |> Job.toAsync

    member this.GalleryFolders 
            (parameters: GalleryFolders.Parameters)
            : Async<Result<GalleryFolders.Response, Set<string>>> =
        job {
            match parameters.Validate() with
            | Error errors ->
                return (errors |> Set.map (fun (error: T.Invalid) -> error.Message) |> Error)
            | Ok validatedParameters ->
                let request : TRequest =
                    this.CreateRequest this.Endpoints.GalleryFolders
                    |> Client.AddOptionalQueryString "username" validatedParameters.Username
                    |> Client.AddOptionalQueryString "calculate_size" (Option.map string validatedParameters.CalculateSize)
                    |> Client.AddOptionalQueryString "ext_preload" (Option.map string validatedParameters.ExtPreload)
                    |> Client.AddOptionalQueryString "offset" (Option.map string validatedParameters.Offset)
                    |> Client.AddOptionalQueryString "limit" (Option.map string validatedParameters.Limit)
                    |> Client.AddQueryString "mature_content" (string validatedParameters.MatureContent)
                let! json = this.RunRequestJob request
                let folders =
                    Result.bind (Json.deserializeEx<GalleryFolders.Response> S.jsonConfig >> Ok) json
                return folders
        } |> Job.toAsync








