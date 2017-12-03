using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;

using Telephonist.Models;
using Telephonist.Utilities;

namespace Telephonist
{
  public class TwilioIncomingCall
  {
    public override string ToString()
    {
      return "TwilioIncomingCall()";
    }
  }

  public class TwilioHandler
  {
    private static readonly string ResponseTwiML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response><Say voice=\"alice\" language=\"en-GB\">The current on-call engineer is {0}. The current time in their timezone is {1}. Please hold while we connect you.</Say><Dial>{2}</Dial></Response>";

    public async Task<string> HandleIncomingCall(TwilioIncomingCall twilioCallDetails, ILambdaContext context)
    {
      string stage = System.Environment.GetEnvironmentVariable("STAGE");
      string region = System.Environment.GetEnvironmentVariable("AWS_REGION");

      string functionName = $"telephonist-csharp-{stage}-GetDetailsForCurrentOnCallOperator";

      LambdaLogger.Log($"REQUEST: {twilioCallDetails}");

      LambdaLogger.Log($"PARAM: stage={stage}");
      LambdaLogger.Log($"PARAM: region={region}");

      LambdaLogger.Log($"functionName: {functionName}");

      using(AmazonLambdaClient client = new AmazonLambdaClient(RegionEndpoint.GetBySystemName(region)))
      {
        InvokeRequest request = new InvokeRequest() { FunctionName = functionName };
        InvokeResponse response = await client.InvokeAsync(request);

        using(StreamReader reader = new StreamReader(response.Payload))
        {
          string result = reader.ReadToEnd();

          LambdaLogger.Log($"Lambda result: {result}");

          OnCallOperatorDetails engineer = WebHelpers.ParseJSON<OnCallOperatorDetails>(result);

          string justName = engineer.Name.Split(' ')[0];

          TimeZoneInfo localTimeZone = WebHelpers.OlsonTimeZoneToTimeZoneInfo(engineer.TimeZone);
          DateTime localTime = TimeZoneInfo.ConvertTime(DateTime.Now, localTimeZone);
          string localTimeString = localTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);

          return String.Format(CultureInfo.InvariantCulture, ResponseTwiML, justName, localTimeString, engineer.PhoneNumber);
        }
      }
    }
  }
}
