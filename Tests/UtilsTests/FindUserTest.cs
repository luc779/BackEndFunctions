using Xunit;

namespace BackEndFucntions.Tests.UtilsTests
{
    public static class FindUserTest
    {
        [Fact]
        public static void DatabaseNotFound()
        {
            const string ID = "0";
            const int EXPECTED_RESULT = -1;
            int returnedVal = FindUser.UserFinder(ID);
            Assert.Equal(EXPECTED_RESULT, returnedVal);
        }

        [Fact]
        public static void NotFoundUserKey()
        {
            const string expectedErrorMessage = "DatabaseIssue";
            try
            {
                const string ID = "0303";
                int returnedVal = FindUser.UserFinder(ID);
                throw new Exception(expectedErrorMessage);
            }
            catch (Exception ex)
            {
                Assert.Equal(expectedErrorMessage, ex.Message);
            }
        }
    }
}