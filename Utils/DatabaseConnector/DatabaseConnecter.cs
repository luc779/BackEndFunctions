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
        server = "databaseht.cyethqvobvkg.us-west-2.rds.amazonaws.com";
        database = "Hackathon";
        uid = "masterUsername";
        password = "vafwa4-vozqyn-naxqAb";
        string connectionString = "server=" + server + ";uid=" + uid +";pwd=" + password + ";database=" + database;

        connection = new MySqlConnection(connectionString);
        return connection;
    }
}