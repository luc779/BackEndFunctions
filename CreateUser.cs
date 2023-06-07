using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using FirebaseAdmin.Auth;

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
        try {
          // Get post body if any
          string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
          dynamic data = JsonConvert.DeserializeObject(requestBody);

          // Get "input" parameter from HTTP request as either parameter or post value
          string userKey = req.Query["UserKey"];
          userKey = userKey ?? data?.UserKey;

          UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);
          // See the UserRecord reference doc for the contents of userRecord.
          Console.WriteLine($"Successfully fetched user data: {userRecord.Uid}");
        } catch (ArgumentException argError) {

        } catch (FirebaseAuthException authError) {
          return new BadRequestObjectResult("UserKeyNotAuth");
        }
        throw new NotImplementedException();
      }
    }
}