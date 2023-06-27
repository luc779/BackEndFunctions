using MySqlConnector;

namespace UserKeyUtils
{
    public static class SelectUserKey
    {
        public static bool Select(MySqlConnection connection, MySqlTransaction transaction, string originalUserKey)
        {
            using MySqlCommand command = connection.CreateCommand();
            const string QUERY = "SELECT UserKey FROM Users";
            command.Transaction = transaction;
            command.CommandText = QUERY;
            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string hashedUserKeyInDB = reader.GetString(0);
                    if (BCrypt.Net.BCrypt.Verify(originalUserKey, hashedUserKeyInDB))
                    {
                        connection.Close();
                        return true;
                    }
                }
            }
            // go to Insert 
            return false;
        }
    }
}