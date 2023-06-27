using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using FirebaseAdmin.Auth;
using NoCO2.Util;
using CreateUserUtils;

namespace NoCO2.Function
{
  public class CreateUser
  {
    static CreateUser()
    {
        FirebaseInitializer.Initialize();
    }

    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUserWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create-user")] HttpRequestData req)
    {
      var responseBodyObject = new {
        reply = "InternalError"
      };
      try
      {
        req.Body.TryParseJson<GeneralUserKeyBody>(out var requestBody);

        // Get "UserKey" parameter from HTTP request as either parameter or post value
        string userKey = requestBody?.UserKey;

        UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);

        // Hash the userKey
        string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey);

        // Check if the database has a user with the same hashedUserKey
        bool isUserAdded = AddUser.Add(userKey, hashedUserKey);
        if (isUserAdded) {
          responseBodyObject = new {
            reply = "Success"
          };
          return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.OK, responseBodyObject);
        }

        // For some reason, the userkey is not added to the database
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, responseBodyObject);
      } catch (ArgumentException) {
        responseBodyObject = new {
          reply = "InvalidArgument"
        };
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
      } catch (FirebaseAuthException) {
        responseBodyObject = new {
          reply = "UserKeyNotAuth"
        };
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
      } catch (Exception) {
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, responseBodyObject);
      }
      throw new NotImplementedException();
    }
  }
}