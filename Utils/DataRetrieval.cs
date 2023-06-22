using Company.Function;
using MySqlConnector;

namespace BackEndFucntions
{
    public class DataRetrieval
    {
        public List<dynamic> RetrieveCertainType(int userID, DateTime today, string ACTIVITY_TYPE)
        {
            List<dynamic> Transports = null;
            string QUERY = "SELECT FROM Activities WHERE UserID = '" + userID + "' AND DateTime = '" + today.ToString("yyyy/MM/dd") + "' AND ActivityType = '" + ACTIVITY_TYPE + "'";

            using MySqlConnection connetion = DatabaseConnecter.MySQLDatabase();
            connetion.Open();

            using MySqlCommand command = new(QUERY, connetion);

            try {
                command.CommandText = QUERY;
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    string type = reader.GetString("Method");
                    double amount = reader.GetDouble("Amount");
                    Transports.Add(new {Type = type, Amount = amount});
                }
                return Transports;
            }
            catch (Exception) {
                throw new Exception();
            }
        }
    }
}