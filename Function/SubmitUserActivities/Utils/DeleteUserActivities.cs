using MySqlConnector;

namespace SubmitUserActivitiesUtil
{
    public static class DeleteUserActivities
    {
        public static void Delete(MySqlConnection connection, MySqlTransaction transaction, int userID, DateTime todaysDate) {
            // new command
            string query = "DELETE a FROM Activities AS a JOIN Users AS u ON a.UserID = u.ID WHERE u.ID = @UserId AND a.DateTime = @DateTime";
            using MySqlCommand command = new(query, connection);
            // start transaction
            command.Transaction = transaction;
            try
            {
                command.Parameters.AddWithValue("@UserId", userID);
                command.Parameters.AddWithValue("@DateTime", todaysDate.ToString("yyyy-MM-dd"));
                command.ExecuteNonQuery();
            }
            catch (Exception) {
                throw new Exception();
            }
        }
    }
}