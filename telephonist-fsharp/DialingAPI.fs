namespace Telephonist

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.Json

[<assembly:LambdaSerializer(typeof<JsonSerializer>)>]
do ()

type Request = unit
type Response = { Message : string }

module DialingAPI =
  open System
  open System.IO
  open System.Text

  let testOnCallPhoneNumber(request : Request) =
    { Message = "Tested!" }
