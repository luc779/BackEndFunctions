using System.Net;
using Xunit;
using NoCO2.Function;
using NoCO2.Test.Util;
using Newtonsoft.Json;

namespace NoCO2.Test
{
  public class GetEmissionHistoryTest : IClassFixture<GetEmissionHistory>
  {
    private readonly GetEmissionHistory _getEmissionHistory;
    public GetEmissionHistoryTest(GetEmissionHistory getEmissionHistory)
    {
      _getEmissionHistory = getEmissionHistory;
    }
  }
}