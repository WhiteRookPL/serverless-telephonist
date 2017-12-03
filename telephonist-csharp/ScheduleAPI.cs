using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Telephonist.Models;
using Telephonist.Utilities;

namespace Telephonist
{
  public class OnCallOperatorDetails
  {
    public string Name { get; set; }
    public string TimeZone { get; set; }
    public string PhoneNumber { get; set; }

    public override string ToString()
    {
      return $"OnCallOperatorDetails(Name={Name}, TZ={TimeZone}, Phone={PhoneNumber}";
    }
  }

  public class ScheduleAPI
  {
    public async Task<OnCallOperatorDetails> GetDetailsForCurrentOnCallOperator(EmptyRequest request, ILambdaContext context)
    {
      string subdomain = Environment.GetEnvironmentVariable("PAGER_DUTY_DOMAIN");
      string apiToken = Environment.GetEnvironmentVariable("PAGER_DUTY_API_KEY");

      string scheduleId = Environment.GetEnvironmentVariable("PAGER_DUTY_SCHEDULE_ID");

      LambdaLogger.Log($"REQUEST: {request}");

      LambdaLogger.Log($"PARAM: subdomain={subdomain}");
      LambdaLogger.Log($"PARAM: apiToken={apiToken}");
      LambdaLogger.Log($"PARAM: scheduleId={scheduleId}");

      PagerDutyClient service = new PagerDutyClient(subdomain, apiToken);

      User user = await GetCurrentOnCallOperator(subdomain, apiToken, scheduleId);

      if (user != null)
      {
        UserPhone phone = await GetUserContactMethods(subdomain, apiToken, user.Id);

        OnCallOperatorDetails result = new OnCallOperatorDetails()
        {
          Name = user.Name,
          TimeZone = WebHelpers.MapToOlsonTimeZone(user.TimeZone),
          PhoneNumber = $"+{phone.CountryCode}{phone.PhoneNumber}"
        };

        LambdaLogger.Log($"RESPONSE: {result}");

        return result;
      } else {
        return null;
      }
    }

    private async Task<User> GetCurrentOnCallOperator(string subdomain, string apiToken, string scheduleId)
    {
      PagerDutyClient client = new PagerDutyClient(subdomain, apiToken);
      string data = await client.GetCurrentOnCallOperator(scheduleId);

      LambdaLogger.Log($"GetCurrentOnCallOperator: {data}");

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

      LambdaLogger.Log($"GetUserContactMethods: {data}");

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
