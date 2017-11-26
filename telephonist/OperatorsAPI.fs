namespace Telephonist

open Amazon.Lambda.Core

[<assembly:LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]

do ()

module Models =
  type Request = { Key1 : string; Key2 : string; Key3 : string }
  type Response = { Message : string; Request : Request }

module OperatorsAPI =
  open System
  open System.IO
  open System.Text
  open Models

  let getDetailsOfCurrentOnCallPerson(request:Request): Response =
    { Message = "Getting current on-call person details."
      Request = request }
