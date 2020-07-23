namespace DeviantArt

open Hopac

// ---------------------------------
// Module aliases
// ---------------------------------

module C = HttpFs.Client
module MResponse = C.Response

// ---------------------------------
// Type aliases
// ---------------------------------

// ---------------------------------
// Types
// ---------------------------------

type Dependencies = {
    TryGetResponse: TRequest -> Alt<Choice<TResponse,exn>>
    ReadBodyAsString: TResponse -> Job<string>
    GetResponse: TRequest -> Alt<TResponse>
    CreationTime: DateTimeOffset
    TokenUrl: string
} 


