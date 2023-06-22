using Xunit;
using NoCO2.Util;

namespace NoCO2.Test
{
  public class StatisticsCalculatorTest
  {
    [Fact]
    public void GetUserHighestEmissionActivity()
    {
      const int USER_ID = 0;

      EmissionStatistic statistic = StatisticsCalculator.GetHighestEmissionActivitybyUserID(USER_ID);

      Assert.NotEmpty(statistic.Statistic);
      Assert.NotEmpty(statistic.Topic);
      Assert.NotEmpty(statistic.Stat);
    }
  }
}