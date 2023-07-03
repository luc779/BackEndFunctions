using MySqlConnector;

namespace SubmitUserActivitiesUtil
{
    public static class InputActivity
    {
        public  static void Input(int userID, List<dynamic> ActivityType, MySqlConnection connection, MySqlTransaction transaction, DateTime todaysDate, string QUERY, string ACTIVITY_TYPE) {
            if (ActivityType.Count == 0) return;
            // new command
            using MySqlCommand command = new(QUERY, connection);
            // start transaction
            command.Transaction = transaction;
            // varaibles that dont change for each unique foods
            EmissionCalculator calculations = new();

            try
            {
                // add each food in foods
                foreach (var SpecificActivity in ActivityType)
                {
                    // Specific Activity type; Food, Transport, Utility
                    string method = SpecificActivity.Type;
                    // Specific Activity amount; Food, Transport, Utility
                    double amount = SpecificActivity.Amount;

                    // calculate emissions, method (foodType), amount (servings or grams)
                    double emission = calculations.PickCalulation(method, amount, ACTIVITY_TYPE);

                    // insert info in Activities Table
                    command.CommandText = QUERY;
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.Parameters.AddWithValue("@ActivityType", ACTIVITY_TYPE);
                    command.Parameters.AddWithValue("@Method", method);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@DateTime", todaysDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Emission", emission);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) {
                throw; // makes SubmitActivities rollback
            }
        }
    }
}