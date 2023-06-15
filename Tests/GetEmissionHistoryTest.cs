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
    public async Task EmptyBody()
    {
      var user = new {};
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _getEmissionHistory.GetEmissionHistoryWithUserKey(request);

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
      var response = await _getEmissionHistory.GetEmissionHistoryWithUserKey(request);

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
      var response = await _getEmissionHistory.GetEmissionHistoryWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task GetOneYearOfEmissionHistory()
    {
      var user = new {
        UserKey = "pGIWAl55j3XH4LFHbXgsdtoM46j2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "get");
      var response = await _getEmissionHistory.GetEmissionHistoryWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      // Verify that the response content has the expected format
      string content = await response.GetResponseBody();
      Assert.Matches(@"\{\s*""reply"":\s*""Success"",\s*""History"":\s*\[.*\]\s*}", content);

      // Verify the length of the History array
      dynamic responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
      var historyArray = responseObject.History;
      var expectedLength = (DateTime.Now - DateTime.Now.AddYears(-1)).TotalDays;
      Assert.Equal(expectedLength, historyArray.Count);
    }
  }
}