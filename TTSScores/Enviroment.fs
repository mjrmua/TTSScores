﻿module Enviroment

#if FABLE
    let inline ofJson<'T> json = Fable.Core.JsInterop.ofJson<'T> json
    let readFileSync (encoding:string) (fileName:string)= Fable.Import.Node.fs.readFileSync(fileName,encoding)
    let readFile = readFileSync (string "utf8")
#else
open System
open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open Newtonsoft.Json.Converters

    type private OptionConverter() =
        inherit JsonConverter()

        override x.CanConvert(t) = 
            t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

        override x.WriteJson(writer, value, serializer) =
            let value = 
                if value = null then null
                else 
                    let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                    fields.[0]  
            serializer.Serialize(writer, value)

        override x.ReadJson(reader, t, existingValue, serializer) =        
            let innerType = t.GetGenericArguments().[0]
            let innerType = 
                if innerType.IsValueType then (typedefof<Nullable<_>>).MakeGenericType([|innerType|])
                else innerType        
            let value = serializer.Deserialize(reader, innerType)
            let cases = FSharpType.GetUnionCases(t)
            if value = null then FSharpValue.MakeUnion(cases.[0], [||])
            else FSharpValue.MakeUnion(cases.[1], [|value|])


    let ofJson<'T> json = Newtonsoft.Json.JsonConvert.DeserializeObject<'T>(json, new OptionConverter())
    let readFile fileName= System.IO.Path.Combine ("../", fileName )
                            |> System.IO.File.ReadAllText
#endif
