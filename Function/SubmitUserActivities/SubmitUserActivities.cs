using System.Net;
using BackEndFunctions;
using DatabaseConnector;
using FirebaseAdmin.Auth;
using HttpRequestDataExtensions;
using HttpRequestDataFactory;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using SubmitUserActivitiesUtil;
using UserFinder;

namespace SubmitUserActivitiesFunction
{
    public class SubmitUserActivities
    {
        static SubmitUserActivities()
        {
            FirebaseInitializer.Initialize();
        }

        [Function("SubmitUserActivities")]
        public async Task<HttpResponseData> SubmitInformationAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit-user-activity")] HttpRequestData req)
        {
            var responseBodyObject = new {
                reply = "InternalError"
            };
            try
            {
                req.Body.TryParseJson<SubmitUserActivitiesBody>(out var requestBody);

                // Get "input" parameter from HTTP request as either parameter or post value
                string userKey = requestBody?.UserKey;

                List<dynamic> transports = requestBody?.Transports;
                List<dynamic> foods = requestBody?.Foods;
                List<dynamic> utilities = requestBody?.Utilities;

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);
                // See the UserRecord reference doc for the contents of userRecord.

                try
                {
                    // find userKey and return the ID
                    int ID = FindUser.UserFinder(userKey);
                    if (ID == -1) {
                        responseBodyObject = new {
                            reply = "UserNotFound"
                        };
                        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
                    }

                    // Check if the database has a user with the same userKey
                    bool isActivitiesAdded = ActivitySubmit.Submit(ID, transports, foods, utilities);
                    if (isActivitiesAdded) {
                        responseBodyObject = new {
                            reply = "Success"
                        };
                        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.OK, responseBodyObject);
                    } else {
                        responseBodyObject = new {
                            reply = "SubmitActivitiesError"
                        };
                        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, responseBodyObject);
                    }
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