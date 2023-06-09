using System.Net;
using Xunit;
using NoCO2.Function;
using NoCO2.Test.Util;
using NoCO2.Util;
using Newtonsoft.Json;

namespace NoCO2.Test
{
  public class CreateUserTest : IClassFixture<CreateUser>
  {
    private readonly CreateUser _createUser;
    public CreateUserTest(CreateUser createUser)
    {
        _createUser = createUser;
    }

    [Fact]
    public async Task FirebaseNotAuthorized()
    {
      var user = new CreateUserBody {
        UserKey = "RandomRandomRandomRandomRandomRandom"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _createUser.CreateUserWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("UserKeyNotAuth", await response.GetResponseBody());
    }

    [Fact]
    public async Task UserNotExistsInDatabase() {
      // input test userKey
      var user = new CreateUserBody {
        UserKey = "pGIWAl55j3XH4LFHbXgsdtoM46j2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _createUser.CreateUserWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("Success", await response.GetResponseBody());
    }
  }
}