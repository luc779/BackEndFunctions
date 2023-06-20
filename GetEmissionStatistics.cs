using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using FirebaseAdmin.Auth;
using MySqlConnector;
using NoCO2.Util;
using Company.Function;

namespace NoCO2.Function
{
  public class GetEmissionStatistics
  {
    static GetEmissionStatistics()
    {
        FirebaseInitializer.Initialize();
    }

    [Function("GetEmissionStatistics")]
    public async Task<HttpResponseData> GetEmissionStatisticsWithUserKey(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "create-user")] HttpRequestData req)
    {
      throw new NotImplementedException();
    }
  }
}