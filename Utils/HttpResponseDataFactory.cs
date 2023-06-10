using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace NoCO2.Util;

public static class HttpResponseDataFactory
{
  public static async Task<HttpResponseData> GetHttpResponseData(HttpRequestData req, HttpStatusCode statusCode, string body) {
    var response = req.CreateResponse(statusCode);
    response.Headers ??= new HttpHeadersCollection();
    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
    await response.WriteStringAsync(body);
    return response;
  }

  public static async Task<HttpResponseData> GetHttpResponseData(HttpRequestData req, HttpStatusCode statusCode, dynamic bodyObject) {
    string responseBodyObjectString;
    try {
      responseBodyObjectString = JsonConvert.SerializeObject(bodyObject);
    } catch (Exception) {
      var internalError = new {
        reply = "InternalError"
      };
      responseBodyObjectString = JsonConvert.SerializeObject(internalError);
    }

    var response = req.CreateResponse(statusCode);
    response.Headers ??= new HttpHeadersCollection();
    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
    await response.WriteStringAsync(responseBodyObjectString);
    return response;
  }
}