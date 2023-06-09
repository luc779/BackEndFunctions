using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using FirebaseAdmin.Auth;
using MySqlConnector;
using NoCO2.Util;

namespace Company
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
        req.Body.TryParseJson<User>(out var requestBody);

        // Get "UserKey" parameter from HTTP request as either parameter or post value
        string userKey = requestBody?.UserKey;

        UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);
        // See the UserRecord reference doc for the contents of userRecord.

        try
        {
          // Hash the userKey
          string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey);

          // Check if the database has a user with the same hashedUserKey
          bool isUserAdded = AddUserToDatabase(hashedUserKey);
          if (isUserAdded) {
            return HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.OK, "Success");
          }

          // For some reason, the userkey is not added to the database
          return HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, "InternalError");
        } catch (Exception e) {
          return HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.InternalServerError, "InternalError");
        }

      } catch (ArgumentException argError) {
        return HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, "InvalidArgument");
      } catch (FirebaseAuthException authError) {
        return HttpResponseDataFactory.GetHttpResponseData(req, HttpStatusCode.BadRequest, "UserKeyNotAuth");
      }
      throw new NotImplementedException();
    }

    private bool AddUserToDatabase(string hashedUserKey) {
      string server = "databaseht.cyethqvobvkg.us-west-2.rds.amazonaws.com";
      string database = "Hackathon";
      string uid = "masterUsername";
      string password = "vafwa4-vozqyn-naxqAb";
      string connectionString = "server=" + server + ";uid=" + uid +";pwd=" + password + ";database=" + database;

      using (MySqlConnection connection = new MySqlConnection(connectionString))
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
          catch (Exception ex)
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