using MySqlConnector;

namespace DatabaseConnector;

public static class DatabaseConnecter
{
    private static MySqlConnection connection;
    private static string server;
    private static string database;
    private static string uid;
    private static string password;

    public static MySqlConnection MySQLDatabase()
    {
        server = "noco2.cxzqvtptlzae.us-west-2.rds.amazonaws.com";
        database = "NoCO2";
        uid = "admin";
        password = "Ghjkl009!";
        string connectionString = "server=" + server + ";uid=" + uid +";pwd=" + password + ";database=" + database;

        connection = new MySqlConnection(connectionString);
        return connection;
    }
}