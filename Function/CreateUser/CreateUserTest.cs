using System.Net;
using Xunit;
using Newtonsoft.Json;
using CreateUserFunction;
using TestUtilities;

namespace FunctionsTest
{
  public class CreateUserTest : IClassFixture<CreateUser>
  {
    private readonly CreateUser _createUser;
    public CreateUserTest(CreateUser createUser)
    {
        _createUser = createUser;
    }

    [Fact]
    public void CorrectHashUserKey()
    {
      string userKey = "RandomRandomRandomRandomRandom";
      string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey);

      bool isMatched = BCrypt.Net.BCrypt.Verify(userKey, hashedUserKey);

      Assert.True(isMatched);
    }

    [Fact]
    public void IncorrectHashUserKey()
    {
      string userKey1 = "RandomRandomRandomRandomRandom";
      string userKey2 = "IncorrectIncorrectIncorrectIncorrect";
      string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey1);

      bool isMatched = BCrypt.Net.BCrypt.Verify(userKey2, hashedUserKey);

      Assert.False(isMatched);
    }

    [Fact]
    public async Task EmptyBody()
    {
      var user = new {};
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _createUser.CreateUserWithUserKey(request);

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
      var response = await _createUser.CreateUserWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"InvalidArgument\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task FirebaseNotAuthorized()
    {
      var user = new {
        UserKey = "RandomRandomRandomRandomRandomRandom"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _createUser.CreateUserWithUserKey(request);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
    }

    [Fact]
    public async Task UserNotExistsInDatabase()
    {
      // input test userKey
      var user = new {
        UserKey = "pGIWAl55j3XH4LFHbXgsdtoM46j2"
      };
      string body = JsonConvert.SerializeObject(user);

      var request = TestFactory.CreateHttpRequest(body, "post");
      var response = await _createUser.CreateUserWithUserKey(request);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("{\"reply\":\"Success\"}", await response.GetResponseBody());
    }
  }
}