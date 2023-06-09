using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using FirebaseAdmin.Auth;
using MySqlConnector;
using NoCO2.Util;
using Company.Function;

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
      try
      {
        req.Body.TryParseJson<CreateUserBody>(out var requestBody);

        // Get "UserKey" parameter from HTTP request as either parameter or post value
        string userKey = requestBody?.UserKey;

        UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);

        // Hash the userKey
        string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey);

        // Check if the database has a user with the same hashedUserKey
        bool isUserAdded = AddUserToDatabase(hashedUserKey);
        if (isUserAdded) {
          return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.OK, "Success");
        }

        // For some reason, the userkey is not added to the database
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, "InternalError");
      } catch (ArgumentException) {
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, "InvalidArgument");
      } catch (FirebaseAuthException) {
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, "UserKeyNotAuth");
      } catch (Exception) {
        return await HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, "InternalError");
      }
      throw new NotImplementedException();
    }

    // TODO: Move all Database related tasks into one class
    private bool AddUserToDatabase(string hashedUserKey)
    {
      using (MySqlConnection connection = DatabaseConnecter.MySQLDatabase())
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
              command.Transaction = transaction;
              command.CommandText = "SELECT USERKEY FROM Users WHERE UserKey = '" + hashedUserKey + "'";
              using MySqlDataReader reader = command.ExecuteReader();
              if (reader.HasRows) {
                return true;
              }
            }

            // Insert a user to Users table with the hashedUserKey
            using (MySqlCommand command = connection.CreateCommand())
            {
              command.Transaction = transaction;
              string query = "INSERT INTO Users (USERKEY) Values (@USERKEY)";
              command.CommandText = query;
              command.Parameters.AddWithValue("@USERKEY", hashedUserKey);
              command.ExecuteNonQuery();
            }

            // Commit the transaction
            transaction.Commit();

            // Transaction completed successfully
            return true;
          }
          catch (Exception)
          {
            // An error occurred, rollback the transaction
            transaction.Rollback();

            // Handle the exception
            return false;
          }
        }
      }
    }
  }
}