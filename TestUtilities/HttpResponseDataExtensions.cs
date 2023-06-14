using Microsoft.Azure.Functions.Worker.Http;

namespace NoCO2.Test.Util;

public static class HttpResponseDataExtensions
{
  public static async Task<string> GetResponseBody(this HttpResponseData response)
  {
    response.Body.Seek(0, SeekOrigin.Begin);
    using var reader = new StreamReader(response.Body);
    return await reader.ReadToEndAsync();
  }
}