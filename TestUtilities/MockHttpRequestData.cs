using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Text;

namespace TestUtilities;

public sealed class MockHttpRequestData : HttpRequestData
{
  private static readonly FunctionContext context = Mock.Of<FunctionContext>();

  public MockHttpRequestData(string body, string method) : base(context)
  {
    var bytes = Encoding.UTF8.GetBytes(body);
    Body = new MemoryStream(bytes);
    Method = method;
  }

  public override HttpResponseData CreateResponse()
  {
    return new MockHttpResponseData(context);
  }

  public override Stream Body { get; }
  public override HttpHeadersCollection Headers { get; }
  public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
  public override Uri Url { get; }
  public override IEnumerable<ClaimsIdentity> Identities { get; }
  public override string Method { get; }
}