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

  public EmissionStatistic GetAverageEmissionByUserID(int userID)
  {
    DateTime currentDate = DateTime.UtcNow;
    DateTime oneWeekAgo = currentDate.AddDays(-7);

    // Average the sum of total emission with how many entries retrieved
    double average = GetAverageEmissionBetweenTwoDate(currentDate, oneWeekAgo, userID);
    if (average == -1)
    {
      return null;
    }

    return new EmissionStatistic
    {
      Statistic = "Average Emission",
      Topic = "Within 1 Week",
      Stat = average.ToString("0.00")
    };
  }

  public EmissionStatistic GetEmissionDifferenceByUserID(int userID)
  {
    DateTime currentDate = DateTime.UtcNow;
    DateTime oneWeekAgo = currentDate.AddDays(-7);
    DateTime twoWeekAgo = currentDate.AddDays(-14);

    // Retrieve DailyEmission.TotalEmission with a current week
    double currentAverageEmission = GetAverageEmissionBetweenTwoDate(currentDate, oneWeekAgo, userID);
    if (currentAverageEmission == -1)
    {
      return null;
    }

    double previousWeekAverageEmission = GetAverageEmissionBetweenTwoDate(oneWeekAgo, twoWeekAgo, userID);
    if (previousWeekAverageEmission == -1)
    {
      return null;
    }

    double difference = currentAverageEmission - previousWeekAverageEmission;
    string stat = difference.ToString("0.00");
    if (difference >= 0)
    {
      stat = "+" + difference.ToString("0.00");
    }
    return new EmissionStatistic
    {
      Statistic = "Emission Difference",
      Topic = "Between 2 Weeks",
      Stat = stat
    };
  }

  public double GetAverageEmissionBetweenTwoDate(DateTime currentDate, DateTime previousDate, int userID)
  {
    double totalEmission = 0;
    int numEntries = 0;
    double averageEmission = -1;
    // Retrieve DailyEmission.TotalEmission with a week
    MySqlConnection connection = DatabaseConnecter.MySQLDatabase();

    using (connection)
    {
      connection.Open();

      const string query = "SELECT e.TotalAmount FROM DailyEmission AS e " +
        "JOIN Users AS u ON e.UserID = u.ID " +
        "WHERE u.ID = @userId AND e.DateTime >= '@previousDate' AND e.DateTime <= '@currentDate'";

      using MySqlCommand command = connection.CreateCommand();
      command.CommandText = query;
      command.Parameters.AddWithValue("@userId", userID);
      command.Parameters.AddWithValue("@previousDate", previousDate.ToString("yyyy/MM/dd"));
      command.Parameters.AddWithValue("@currentDate", currentDate.ToString("yyyy/MM/dd"));

      using MySqlDataReader reader = command.ExecuteReader();
      if (reader.HasRows)
      {
        while (reader.Read())
        {
          double dailyTotalEmission = reader.GetDouble("TotalAmount");
          totalEmission += dailyTotalEmission;
          numEntries += 1;
        }
      } else {
        connection.Close();
        return averageEmission;
      }
      connection.Close();
    }

    averageEmission = totalEmission / numEntries;
    return averageEmission;
  }
}