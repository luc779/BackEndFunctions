using System.Net;
using Xunit;
using NoCO2.Function;
using NoCO2.Test.Util;
using Newtonsoft.Json;
using Company.Function;
using Company;

namespace NoCO2.Test
{
  public class TestPlan
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
    public async void StepOne_CreateUser()
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
    public async void StepTwo_SubmitUserActivities()
    {
      List<dynamic> transportList = new();
      transportList.Add(new {Type = "Truck", Amount = "10"});

      List<dynamic> foodsList = new();
      foodsList.Add(new {Type = "Beef", Amount = "10"});

      List<dynamic> utlitiesList = new();
      utlitiesList.Add(new {Type = "Electricity", Amount = "12"});

      var bodyObject = new {
        UserKey = "OfqLCi98hTQyvHZvwu4mXMbayCW2",
        Activities = new {
          Transports = transportList,
          Foods = foodsList,
          Utilities =utlitiesList
        }
      };
      string body = JsonConvert.SerializeObject(bodyObject);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _submitUserActivities.SubmitInformationAsync(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("{\"reply\":\"Success\"}", await response.GetResponseBody());
    }
  }
}