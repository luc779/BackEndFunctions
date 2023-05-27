using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class LogTransport
    {
        private readonly ILogger _logger;

        public LogTransport(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LogTransport>();
        }

        [Function("LogTransport")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a Transport Log request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            

            response.WriteString("Transport logger");

            return response;
        }
    }
}
