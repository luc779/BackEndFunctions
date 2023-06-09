using DatabaseConnector;
using MySqlConnector;

namespace UserFinder
{
    public static class FindUser
    {
        public static int UserFinder(string userKey)
        {
            int ID = -1;
            const string QUERY = "SELECT * From Users";

            try {
                // open connection
                MySqlConnection connection = DatabaseConnecter.MySQLDatabase();
                connection.Open();

                // set command and reader at the Users Table
                using MySqlCommand command = new(QUERY, connection);
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int currID = reader.GetInt32("ID");
                    string currUserKey = reader.GetString("UserKey");

                    bool found = BCrypt.Net.BCrypt.Verify(userKey, currUserKey);
                    if (found) {
                        ID = currID;
                        break;
                    }
                }
                connection.Close();
                return ID;
            }
            catch (MySqlException)
            {
                throw new Exception("DatabaseIssue");
            }
        }
    }
}