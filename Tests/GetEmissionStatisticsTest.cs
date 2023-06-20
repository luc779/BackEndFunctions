using System.Net;
using Xunit;
using NoCO2.Function;
using NoCO2.Test.Util;
using Newtonsoft.Json;

namespace NoCO2.Test
{
  public class GetEmissionStatisticsTest : IClassFixture<GetEmissionStatistics>
  {
    private readonly GetEmissionStatistics _getEmissionStatistics;
    public GetEmissionStatisticsTest(GetEmissionStatistics getEmissionStatistics)
    {
        _getEmissionStatistics = getEmissionStatistics;
    }

    [Fact]
    public async Task EmptyBody()
    {
      var user = new {};
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"InvalidArgument\"}", await response.GetResponseBody());
    }
  }
}