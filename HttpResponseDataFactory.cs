using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace NoCO2.Util;

public static class HttpResponseDataFactory
{
  public static HttpResponseData GetHttpResponseData(HttpRequestData req, HttpStatusCode statusCode, string body) {
    var response = req.CreateResponse(statusCode);
    response.WriteStringAsync(body);
    return response;
  }
}