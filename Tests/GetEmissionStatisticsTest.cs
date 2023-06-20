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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task EmptyBodyUserKey(string userKey)
    {
      var user = new {
        UserKey = userKey
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"InvalidArgument\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task UserKeyNotAuth()
    {
      var user = new {
        UserKey = "RandomRandomRandomRandomRandomRandom"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "get");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
    }
  }
}