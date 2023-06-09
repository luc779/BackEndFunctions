using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoCO2.Test.Util
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