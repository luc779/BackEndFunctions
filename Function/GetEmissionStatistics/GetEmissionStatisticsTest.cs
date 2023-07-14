using System.Net;
using Xunit;
using Newtonsoft.Json;
using GetEmissionStatisticsFunction;
using TestUtilities;

namespace FunctionsTest
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

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task GetUserEmissionStatistics()
    {
      var user = new {
        UserKey = "pGIWAl55j3XH4LFHbXgsdtoM46j2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      // Verify that the response content has the expected format
      string content = await response.GetResponseBody();
      Assert.Matches(@"\{\s*""reply"":\s*""Success"",\s*""Statistics"":\s*\[.*\]\s*}", content);
    }
  }
}