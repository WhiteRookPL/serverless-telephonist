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
      this.authorization = String.Format(CultureInfo.InvariantCulture, "Token token={0}", apiToken);

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

      string path = String.Format(CultureInfo.InvariantCulture, "https://{0}.pagerduty.com/api/v1/schedules/{1}?{2}", this.subdomain, scheduleId, WebHelpers.ToQueryString(parameters));

      HttpResponseMessage response = await this.client.GetAsync(path);

      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "path: {0}", path));
      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "Status Code: {0}", response.StatusCode));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }

    public async Task<string> GetUserContactMethods(string userId)
    {
      string path = String.Format(CultureInfo.InvariantCulture, "https://{0}.pagerduty.com/api/v1/users/{1}/contact_methods", this.subdomain, userId);

      HttpResponseMessage response = await this.client.GetAsync(path);

      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "path: {0}", path));
      LambdaLogger.Log(String.Format(CultureInfo.InvariantCulture, "Status Code: {0}", response.StatusCode));

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadAsStringAsync();
      }

      return null;
    }
  }
}
