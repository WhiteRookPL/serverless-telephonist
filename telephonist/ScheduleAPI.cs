using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly:LambdaSerializer(typeof(JsonSerializer))]

namespace Telephonist
{
  public class UserContactMethod {
    public string type { get; set; }
    public string country_code { get; set; }
    public string phone_number { get; set; }
  }

  public class User {
    public string id { get; set; }
    public string name { get; set; }
    public string time_zone { get; set; }
    public List<UserContactMethod> contact_methods { get; set; }
  }

  public class Request {}

  public class Response {
    public string TimeZone { get; set; }
    public string PhoneNumber { get; set; }
  }

  public static class WebHelpers {
    public static string ToQueryString(Dictionary<string, string> source)
    {
      return String.Join("&", source.Select(kv => String.Format("{0}={1}", WebUtility.UrlEncode(kv.Key), WebUtility.UrlEncode(kv.Value))).ToList());
    }

    public static T ParseJSON<T>(string data) {
      byte[] byteArray = Encoding.ASCII.GetBytes(data);
      MemoryStream stream = new MemoryStream( byteArray );

      JsonSerializer parser = new JsonSerializer();
      return parser.Deserialize<T>(stream);
    }
  }

  public class PagerDutyClient {
    private string subdomain;
    private string apiToken;

    private Uri uri;

    private HttpClient client;

    public PagerDutyClient(string subdomain, string apiToken) {
      this.subdomain = subdomain;
      this.apiToken = apiToken;

      this.uri = new Uri(String.Format(CultureInfo.InvariantCulture, "https://{0}.pagerduty.com/api/v1", this.subdomain));

      this.client = new HttpClient();

      this.client.BaseAddress = this.uri;

      this.client.DefaultRequestHeaders.Accept.Clear();
      this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", this.apiToken);
    }

    public async Task<string> GetCurrentOnCallOperatorUserId(string scheduleId) {
      DateTime now = DateTime.Now;
      DateTime oneSecondLater = now.AddSeconds(1);

      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["since"] = now.ToString("s", CultureInfo.InvariantCulture);
      parameters["until"] = oneSecondLater.ToString("s", CultureInfo.InvariantCulture);

      string path = String.Format(CultureInfo.InvariantCulture, "/schedules/{0}?{1}", scheduleId, WebHelpers.ToQueryString(parameters));

      HttpResponseMessage response = await client.GetAsync(path);

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<string> GetUserDetails(string userId) {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["includes[]"] = "contact_methods";

      string path = String.Format(CultureInfo.InvariantCulture, "/users/{0}?{1}", userId, WebHelpers.ToQueryString(parameters));

      HttpResponseMessage response = await client.GetAsync(path);

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }
  }

  public class ScheduleAPI
  {
    public async Task<Response> GetDetailsForCurrentOnCallOperator(Request request, ILambdaContext context)
    {
      string subdomain = Environment.GetEnvironmentVariable("PAGER_DUTY_DOMAIN");
      string apiToken = Environment.GetEnvironmentVariable("PAGER_DUTY_API_KEY");

      string scheduleId = Environment.GetEnvironmentVariable("PAGER_DUTY_SCHEDULE_ID");

      PagerDutyClient service = new PagerDutyClient(subdomain, apiToken);

      User user = await GetCurrentOnCallOperatorUserId(subdomain, apiToken, scheduleId);

      if (user != null) {
        return await GetUserDetails(subdomain, apiToken, user.id);
      } else {
        return null;
      }
    }

    public async Task<User> GetCurrentOnCallOperatorUserId(string subdomain, string apiToken, string scheduleId)
    {
      PagerDutyClient client = new PagerDutyClient(subdomain, apiToken);
      string data = await client.GetCurrentOnCallOperatorUserId(scheduleId);

      if (data == null) {
        return null;
      }

      return WebHelpers.ParseJSON<User>(data);
    }

    public async Task<Response> GetUserDetails(string subdomain, string apiToken, string userId)
    {
      PagerDutyClient client = new PagerDutyClient(subdomain, apiToken);
      string data = await client.GetUserDetails(userId);

      if (data == null) {
        return null;
      }

      User user = WebHelpers.ParseJSON<User>(data);

      List<UserContactMethod> contacts = user.contact_methods.Where(method => method.type == "phone").ToList();

      if (contacts.Count != 0)
      {
        UserContactMethod contact = contacts.First();
        return new Response() {
          TimeZone = user.time_zone,
          PhoneNumber = String.Format("+{0}{1}", contact.country_code, contact.phone_number)
        };
      }

      return null;
    }
  }
}
