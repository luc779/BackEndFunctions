using System.Net;
using Xunit;
using CreateUserFunction;
using GetEmissionHistoryFunction;
using GetEmissionStatisticsFunction;
using GetUserActivitiesFunction;
using SubmitUserActivitiesFunction;
using TestUtilities;
using Newtonsoft.Json;

namespace NoCO2.Test
{
  public class TestPlan : IClassFixture<CreateUser>, IClassFixture<GetEmissionHistory>, IClassFixture<GetEmissionStatistics>, IClassFixture<GetUserActivities>, IClassFixture<SubmitUserActivities>
  {
    private readonly CreateUser _createUser;
    private readonly GetEmissionHistory _getEmissionHistory;
    private readonly GetEmissionStatistics _getEmissionStatistics;
    private readonly GetUserActivities _getUserActivities;
    private readonly SubmitUserActivities _submitUserActivities;

    public TestPlan(
      CreateUser createUser,
      GetEmissionHistory getEmissionHistory,
      GetEmissionStatistics getEmissionStatistics,
      GetUserActivities getUserActivities,
      SubmitUserActivities submitUserActivities)
    {
        _createUser = createUser;
        _getEmissionHistory = getEmissionHistory;
        _getEmissionStatistics = getEmissionStatistics;
        _getUserActivities = getUserActivities;
        _submitUserActivities = submitUserActivities;
    }

    /*
      Test Account:
      Email: frontendtest@example.com
      password: test12345
      Corresponded UserKey = OfqLCi98hTQyvHZvwu4mXMbayCW2
    */

    [Fact]
    public async Task StepOne_CreateUser()
    {
      // input test userKey
      var user = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _createUser.CreateUserWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("{\"reply\":\"Success\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task StepTwo_SubmitUserActivities()
    {
      List<dynamic> transportList = new();
      transportList.Add(new {Type = "Truck", Amount = "10"});

      List<dynamic> foodsList = new();
      foodsList.Add(new {Type = "Beef", Amount = "10"});

      List<dynamic> utlitiesList = new();
      utlitiesList.Add(new {Type = "Electricity", Amount = "12"});

      var bodyObject = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2",
        Transports = transportList,
        Foods = foodsList,
        Utilities = utlitiesList
      };
      string body = JsonConvert.SerializeObject(bodyObject);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _submitUserActivities.SubmitInformationAsync(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("{\"reply\":\"Success\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task StepThree_GetUserActivities()
    {
      // input test userKey
      var user = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _getUserActivities.GetActivities(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      // Verify that the response content has the expected format
      string content = await response.GetResponseBody();
      Assert.Matches(@"\{""reply"":""Success"",""Transports"":\[.*\],""Foods"":\[.*\],""Utilities"":\[.*\]}", content);
    }

    [Fact]
    public async Task StepFour_GetEmissionHistory()
    {
      // input test userKey
      var user = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2"
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

    [Fact]
    public async Task StepFive_GetUserEmissionStatistics()
    {
      var user = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "get");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      // Verify that the response content has the expected format
      string content = await response.GetResponseBody();
      Assert.Matches(@"\{\s*""reply"":\s*""Success"",\s*""Statistics"":\s*\[.*\]\s*}", content);

      dynamic responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
      var statisticsArray = responseObject.Statistics;
      var expectedLength = 2;
      Assert.Equal(expectedLength, statisticsArray.Count);
    }

    [Fact]
    public async Task StepSix_GetUserEmissionStatistics()
    {
      var user = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "get");
      var response = await _getEmissionStatistics.GetEmissionStatisticsWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      // Verify that the response content has the expected format
      string content = await response.GetResponseBody();
      Assert.Matches(@"\{\s*""reply"":\s*""Success"",\s*""Statistics"":\s*\[.*\]\s*}", content);

      dynamic responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
      var statisticsArray = responseObject.Statistics;
      var expectedLength = 3;
      Assert.Equal(expectedLength, statisticsArray.Count);
    }
  }
}