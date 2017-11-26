using Amazon.Lambda.Core;
using System;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Telephonist
{
  public class TwilioHandler
  {
     public Response HandleIncomingCall(Request request)
     {
       return new Response("Handling incoming call from Twilio.", request);
     }
  }

  public class Response
  {
    public string Message {get; set;}
    public Request Request {get; set;}

    public Response(string message, Request request){
      Message = message;
      Request = request;
    }
  }

  public class Request
  {
    public string Key1 {get; set;}
    public string Key2 {get; set;}
    public string Key3 {get; set;}

    public Request(string key1, string key2, string key3){
      Key1 = key1;
      Key2 = key2;
      Key3 = key3;
    }
  }
}
