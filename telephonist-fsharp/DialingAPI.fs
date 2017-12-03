namespace Telephonist

open Amazon.Lambda.Core
open Amazon.Lambda.Serialization.Json

[<assembly:LambdaSerializer(typeof<JsonSerializer>)>]
do ()

type Request = unit
type Response = { Status : string; TwilioRawResult : string }

module DialingAPI =
  open System
  open System.Net
  open System.Net.Http
  open System.Net.Http.Headers
  open System.Text

  let httpPostAsync(url : string, sid : string, token : string, content : StringContent) =
    async {
      let httpClient = new System.Net.Http.HttpClient()

      let value = Convert.ToBase64String(Encoding.UTF8.GetBytes(sprintf "%s:%s" sid token))
      let authorizationHeader = new AuthenticationHeaderValue("Basic", value)

      httpClient.DefaultRequestHeaders.Authorization <- authorizationHeader
      httpClient.DefaultRequestHeaders.Authorization <- authorizationHeader

      let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask

      response.EnsureSuccessStatusCode() |> ignore

      let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
      return content
    }

  let testOnCallPhoneNumber(request : Request) =
    let accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID")
    let apiToken = Environment.GetEnvironmentVariable("TWILIO_API_TOKEN")
    let caller = Environment.GetEnvironmentVariable("CALLER")
    let onCallNumber = Environment.GetEnvironmentVariable("ON_CALL_NUMBER")
    let twimlUrl = Environment.GetEnvironmentVariable("TEST_SCENARIO_TWIML_URL")

    LambdaLogger.Log(sprintf "PARAM: accountSid=%s" accountSid)
    LambdaLogger.Log(sprintf "PARAM: apiToken=%s" apiToken)
    LambdaLogger.Log(sprintf "PARAM: caller=%s" caller)
    LambdaLogger.Log(sprintf "PARAM: onCallNumber=%s" onCallNumber)
    LambdaLogger.Log(sprintf "PARAM: twimlUrl=%s" twimlUrl)

    let url = sprintf "https://api.twilio.com/2010-04-01/Accounts/%s/Calls.json" accountSid

    let callerEncoded = WebUtility.UrlEncode(caller)
    let onCallNumberEncoded = WebUtility.UrlEncode(onCallNumber)
    let twimlUrlEncoded = WebUtility.UrlEncode(twimlUrl)

    let form = sprintf "To=%s&From=%s&Url=%s&Method=GET" onCallNumberEncoded callerEncoded twimlUrlEncoded
    let content = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded")

    let result = httpPostAsync(url, accountSid, apiToken, content) |> Async.RunSynchronously

    { Status = "On-call number called successfully."; TwilioRawResult = result }
