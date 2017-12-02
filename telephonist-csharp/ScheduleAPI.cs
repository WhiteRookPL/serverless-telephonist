using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

using Telephonist.Models;
using Telephonist.Utilities;

[assembly:LambdaSerializer(typeof(JsonSerializer))]

namespace Telephonist
{
  public class Request
  {
    public override string ToString()
    {
      return "Request()";
    }
  }

  public class Response
  {
    public string Name { get; set; }
    public string TimeZone { get; set; }
    public string PhoneNumber { get; set; }

    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "Response(Name={0}, TZ={1}, Phone={2}", Name, TimeZone, PhoneNumber);
    }
  }

  public class ScheduleAPI
  {
    public async Task<Response> GetDetailsForCurrentOnCallOperator(Request request, ILambdaContext context)
    {
      string subdomain = Environment.GetEnvironmentVariable("PAGER_DUTY_DOMAIN");
      string apiToken = Environment.GetEnvironmentVariable("PAGER_DUTY_API_KEY");

      string scheduleId = Environment.GetEnvironmentVariable("PAGER_DUTY_SCHEDULE_ID");

      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "REQUEST: {0}", request));

      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "PARAM: subdomain={0}", subdomain));
      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "PARAM: apiToken={0}", apiToken));
      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "PARAM: scheduleId={0}", scheduleId));

      PagerDutyClient service = new PagerDutyClient(subdomain, apiToken);

      User user = await GetCurrentOnCallOperator(subdomain, apiToken, scheduleId);

      if (user != null)
      {
        UserPhone phone = await GetUserContactMethods(subdomain, apiToken, user.Id);

        Response result = new Response()
        {
          Name = user.Name,
          TimeZone = WebHelpers.MapToTimeZoneCompliantWithISO(user.TimeZone),
          PhoneNumber = String.Format("+{0}{1}", phone.CountryCode, phone.PhoneNumber)
        };

        LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "RESPONSE: {0}", result));

        return result;
      } else {
        return null;
      }
    }

    private async Task<User> GetCurrentOnCallOperator(string subdomain, string apiToken, string scheduleId)
    {
      PagerDutyClient client = new PagerDutyClient(subdomain, apiToken);
      string data = await client.GetCurrentOnCallOperator(scheduleId);

      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "GetCurrentOnCallOperator: {0}", data));

      if (data == null)
      {
        return null;
      }

      dynamic response = WebHelpers.ParseJSON(data);
      dynamic user = response.schedule.final_schedule.rendered_schedule_entries[0].user;

      return new User()
      {
        Id = user.id,
        Name = user.name,
        TimeZone = response.schedule.time_zone
      };
    }

    private async Task<UserPhone> GetUserContactMethods(string subdomain, string apiToken, string userId)
    {
      PagerDutyClient client = new PagerDutyClient(subdomain, apiToken);
      string data = await client.GetUserContactMethods(userId);

      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "GetUserContactMethods: {0}", data));

      if (data == null)
      {
        return null;
      }

      dynamic response = WebHelpers.ParseJSON(data);

      var contacts = ((IEnumerable<dynamic>) response.contact_methods).Where(method => method.type == "phone");
      List<UserPhone> phones = contacts.Select(method => new UserPhone() { CountryCode = method.country_code, PhoneNumber = method.phone_number }).ToList();

      if (phones.Count != 0)
      {
        return phones.First();
      }

      return null;
    }
  }
}
