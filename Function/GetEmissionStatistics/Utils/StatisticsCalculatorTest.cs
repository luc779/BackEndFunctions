using Xunit;
using GetEmissionStatisticsUtil;

namespace FunctionsTest.Util
{
  public class StatisticsCalculatorTest
  {
    [Fact]
    public void GetUserHighestEmissionActivity()
    {
      const int USER_ID = 0;
      StatisticsCalculator calculator = new();

      EmissionStatistic statistic = StatisticsCalculator.GetHighestEmissionActivityByUserID(USER_ID);

      Assert.NotEmpty(statistic.Statistic);
      Assert.NotEmpty(statistic.Topic);
      Assert.NotEmpty(statistic.Stat);
    }

    [Fact]
    public void GetUserAverageEmission()
    {
      const int USER_ID = 0;
      StatisticsCalculator calculator = new();

      EmissionStatistic statistic = StatisticsCalculator.GetAverageEmissionByUserID(USER_ID);

      Assert.NotEmpty(statistic.Statistic);
      Assert.NotEmpty(statistic.Topic);
      Assert.NotEmpty(statistic.Stat);
    }

    [Fact]
    public void GetUserEmissionDifference()
    {
      const int USER_ID = 0;
      StatisticsCalculator calculator = new();

      EmissionStatistic statistic = StatisticsCalculator.GetEmissionDifferenceByUserID(USER_ID);

      Assert.NotEmpty(statistic.Statistic);
      Assert.NotEmpty(statistic.Topic);
      Assert.NotEmpty(statistic.Stat);
    }
  }
}