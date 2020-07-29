# DeviantArt client

Client to retrieve data from DeviantArt endpoints available with the Client Credentials Grant.

## Installation

It's not uploaded to Nuget yet, so download source code, install locally and use it from a demo application, like:

```
$ cd /tmp && git clone https://github.com/Idrek/FSharp.DeviantArt && cd FSharp.DeviantArt

$ dotnet pack src/DeviantArt.fsproj

$ mkdir -p /tmp/nuget

$ nuget add src/bin/Debug/FSharp.DeviantArt.0.0.1.nupkg -Source /tmp/nuget

$ cd /tmp && dotnet new console --language F# --name Demo && cd $_

$ dotnet add package FSharp.DeviantArt --source /tmp/nuget --version 0.0.1
```

## Usage

Now register your application with DeviantArt, in the [apps section](https://www.deviantart.com/developers/apps), so they can afford you a pair of keys (client-id and client-secret) to use in the demo.

Replace your `Program.fs` with:
```
module Program

module Category = DeviantArt.Types.Browse.Category

type Client = DeviantArt.Client
type FailureTokenResponse = DeviantArt.Types.FailureTokenResponse

[<EntryPoint>]
let main _ = 
    let clientId : int = <Your client-id>
    let clientSecret : string = <Your client-secret>
    async {
        let! (clientR : Result<Client, FailureTokenResponse>) = 
            Client.Create clientId clientSecret
        match clientR with
        | Error e -> printfn "Error: %A" e
        | Ok (client: Client) -> 
            let parameters : Category.Parameters =
                Category.Parameters.Initialize(categoryPath = "/")
            let! r = client.CategoryTree parameters
            printfn "Output: %A" r
    } |> Async.RunSynchronously
    0
```

Run it:
```
$ dotnet run

Output: Ok
  {Categories =
    [|{Catpath = "/digitalart";
       Title = "Digital Art";
       HasSubcategory = true;
       ParentCatpath = "/";}; {Catpath = "/traditional";
                                    Title = "Traditional Art";
                                    HasSubcategory = true;
                                    ParentCatpath = "/";};
      {Catpath = "/photography";
       Title = "Photography";
       HasSubcategory = true;
       ParentCatpath = "/";}; {Catpath = "/artisan";
                                    Title = "Artisan Crafts";
                                    HasSubcategory = true;
                                    ParentCatpath = "/";};
      {Catpath = "/literature";
       Title = "Literature";
       HasSubcategory = true;
       ParentCatpath = "/";}; {Catpath = "/film";
                                    Title = "Film & Animation";
                                    HasSubcategory = true;
                                    ParentCatpath = "/";};
...                                    
```

## Pending

- Unit tests.
- Integration tests.