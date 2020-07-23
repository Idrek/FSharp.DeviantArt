module DeviantArt.Settings

open FSharp.Json

let jsonConfig = JsonConfig.create(jsonFieldNaming = Json.snakeCase)