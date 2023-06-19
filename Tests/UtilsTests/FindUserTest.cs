using Xunit;

namespace BackEndFunctions.Tests.UtilsTests
{
    public static class FindUserTest
    {
        [Fact]
        public static void DatabaseNotFound()
        {
            const string USER_KEY = "0";
            const int EXPECTED_RESULT = -1;
            int returnedVal = FindUser.UserFinder(USER_KEY);
            Assert.Equal(EXPECTED_RESULT, returnedVal);
        }

        [Fact]
        public static void NotFoundUserKey()
        {
            const string expectedErrorMessage = "DatabaseIssue";
            try
            {
                const string USER_KEY = "0303";
                int returnedVal = FindUser.UserFinder(USER_KEY);
                throw new Exception(expectedErrorMessage);
            }
            catch (Exception ex)
            {
                Assert.Equal(expectedErrorMessage, ex.Message);
            }
        }
    }
}