using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Telephonist.Utilities;

namespace Telephonist
{
  public class PagerDutyClient {
    private string subdomain;
    private string authorization;

    private HttpClient client;

    public PagerDutyClient(string subdomain, string apiToken)
    {
      this.subdomain = subdomain;
      this.authorization = $"Token token={apiToken}";

      this.client = new HttpClient();
      this.client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", this.authorization);
    }

    public async Task<string> GetCurrentOnCallOperator(string scheduleId)
    {
      DateTime now = DateTime.Now;
      DateTime oneSecondLater = now.AddSeconds(1);

      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["since"] = now.ToString("s", CultureInfo.InvariantCulture);
      parameters["until"] = oneSecondLater.ToString("s", CultureInfo.InvariantCulture);

      string queryString = WebHelpers.ToQueryString(parameters);
      string path = $"https://{this.subdomain}.pagerduty.com/api/v1/schedules/{scheduleId}?{queryString}";

      HttpResponseMessage response = await this.client.GetAsync(path);

      LambdaLogger.Log($"path: {path}");
      LambdaLogger.Log($"Status Code: {response.StatusCode}");

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<string> GetUserContactMethods(string userId)
    {
      string path = $"https://{this.subdomain}.pagerduty.com/api/v1/users/{userId}/contact_methods";

      HttpResponseMessage response = await this.client.GetAsync(path);

      LambdaLogger.Log($"path: {path}");
      LambdaLogger.Log($"Status Code: {response.StatusCode}");

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }
  }
}
