using System.Net;
using FirebaseAdmin.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MySqlConnector;
using NoCO2.Util;
using Company.Function;
using BackEndFucntions;

namespace NoCO2.Function
{
  public class GetEmissionHistory
  {
    static GetEmissionHistory()
    {
      FirebaseInitializer.Initialize();
    }

    private const double EMISSION_GOAL = 7.36;

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
        List<DailyEmission> emissionHistory = await GetDailyEmissionsForUserWithinOneYear(matchedUserID);
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

    private int GetUserIdIfUserKeyExistsInDB(string userKey) {
      MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

      using (connection)
      {
        connection.Open();

        try
        {
          using (MySqlCommand command = connection.CreateCommand())
          {
            string query = "SELECT ID, UserKey FROM Users";
            command.CommandText = query;
            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows) {
              while (reader.Read())
              {
                string hashedUserKeyInDB = reader.GetString(1);
                if (BCrypt.Net.BCrypt.Verify(userKey, hashedUserKeyInDB)) {
                  connection.Close();
                  return reader.GetInt32("ID");
                }
              }
            }
          }
          connection.Close();
          return -1;
        } catch (Exception) {
          connection.Close();
          return -1;
        }
      }
    }

    private async Task<List<DailyEmission>> GetDailyEmissionsForUserWithinOneYear(int userId)
    {
      DateTime currentDate = DateTime.UtcNow;
      DateTime oneYearAgo = currentDate.AddYears(-1);

      MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

      using (connection)
      {
        connection.Open();

        string query = "SELECT e.DateTime, e.TotalAmount, e.Goal " +
               "FROM DailyEmission AS e " +
               "JOIN Users AS u ON e.UserID = u.ID " +
               "WHERE u.ID = @userId AND e.DateTime >= '@oneYearAgo' AND e.DateTime <= '@currentDate'" +
               " ORDER BY e.DateTime ASC";


        using (MySqlCommand command = connection.CreateCommand())
        {
          command.CommandText = query;
          command.Parameters.AddWithValue("@userId", userId);
          command.Parameters.AddWithValue("@oneYearAgo", oneYearAgo.ToString("yyyy/MM/dd"));
          command.Parameters.AddWithValue("@currentDate", currentDate.ToString("yyyy/MM/dd"));

          using (MySqlDataReader reader = await command.ExecuteReaderAsync())
          {
            List<DailyEmission> emissions = new List<DailyEmission>();
            const string DATE_TIME_COL = "DateTime";
            const string TOTAL_AMOUNT_COL = "TotalAmount";
            const string GOAL_COL = "Goal";

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                // Store a list of DailyEmission
                DateTime dateTime = reader.GetDateTime(DATE_TIME_COL).Date;
                double total = reader.GetDouble(TOTAL_AMOUNT_COL);
                double goal = reader.GetDouble(GOAL_COL);

                emissions.Add(new DailyEmission { DateTime = dateTime, Total = total, Goal = goal });
              }

              // Fill in the missing days with null total and default goal
              const int ZERO_DATE_OFFSET = 0;
              List<DateTime> allDates = Enumerable.Range(ZERO_DATE_OFFSET, (currentDate - oneYearAgo).Days)
                  .Select(offset => oneYearAgo.AddDays(offset).Date)
                  .ToList();

              List<DateTime> existingDates = emissions.Select(e => e.DateTime).ToList();

              List<DateTime> missingDates = allDates.Except(existingDates).ToList();

              emissions.AddRange(missingDates.Select(date => new DailyEmission
              {
                DateTime = date,
                Total = null,
                Goal = EMISSION_GOAL
              }));
              connection.Close();
              return emissions.OrderBy(e => e.DateTime).ToList();
            }
            // Return empty history if there are none within an year
            connection.Close();
            return emissions;
          }
        }
      }
    }
  }
}