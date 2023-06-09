using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Xunit;
using HttpRequestDataFactory;
using TestUtilities;

namespace UtilsTest
{
  public class HttpResponseDataFactoryTests
  {
    [Theory]
    [InlineData("Test body")]
    [InlineData("{\"reply\":\"Success\"}")]
    public async Task HttpResponseDataWithCorrectProperties(string body)
    {
        // Arrange
        var request = TestFactory.CreateHttpRequest("Empty", "post");
        var statusCode = HttpStatusCode.OK;

        // Act
        HttpResponseData httpResponseData = await HttpResponseDataFactory.GetHttpResponseData(request, statusCode, body);

        // Assert
        Assert.Equal(statusCode, httpResponseData.StatusCode);
        Assert.Equal(body, await httpResponseData.GetResponseBody());
    }
  }
}
