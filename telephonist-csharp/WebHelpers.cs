using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

using Newtonsoft.Json.Linq;

namespace Telephonist.Utilities
{
  public static class WebHelpers
  {
    public static string ToQueryString(Dictionary<string, string> source)
    {
      return String.Join("&", source.Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}").ToList());
    }

    public static dynamic ParseJSON(string data)
    {
      return JValue.Parse(data);
    }

    public static T ParseJSON<T>(string data)
    {
      using(MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
      {
        JsonSerializer parser = new JsonSerializer();
        return parser.Deserialize<T>(stream);
      }
    }

    public static string MapToOlsonTimeZone(string pagerDutyTZ)
    {
      return PagerDutyToOlsonTimeZones[pagerDutyTZ];
    }

    public static TimeZoneInfo OlsonTimeZoneToTimeZoneInfo(string olsonTimeZoneId)
    {
      TimeZoneInfo timeZoneInfo = default(TimeZoneInfo);

      try { timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(olsonTimeZoneId); }
      catch (Exception) {}

      return timeZoneInfo;
    }

    private static readonly Dictionary<string, string> PagerDutyToOlsonTimeZones = new Dictionary<string, string>()
    {
      { "International Date Line West", "Pacific/Midway" },
      { "Midway Island", "Pacific/Midway" },
      { "American Samoa", "Pacific/Pago_Pago" },
      { "Hawaii", "Pacific/Honolulu" },
      { "Alaska", "America/Juneau" },
      { "Pacific Time (US & Canada)", "America/Los_Angeles" },
      { "Tijuana", "America/Tijuana" },
      { "Mountain Time (US & Canada)", "America/Denver" },
      { "Arizona", "America/Phoenix" },
      { "Chihuahua", "America/Chihuahua" },
      { "Mazatlan", "America/Mazatlan" },
      { "Central Time (US & Canada)", "America/Chicago" },
      { "Saskatchewan", "America/Regina" },
      { "Guadalajara", "America/Mexico_City" },
      { "Mexico City", "America/Mexico_City" },
      { "Monterrey", "America/Monterrey" },
      { "Central America", "America/Guatemala" },
      { "Eastern Time (US & Canada)", "America/New_York" },
      { "Indiana (East)", "America/Indiana/Indianapolis" },
      { "Bogota", "America/Bogota" },
      { "Lima", "America/Lima" },
      { "Quito", "America/Lima" },
      { "Atlantic Time (Canada)", "America/Halifax" },
      { "Caracas", "America/Caracas" },
      { "La Paz", "America/La_Paz" },
      { "Santiago", "America/Santiago" },
      { "Newfoundland", "America/St_Johns" },
      { "Brasilia", "America/Sao_Paulo" },
      { "Buenos Aires", "America/Argentina/Buenos_Aires" },
      { "Montevideo", "America/Montevideo" },
      { "Georgetown", "America/Guyana" },
      { "Greenland", "America/Godthab" },
      { "Mid-Atlantic", "Atlantic/South_Georgia" },
      { "Azores", "Atlantic/Azores" },
      { "Cape Verde Is.", "Atlantic/Cape_Verde" },
      { "Dublin", "Europe/Dublin" },
      { "Edinburgh", "Europe/London" },
      { "Lisbon", "Europe/Lisbon" },
      { "London", "Europe/London" },
      { "Casablanca", "Africa/Casablanca" },
      { "Monrovia", "Africa/Monrovia" },
      { "UTC", "Etc/UTC" },
      { "Belgrade", "Europe/Belgrade" },
      { "Bratislava", "Europe/Bratislava" },
      { "Budapest", "Europe/Budapest" },
      { "Ljubljana", "Europe/Ljubljana" },
      { "Prague", "Europe/Prague" },
      { "Sarajevo", "Europe/Sarajevo" },
      { "Skopje", "Europe/Skopje" },
      { "Warsaw", "Europe/Warsaw" },
      { "Zagreb", "Europe/Zagreb" },
      { "Brussels", "Europe/Brussels" },
      { "Copenhagen", "Europe/Copenhagen" },
      { "Madrid", "Europe/Madrid" },
      { "Paris", "Europe/Paris" },
      { "Amsterdam", "Europe/Amsterdam" },
      { "Berlin", "Europe/Berlin" },
      { "Bern", "Europe/Berlin" },
      { "Rome", "Europe/Rome" },
      { "Stockholm", "Europe/Stockholm" },
      { "Vienna", "Europe/Vienna" },
      { "West Central Africa", "Africa/Algiers" },
      { "Bucharest", "Europe/Bucharest" },
      { "Cairo", "Africa/Cairo" },
      { "Helsinki", "Europe/Helsinki" },
      { "Kyiv", "Europe/Kiev" },
      { "Riga", "Europe/Riga" },
      { "Sofia", "Europe/Sofia" },
      { "Tallinn", "Europe/Tallinn" },
      { "Vilnius", "Europe/Vilnius" },
      { "Athens", "Europe/Athens" },
      { "Istanbul", "Europe/Istanbul" },
      { "Minsk", "Europe/Minsk" },
      { "Jerusalem", "Asia/Jerusalem" },
      { "Harare", "Africa/Harare" },
      { "Pretoria", "Africa/Johannesburg" },
      { "Kaliningrad", "Europe/Kaliningrad" },
      { "Moscow", "Europe/Moscow" },
      { "St. Petersburg", "Europe/Moscow" },
      { "Volgograd", "Europe/Volgograd" },
      { "Samara", "Europe/Samara" },
      { "Kuwait", "Asia/Kuwait" },
      { "Riyadh", "Asia/Riyadh" },
      { "Nairobi", "Africa/Nairobi" },
      { "Baghdad", "Asia/Baghdad" },
      { "Tehran", "Asia/Tehran" },
      { "Abu Dhabi", "Asia/Muscat" },
      { "Muscat", "Asia/Muscat" },
      { "Baku", "Asia/Baku" },
      { "Tbilisi", "Asia/Tbilisi" },
      { "Yerevan", "Asia/Yerevan" },
      { "Kabul", "Asia/Kabul" },
      { "Ekaterinburg", "Asia/Yekaterinburg" },
      { "Islamabad", "Asia/Karachi" },
      { "Karachi", "Asia/Karachi" },
      { "Tashkent", "Asia/Tashkent" },
      { "Chennai", "Asia/Kolkata" },
      { "Kolkata", "Asia/Kolkata" },
      { "Mumbai", "Asia/Kolkata" },
      { "New Delhi", "Asia/Kolkata" },
      { "Kathmandu", "Asia/Kathmandu" },
      { "Astana", "Asia/Dhaka" },
      { "Dhaka", "Asia/Dhaka" },
      { "Sri Jayawardenepura", "Asia/Colombo" },
      { "Almaty", "Asia/Almaty" },
      { "Novosibirsk", "Asia/Novosibirsk" },
      { "Rangoon", "Asia/Rangoon" },
      { "Bangkok", "Asia/Bangkok" },
      { "Hanoi", "Asia/Bangkok" },
      { "Jakarta", "Asia/Jakarta" },
      { "Krasnoyarsk", "Asia/Krasnoyarsk" },
      { "Beijing", "Asia/Shanghai" },
      { "Chongqing", "Asia/Chongqing" },
      { "Hong Kong", "Asia/Hong_Kong" },
      { "Urumqi", "Asia/Urumqi" },
      { "Kuala Lumpur", "Asia/Kuala_Lumpur" },
      { "Singapore", "Asia/Singapore" },
      { "Taipei", "Asia/Taipei" },
      { "Perth", "Australia/Perth" },
      { "Irkutsk", "Asia/Irkutsk" },
      { "Ulaanbaatar", "Asia/Ulaanbaatar" },
      { "Seoul", "Asia/Seoul" },
      { "Osaka", "Asia/Tokyo" },
      { "Sapporo", "Asia/Tokyo" },
      { "Tokyo", "Asia/Tokyo" },
      { "Yakutsk", "Asia/Yakutsk" },
      { "Darwin", "Australia/Darwin" },
      { "Adelaide", "Australia/Adelaide" },
      { "Canberra", "Australia/Melbourne" },
      { "Melbourne", "Australia/Melbourne" },
      { "Sydney", "Australia/Sydney" },
      { "Brisbane", "Australia/Brisbane" },
      { "Hobart", "Australia/Hobart" },
      { "Vladivostok", "Asia/Vladivostok" },
      { "Guam", "Pacific/Guam" },
      { "Port Moresby", "Pacific/Port_Moresby" },
      { "Magadan", "Asia/Magadan" },
      { "Srednekolymsk", "Asia/Srednekolymsk" },
      { "Solomon Is.", "Pacific/Guadalcanal" },
      { "New Caledonia", "Pacific/Noumea" },
      { "Fiji", "Pacific/Fiji" },
      { "Kamchatka", "Asia/Kamchatka" },
      { "Marshall Is.", "Pacific/Majuro" },
      { "Auckland", "Pacific/Auckland" },
      { "Wellington", "Pacific/Auckland" },
      { "Nuku'alofa", "Pacific/Tongatapu" },
      { "Tokelau Is.", "Pacific/Fakaofo" },
      { "Chatham Is.", "Pacific/Chatham" },
      { "Samoa", "Pacific/Apia" }
    };
  }
}
