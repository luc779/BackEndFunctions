using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using FirebaseAdmin.Auth;
using MySqlConnector;
using Company.Function;

namespace Company
{
  public static class CreateUser
  {
    static CreateUser()
    {
        FirebaseInitializer.Initialize();
    }

    [Function("CreateUser")]
    public static async Task<IActionResult> CreateUserWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create-user")] HttpRequest req,
        ILogger log)
    {
      try
      {
        // Get post body if any
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        // Get "input" parameter from HTTP request as either parameter or post value
        string userKey = req.Query["UserKey"];
        userKey = userKey ?? data?.UserKey;

        UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);
        // See the UserRecord reference doc for the contents of userRecord.

        try
        {
          // Hash the userKey
          string hashedUserKey = BCrypt.Net.BCrypt.HashPassword(userKey);

          // Check if the database has a user with the same hashedUserKey
          bool isUserAdded = AddUserToDatabase(hashedUserKey);
          if (isUserAdded) {
            return new OkObjectResult("Success");
          }

          // For some reason, the userkey is not added to the database
          return new BadRequestObjectResult("InternalError");
        } catch (Exception) {
          return new BadRequestObjectResult("InternalError");
        }

      } catch (ArgumentException) {
        return new BadRequestObjectResult("InvalidArgument");
      } catch (FirebaseAuthException) {
        return new BadRequestObjectResult("UserKeyNotAuth");
      }
      throw new NotImplementedException();
    }

    private static bool AddUserToDatabase(string hashedUserKey) {

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
              string query = "SELECT USERKEY FROM Users WHERE UserKey = '" + hashedUserKey + "'";
              command.Transaction = transaction;
              command.CommandText = query;
              using MySqlDataReader reader = command.ExecuteReader();
              if (reader.HasRows) {
                return true;
              }
            }

            // Insert a user to Users table with the hashedUserKey
            using (MySqlCommand command = connection.CreateCommand())
            {
              const string userKey = "@USERKEY";
              const string query = "INSERT INTO Users (USERKEY) Values ("+ userKey +")";
              command.CommandText = query;
              command.Parameters.AddWithValue(userKey, hashedUserKey);
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