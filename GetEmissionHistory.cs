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
    [Function("GetEmissionHistory")]
    public async Task<HttpResponseData> GetEmissionHistoryWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-emission-history")] HttpRequestData req)
    {
      try
      {
        req.Body.TryParseJson<GeneralUserKeyBody>(out var requestBody);

        // Get "UserKey" parameter from HTTP request as either parameter or post value
        string userKey = requestBody?.UserKey;
        bool isMatched = CheckIfUserKeyExistsInDB(userKey);

        if (!isMatched)
        {
          var responseBodyObject = new {
            reply = "UserNotFound"
          };
          return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, responseBodyObject);
        }
      } catch (Exception) {

      }
      throw new NotImplementedException();
    }

    private bool CheckIfUserKeyExistsInDB(string userKey) {
      MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

      using (connection)
      {
        connection.Open();

        try
        {
          using (MySqlCommand command = connection.CreateCommand())
          {
            string query = "SELECT USERKEY FROM Users";
            command.CommandText = query;
            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows) {
              while (reader.Read())
              {
                string hashedUserKeyInDB = reader.GetString(0);
                if (BCrypt.Net.BCrypt.Verify(userKey, hashedUserKeyInDB)) {
                  return true;
                }
              }
            }
          }
          return false;
        } catch (Exception) {
          return false;
        }
      }
    }
  }
}