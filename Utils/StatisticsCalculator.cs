using MySqlConnector;
using Company.Function;

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

    Dictionary<string, double> emissionsDict = new();
    emissionsDict.Add("transport", 0);
    emissionsDict.Add("foods", 0);
    emissionsDict.Add("utility", 0);

    // Retrieve emission per activity type from database
    MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

    using (connection)
    {
      connection.Open();

      const string query = "SELECT a.ActivityType, a.Emission " +
        "FROM Activities AS a " +
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
        while (reader.Read())
        {
          string activityType = reader.GetString("ActivityType");
          if (emissionsDict.ContainsKey(activityType)) {
            emissionsDict[activityType] += reader.GetDouble("Emission");
          } else {
            Console.WriteLine("ALERT (GetHighestEmissionActivityByUserID): ActivityType read from query does not exists in emissionDict: " + activityType);
          }
        }
      } else {
        connection.Close();
        return null;
      }
      connection.Close();
    }

    // Calculate percentage
    double sum = emissionsDict.Values.Sum();
    emissionsDict["transport"] = emissionsDict["transport"] / sum;
    emissionsDict["foods"] = emissionsDict["foods"] / sum;
    emissionsDict["utility"] = emissionsDict["utility"] / sum;

    // Determine the highest emission activity
    KeyValuePair<string, double> highestEmissionActivity = emissionsDict
      .OrderByDescending(keyValue => keyValue.Value)
      .FirstOrDefault();
    return new EmissionStatistic
    {
      Statistic = "Highest Emission Activity",
      Topic = char.ToUpper(highestEmissionActivity.Key[0]) + highestEmissionActivity.Key.Substring(1),
      Stat = highestEmissionActivity.Value.ToString("0.00")
    };
  }
}