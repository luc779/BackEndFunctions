using MySqlConnector;

namespace SubmitUserActivitiesUtil
{
    public static class DailyEmissionsUpdate
    {
        public static void Update(int userID, MySqlConnection connection, MySqlTransaction transaction, DateTime todaysDate)
        {
            // get the total Emissions from the day
            double totalEmissions = GetTotalEmissions(userID, connection, transaction, todaysDate);
            UpdateOrSetDailyEmissions(userID, connection, transaction, todaysDate, totalEmissions);
        }
        private static double GetTotalEmissions(int userID, MySqlConnection connection, MySqlTransaction transaction, DateTime todaysDate)
        {
            // new command
            string query = "SELECT a.Emission FROM Activities AS a JOIN Users AS u ON u.ID = a.UserID WHERE u.ID = " + userID + " AND a.DateTime = '" + todaysDate.ToString("yyyy-MM-dd") + "'";
            using MySqlCommand command = new(query, connection);
            // start a transaction
            command.Transaction = transaction;

            try {
                double totalEmissions = 0;
                // set command and reader at the Users Table
                //command.CommandText = query;
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    double currEmission = reader.GetDouble("Emission");
                    totalEmissions += currEmission;
                }
                return totalEmissions;
            }
            catch (Exception) {
                transaction.Rollback();
                throw;
            }
        }
        private static void UpdateOrSetDailyEmissions(int userID, MySqlConnection connection, MySqlTransaction transaction, DateTime todaysDate, double totalEmissions)
        {
            bool isEntryExists = false;
            try
            {
                // read table DailyEmissions
                string query = "SELECT * FROM DailyEmission AS e JOIN Users AS u ON u.ID = e.UserID WHERE u.ID = " + userID + " AND e.DateTime = '"+ todaysDate.ToString("yyyy-MM-dd") +"' FOR UPDATE";
                using (MySqlCommand command = new(query, connection))
                {
                    command.Transaction = transaction;
                    using MySqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows) isEntryExists = true;
                }

                if (isEntryExists)
                {
                    // Update the TotalAmount for the existing entry
                    string updateQuery = "UPDATE DailyEmission AS e JOIN Users AS u ON u.ID = e.UserID SET e.TotalAmount = @TotalAmount WHERE u.ID = " + userID + " AND e.DateTime = '" + todaysDate.ToString("yyyy-MM-dd") + "'";

                    // command to update table DailyEmissions
                    using MySqlCommand updateCommand = new(updateQuery, connection);
                    // start a transaction
                    updateCommand.Transaction = transaction;
                    updateCommand.Parameters.AddWithValue("@TotalAmount", totalEmissions);
                    updateCommand.ExecuteNonQuery();
                }
            }
            catch (Exception) {
                transaction.Rollback();
                throw; // makes SubmitActivities Rollback
            }

            if (isEntryExists) return;

            try {
                // Entry does not exist, insert a new row
                const string INSERT_QUERY = "INSERT INTO DailyEmission (UserID, TotalAmount, Goal, DateTime) VALUES ((SELECT ID FROM Users WHERE ID = @UserID), @TotalAmount, @Goal, @DateTime)";

                // command to insert new row to DailyEmissions
                using MySqlCommand insertCommand = new(INSERT_QUERY, connection);
                // start a transaction
                insertCommand.Transaction = transaction;
                insertCommand.Parameters.AddWithValue("@UserID", userID);
                insertCommand.Parameters.AddWithValue("@TotalAmount", totalEmissions);
                const double GOAL = 60.4;
                insertCommand.Parameters.AddWithValue("@Goal", GOAL);
                insertCommand.Parameters.AddWithValue("@DateTime", todaysDate.ToString("yyyy-MM-dd"));
                insertCommand.ExecuteNonQuery();
            } catch (Exception) {
                transaction.Rollback();
                throw;
            }

        }
    }
}