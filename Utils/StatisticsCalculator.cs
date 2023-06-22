namespace NoCO2.Util;

internal class StatisticsCalculator
{
  public List<EmissionStatistic> GetUserEmissionStatistics(int userID)
  {
    List<EmissionStatistic> statistics = new();
    return statistics;
  }

  public EmissionStatistic GetHighestEmissionActivityByUserID(int userID)
  {
    return new EmissionStatistic {Statistic = null, Topic = null, Stat = null};
  }
}