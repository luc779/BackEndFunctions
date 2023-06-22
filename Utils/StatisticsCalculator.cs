using MySqlConnector;
using Company.Function;
using System.Collections.Generic;

namespace NoCO2.Util;

internal class StatisticsCalculator
{
  // Precondition: userID is found from FindUser.UserFinder
  public List<EmissionStatistic> GetUserEmissionStatistics(int userID)
  {
    List<EmissionStatistic> statistics = new();
    return statistics;
  }

  public EmissionStatistic GetHighestEmissionActivityByUserID(int userID)
  {
    DateTime currentDate = DateTime.UtcNow;
    DateTime oneWeekAgo = currentDate.AddDays(-7);

    MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

    using (connection)
    {
      connection.Open();

      const string query = "SELECT a.ActivityType, a.Emission" +
        "FROM Activities AS a" +
        "JOIN Users AS u ON a.UserID = u.ID " +
        "WHERE u.ID = @userId AND a.DateTime >= '@oneWeekAgo' AND a.DateTime <= '@currentDate'";

      using MySqlCommand command = connection.CreateCommand();
      command.CommandText = query;
      command.Parameters.AddWithValue("@userId", userID);
      command.Parameters.AddWithValue("@oneWeekAgo", oneWeekAgo.ToString("yyyy/MM/dd"));
      command.Parameters.AddWithValue("@currentDate", currentDate.ToString("yyyy/MM/dd"));

      using MySqlDataReader reader = command.ExecuteReader();
      if (reader.HasRows)
      {
        Dictionary<string, double> emissionsDict = new();
        emissionsDict.Add("transport", 0);
        emissionsDict.Add("foods", 0);
        emissionsDict.Add("utility", 0);
        while (reader.Read())
        {
          string activityType = reader.GetString("ActivityType");
          if (emissionsDict.ContainsKey(activityType)) {
            emissionsDict[activityType] += reader.GetDouble("Emission");
          }
        }
      }
    }
    return new EmissionStatistic {Statistic = null, Topic = null, Stat = null};
  }
}