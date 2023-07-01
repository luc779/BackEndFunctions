using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace HttpRequestDataFactory;

public static class HttpResponseDataFactory
{
  public static async Task<HttpResponseData> GetHttpResponseData(HttpRequestData req, HttpStatusCode statusCode, string body)
  {
    return await GenerateHttpResponseData(req, statusCode, body);
  }

  public static async Task<HttpResponseData> GetHttpResponseData(HttpRequestData req, HttpStatusCode statusCode, dynamic bodyObject)
  {
    string responseBodyObjectString;
    HttpStatusCode _statusCode = statusCode;
    try {
      responseBodyObjectString = JsonConvert.SerializeObject(bodyObject);
    } catch (Exception) {
      var internalError = new {
        reply = "InternalError"
      };
      responseBodyObjectString = JsonConvert.SerializeObject(internalError);
      _statusCode = HttpStatusCode.InternalServerError;
    }
    return await GenerateHttpResponseData(req, _statusCode, responseBodyObjectString);
  }

  private static async Task<HttpResponseData> GenerateHttpResponseData(HttpRequestData req, HttpStatusCode statusCode, string body)
  {
    var response = req.CreateResponse(statusCode);
    response.Headers ??= new HttpHeadersCollection();
    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
    await response.WriteStringAsync(body);
    return response;
  }
}