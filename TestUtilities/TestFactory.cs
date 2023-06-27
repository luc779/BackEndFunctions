using Microsoft.Azure.Functions.Worker.Http;

namespace TestUtilities
{
  public static class TestFactory
  {
    private static Dictionary<string, string> CreateDictionary(string key, string value)
    {
      var qs = new Dictionary<string, string>
      {
        { key, value }
      };
      return qs;
    }

    public static HttpRequestData CreateHttpRequest(string body, string method = "get")
    {
      return new MockHttpRequestData(body,method);
    }
  }
}