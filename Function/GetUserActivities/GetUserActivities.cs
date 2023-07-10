using System.Net;
using FirebaseAdmin.Auth;
using GetUserActivitiesUtil;
using HttpRequestDataExtensions;
using HttpRequestDataFactory;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using UserFinder;
using UserKeyBody;

namespace GetUserActivitiesFunction
{
    public class GetUserActivities
    {
        public GetUserActivities()
        {
            FirebaseInitializer.Initialize();
        }

        [Function("GetUserActivities")]
        public async Task<HttpResponseData> GetActivities([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "get-user-activities")] HttpRequestData req)
        {
            var responseBodyObject = new {
                reply = "InternalError"
            };
            try
            {
                req.Body.TryParseJson<GeneralUserKeyBody>(out var requestBody);

                // Get "input" parameter from HTTP request as either parameter or post value
                string userKey = requestBody?.UserKey;

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);
                // See the UserRecord reference doc for the contents of userRecord.

                try
                {
                    // find userKey and return the ID
                    int matchedUserID = FindUser.UserFinder(userKey);
                    if (matchedUserID == -1) {
                        responseBodyObject = new {
                            reply = "UserNotFound"
                        };
                        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
                    }

                    // retrieve all activities
                    ReturnedInfo information = AllActivityRetrieval.Retrieve(matchedUserID);
                    var successresponseBodyObject = new {
                        reply = "Success",
                        Transports = information.Transports,
                        Foods = information.Foods,
                        Utilities = information.Utilities
                    };
                    return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.OK, successresponseBodyObject);
                }
                catch (Exception) {
                    return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, responseBodyObject);
                }
            }
            catch (ArgumentException) {
                responseBodyObject = new {
                    reply = "InvalidArgument"
                };
                return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
            }
            catch (FirebaseAuthException) {
                responseBodyObject = new {
                    reply = "UserKeyNotAuth"
                };
                return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
            }
            catch (Exception) {
                return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, responseBodyObject);
            }
            throw new NotImplementedException();
        }
    }
}
