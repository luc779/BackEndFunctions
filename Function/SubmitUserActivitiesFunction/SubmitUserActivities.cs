using System.Net;
using BackEndFunctions;
using Company.Function;
using FirebaseAdmin.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MySqlConnector;
using NoCO2.Util;
using SubmitUserActivitiesUtil;

namespace Company
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
                    bool isActivitiesAdded = SubmitActivities(ID, transports, foods, utilities);
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
        private static bool SubmitActivities(int userID, List<dynamic> transports, List<dynamic> foods, List<dynamic> utilities) {
            // check if the new parameters are valid numbers
            bool validatedNumbers = ValidateActivitiesInput.ValidateNumbers(transports, foods, utilities);
            if (!validatedNumbers) {
                throw new Exception();
            }

            // create connection
            MySqlConnection connection = DatabaseConnecter.MySQLDatabase();
            using(connection)
            {
                connection.Open();
                using MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    DateTime date = DateTime.UtcNow.Date;

                    // delete previous entries
                    DeleteUserActivities.Delete(connection, userID, date);

                    // all are null no need to update database
                    if (transports.Count == 0 && foods.Count == 0 && utilities.Count == 0) {
                        return true;
                    }

                    // input new entries using same query
                    const string QUERY = "INSERT INTO Activities (UserID, ActivityType, Method, Amount, DateTime, Emission) Values (@UserID, @ActivityType, @Method, @Amount, @DateTime, @Emission)";
                    const string transport = "transport";
                    const string food = "food";
                    const string utility = "utility";

                    // input transport
                    InputActivity.Input(userID, transports, connection, date, QUERY, transport);
                    // input food
                    InputActivity.Input(userID, transports, connection, date, QUERY, food);
                    // input utility
                    InputActivity.Input(userID, transports, connection, date, QUERY, utility);

                    // update Daily emissions
                    DailyEmissionsUpdate.Update(userID, connection, date);

                    // commit and close
                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    connection.Close();
                    return false;
                }
            }
            return true;
        }
    }
}