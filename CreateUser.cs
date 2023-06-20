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
        req.Body.TryParseJson<CreateUserBody>(out var requestBody);

        // Get "UserKey" parameter from HTTP request as either parameter or post value
        string userKey = requestBody?.UserKey;

        UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);

        // Hash the userKey
        string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey);

        // Check if the database has a user with the same hashedUserKey
        bool isUserAdded = AddUserToDatabase(userKey, hashedUserKey);
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

    // TODO: Move all Database related tasks into one class
    private static bool AddUserToDatabase(string originalUserKey, string hashedUserKey) {

      MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

      using (connection)
      {
        connection.Open();

        // Start the transaction
        using (MySqlTransaction transaction = connection.BeginTransaction())
        {
          try
          {
            // Selet UserKey from Users where UserKey is equal to hashedUserKey
            using (MySqlCommand command = connection.CreateCommand())
            {
              const string query = "SELECT UserKey FROM Users";
              command.Transaction = transaction;
              command.CommandText = query;
              using MySqlDataReader reader = command.ExecuteReader();
              if (reader.HasRows) {
                while (reader.Read())
                {
                  string hashedUserKeyInDB = reader.GetString(0);
                  if (BCrypt.Net.BCrypt.Verify(originalUserKey, hashedUserKeyInDB)) {
                    connection.Close();
                    return true;
                  }
                }
              }
            }

            // Insert a user to Users table with the hashedUserKey
            using (MySqlCommand command = connection.CreateCommand())
            {
              const string userKey = "@USERKEY";
              const string query = "INSERT INTO Users (UserKey) Values ("+ userKey +")";
              command.CommandText = query;
              command.Transaction = transaction;
              command.Parameters.AddWithValue(userKey, hashedUserKey);
              command.ExecuteNonQuery();
            }

            // Commit the transaction
            transaction.Commit();

            // Transaction completed successfully
            connection.Close();
            return true;
          }
          catch (Exception)
          {
            // An error occurred, rollback the transaction
            transaction.Rollback();

            // Handle the exception
            connection.Close();
            return false;
          }
        }
      }
    }
  }
}