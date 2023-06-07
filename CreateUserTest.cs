using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Company;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;


namespace Company.Test
{
  public class CreateUserTest
  {
    private readonly ILogger logger = TestFactory.CreateLogger();
    private readonly ITestOutputHelper output;
    public CreateUserTest(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task FirebaseNotAuthorized()
    {
      const string userKey = "RandomRandomRandomRandomRandomRandom";
      var request = TestFactory.CreateHttpRequest("UserKey", userKey, "post");
      var response = (ObjectResult)await CreateUser.CreateUserWithUserKey(request, logger);
      Assert.Equal(400, response.StatusCode);
      Assert.Equal("UserKeyNotAuth", response.Value);
    }
  }
}