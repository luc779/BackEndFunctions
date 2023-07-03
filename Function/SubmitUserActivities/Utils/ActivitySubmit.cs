using DatabaseConnector;
using MySqlConnector;

namespace SubmitUserActivitiesUtil
{
    public static class ActivitySubmit
    {
        public static bool Submit(int userID, List<dynamic> transports, List<dynamic> foods, List<dynamic> utilities) {
            // check if the new parameters are valid numbers
            bool validatedNumbers = ValidateActivitiesInput.ValidateNumbers(transports, foods, utilities);
            if (!validatedNumbers) {
                throw new Exception();
            }

            // create connection
            MySqlConnection connection = DatabaseConnecter.MySQLDatabase();
            connection.Open();
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DateTime date = DateTime.UtcNow;

                // delete previous entries
                DeleteUserActivities.Delete(connection, transaction, userID, date);

                // all are null no need to update database
                if (transports.Count != 0 || foods.Count != 0 || utilities.Count != 0) {
                    // input new entries using same query
                    const string QUERY = "INSERT INTO Activities (UserID, ActivityType, Method, Amount, DateTime, Emission) VALUES ((SELECT ID FROM Users WHERE ID = @UserID), @ActivityType, @Method, @Amount, @DateTime, @Emission)";
                    const string transport = "transport";
                    const string food = "food";
                    const string utility = "utility";

                    // input transport
                    InputActivity.Input(userID, transports, connection, transaction, date, QUERY, transport);

                    // input food
                    InputActivity.Input(userID, foods, connection, transaction, date, QUERY, food);
                    // input utility
                    InputActivity.Input(userID, utilities, connection, transaction, date, QUERY, utility);
                }

                // update Daily emissions
                DailyEmissionsUpdate.Update(userID, connection, transaction, date);

                // commit and close
                transaction.Commit();
                connection.Close();
                return true;
            }
            catch (Exception)
            {
                connection.Close();
                return false;
            }
        }
    }
}