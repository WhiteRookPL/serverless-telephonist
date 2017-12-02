using Telephonist.Utilities;

namespace Telephonist.Models
{
  public class EmptyRequest
  {
    public override string ToString()
    {
      return "EmptyRequest()";
    }
  }

  public class UserPhone
  {
    public string CountryCode { get; set; }
    public string PhoneNumber { get; set; }
  }

  public class User
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string TimeZone { get; set; }
  }
}
