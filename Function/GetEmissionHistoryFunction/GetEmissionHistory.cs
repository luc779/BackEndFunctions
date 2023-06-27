using System.Net;
using FirebaseAdmin.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NoCO2.Util;
using BackEndFunctions;
using GetEmissionHistoryUtils;
using NoCO2.Function;

namespace GetEmissionHistoryFunction
{
  public class GetEmissionHistory
  {
    static GetEmissionHistory()
    {
      FirebaseInitializer.Initialize();
    }

    [Function("GetEmissionHistory")]
    public async Task<HttpResponseData> GetEmissionHistoryWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-emission-history")] HttpRequestData req)
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
        int matchedUserID = FindUser.UserFinder(userKey);

        if (matchedUserID == -1)
        {
          // There is no user that has a matching hashed userkey from input userkey
          responseBodyObject = new {
            reply = "UserNotFound"
          };
          return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
        }

        // Format the list of emissions into an object for HttpResponseData
        List<DailyEmission> emissionHistory = await UserOneYearDailyEmissions.GetEmissions(matchedUserID);
        var successResponseBodyObject = new {
            reply = "Success",
            History = emissionHistory
        };
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.OK, successResponseBodyObject);
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