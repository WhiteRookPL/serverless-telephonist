using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

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

  public class IncomingCallHandlerStatus
  {
    public override string ToString()
    {
      return "IncomingCallHandlerStatus()";
    }
  }

  public class TwilioHandler
  {
    public async Task<IncomingCallHandlerStatus> HandleIncomingCall(TwilioIncomingCall request, ILambdaContext context)
    {
      return new IncomingCallHandlerStatus();
    }
  }
}
