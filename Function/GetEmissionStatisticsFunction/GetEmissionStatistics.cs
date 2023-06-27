using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using FirebaseAdmin.Auth;
using MySqlConnector;
using NoCO2.Util;
using Company.Function;
using BackEndFunctions;
using GetEmissionStatisticUtil;

namespace NoCO2.Function
{
  public class GetEmissionStatistics
  {
    static GetEmissionStatistics()
    {
        FirebaseInitializer.Initialize();
    }

    [Function("GetEmissionStatistics")]
    public async Task<HttpResponseData> GetEmissionStatisticsWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-emission-statistics")] HttpRequestData req)
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

        StatisticsCalculator calculator = new();
        List<EmissionStatistic> statistics = calculator.GetUserEmissionStatistics(matchedUserID);
        // Format the list of emissions into an object for HttpResponseData
        var successResponseBodyObject = new {
            reply = "Success",
            Statistics = statistics
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