using System.Net;
using GetUserActivitiesFunction;
using Newtonsoft.Json;
using TestUtilities;
using Xunit;

namespace FunctionsTest
{
    public class GetUserActivitiesTest : IClassFixture<GetUserActivities>
    {
        private readonly GetUserActivities _getUserActivities;
        public GetUserActivitiesTest(GetUserActivities getUserActivities)
        {
            _getUserActivities = getUserActivities;
        }
        [Fact]
        public async Task EmptyBody()
        {
            var user = new {};
            string body = JsonConvert.SerializeObject(user);

            var request = TestFactory.CreateHttpRequest(body, "post");
            var response = await _getUserActivities.GetActivities(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("{\"reply\":\"InvalidArgument\"}", await response.GetResponseBody());
        }
        [Fact]
        public async Task UserNotExistsInDatabase()
        {
            // input test userKey
            var user = new {
                UserKey = "RandomRandomRandomRandomRandomRandom"
            };
            string body = JsonConvert.SerializeObject(user);

            var request = TestFactory.CreateHttpRequest(body, "post");
            var response = await _getUserActivities.GetActivities(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("{\"reply\":\"UserKeyNotAuth\"}", await response.GetResponseBody());
        }
    }
}