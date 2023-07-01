using DatabaseConnector;
using MySqlConnector;

namespace GetUserActivitiesUtil
{
    public class DataRetrieval
    {
        public List<dynamic> RetrieveCertainType(int userID, DateTime today, string ACTIVITY_TYPE)
        {
            List<dynamic> Activity = null;
            string QUERY = "SELECT e.Method, e.Amount FROM Activites AS e "+
                "JOIN Users As u ON e.UsersID = u.ID"+
                " WHERE u.ID = '" + userID + "' AND e.DateTime = '" + today.ToString("yyyy/MM/dd") + "' AND e.ActivityType = '" + ACTIVITY_TYPE + "'";

            using MySqlConnection connetion = DatabaseConnecter.MySQLDatabase();
            connetion.Open();

            using MySqlCommand command = new(QUERY, connetion);

            try {
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    string type = reader.GetString("Method");
                    double amount = reader.GetDouble("Amount");
                    Activity.Add(new {Type = type, Amount = amount});
                }
                return Activity;
            }
            catch (Exception) {
                throw new Exception();
            }
        }
    }
}