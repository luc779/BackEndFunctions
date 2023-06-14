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

    [Fact]
    public async Task UserNotFound()
    {
      var user = new {
        UserKey = "RandomRandomRandomRandomRandomRandom"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "get");
      var response = await _getEmissionHistory.GetEmissionHistoryWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
    }
  }
}