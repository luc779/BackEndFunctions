using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using FirebaseAdmin.Auth;
using MySqlConnector;
using NoCO2.Util;
using Company.Function;
using Newtonsoft.Json;

namespace NoCO2.Function
{
  public class GetEmissionHistory
  {
    private const double EMISSION_GOAL = 7.36;

    [Function("GetEmissionHistory")]
    public async Task<HttpResponseData> GetEmissionHistoryWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-emission-history")] HttpRequestData req)
    {
      try
      {
        req.Body.TryParseJson<GeneralUserKeyBody>(out var requestBody);

        // Get "UserKey" parameter from HTTP request as either parameter or post value
        string userKey = requestBody?.UserKey;
        string matchedUserID = CheckIfUserKeyExistsInDB(userKey);

        if (matchedUserID == null)
        {
          // There is no user that has a matching hashed userkey from input userkey
          var responseBodyObject = new {
            reply = "UserNotFound"
          };
          return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
        }

        List<DailyEmission> emissionHistory = await GetDailyEmissionsForUserWithinOneYear(matchedUserID);
      } catch (Exception) {

      }
      throw new NotImplementedException();
    }

    private string CheckIfUserKeyExistsInDB(string userKey) {
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
                  return reader.GetString(0);
                }
              }
            }
          }
          return null;
        } catch (Exception) {
          return null;
        }
      }
    }

    private async Task<List<DailyEmission>> GetDailyEmissionsForUserWithinOneYear(string userId)
    {
      DateTime currentDate = DateTime.UtcNow;
      DateTime oneYearAgo = currentDate.AddYears(-1);

      MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

      using (connection)
      {
        connection.Open();

        string query = "SELECT e.DateTime, e.TotalAmount, e.Goal " +
               "FROM DailyEmission AS e " +
               "JOIN Users AS u ON e.UserId = u.UserId " +
               "WHERE u.UserId = @userId AND e.DateTime >= @oneYearAgo AND e.DateTime <= @currentDate " +
               "ORDER BY e.DateTime ASC";

        using (MySqlCommand command = connection.CreateCommand())
        {
          command.CommandText = query;
          command.Parameters.AddWithValue("@userId", userId);
          command.Parameters.AddWithValue("@oneYearAgo", oneYearAgo);
          command.Parameters.AddWithValue("@currentDate", currentDate);

          using (MySqlDataReader reader = await command.ExecuteReaderAsync())
          {
            List<DailyEmission> emissions = new List<DailyEmission>();

            while (reader.Read())
            {
              DateTime dateTime = reader.GetDateTime("DateTime").Date;
              double total = reader.GetDouble("TotalAmount");
              double goal = reader.GetDouble("Goal");

              emissions.Add(new DailyEmission { DateTime = dateTime, Total = total, Goal = goal });
            }

            // Fill in the missing days with null total and default goal
            List<DateTime> allDates = Enumerable.Range(0, (currentDate - oneYearAgo).Days)
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

            return emissions.OrderBy(e => e.DateTime).ToList();
          }
        }
      }
    }
  }
}