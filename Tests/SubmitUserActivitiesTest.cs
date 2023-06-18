using System.Net;
using Company;
using Newtonsoft.Json;
using NoCO2.Test.Util;
using Xunit;

namespace BackEndFucntions
{
    public class SubmitUserActivitiesTest : IClassFixture<SubmitUserActivities>
    {
        private readonly SubmitUserActivities _submitUserActivities;
        public SubmitUserActivitiesTest(SubmitUserActivities submitUserActivities)
        {
            _submitUserActivities = submitUserActivities;
        }

        [Fact]
        public async Task EmptyBody()
        {
            var user = new {};
            string body = JsonConvert.SerializeObject(user);

            var request = TestFactory.CreateHttpRequest(body, "post");
            var response = await _submitUserActivities.SubmitInformationAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("{\"reply\":\"InvalidArgument\"}", await response.GetResponseBody());
        }
        [Fact]
        public async Task UserNotExistsInDatabase()
        {
        // input test userKey
        var user = new {
            UserKey = "RandomRandomRandomRandomRandomRandom",
            Transports = new List<(string, double)>(),
            Foods = new List<(string, double)>(),
            Utilities = new List<(string, double)>()
        };
        string body = JsonConvert.SerializeObject(user);

        var request = TestFactory.CreateHttpRequest(body, "post");
        var response = await _submitUserActivities.SubmitInformationAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
        }
    }
}