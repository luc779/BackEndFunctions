using Xunit;

namespace Company.Function;
public class MySqlTests
{
    [Fact]
    public void DatabaseConnectionTest()
    {
        const string num = "456";
        const string numEmpty = "0.00000";
        MySQLDatabase data = new();
        data.CustomerCreation(num);
        Assert.Equal(numEmpty, data.GetCustomerTotalEmissions(num).ClothesTotal);
        Assert.Equal(numEmpty, data.GetCustomerTotalEmissions(num).FoodTotal);
        Assert.Equal(numEmpty, data.GetCustomerTotalEmissions(num).TransprotTotal);
    }

    [Fact]
    public void TransportTotalCreateTest()
    {
        const string num = "1234";
        const string mpg = "25";
        const string distance = "1500";
        MySQLDatabase data = new();
        data.DrivingCalculation(num, mpg, distance);
        const string expectedResult = "730.03200";
        Assert.Equal(expectedResult, data.GetCustomerTransportDaily(num).Emission);
    }

    [Fact]
    public void ClothesTotalCreateTest()
    {
        const string num = "1234";
        const string fabric = "Wool";
        const string amount = "5";
        MySQLDatabase data = new();
        data.ClothesCalculation(num, fabric, amount);
        const string expectedResult = "69.45000";
        Assert.Equal(expectedResult, data.GetCustomerClothesDaily(num).Emission);
    }

    [Fact]
    public void FoodTotalCreateTest()
    {
        const string num = "1234";
        const string foodName = "Cheese";
        const string amount = "5";
        MySQLDatabase data = new();
        data.FoodCalculation(num, foodName, amount);
        const string expectedResult = "0.13950";
        Assert.Equal(expectedResult, data.GetCustomerFoodDaily(num).Emission);
    }

    [Fact]
    public void DuplicateCustomerID()
    {
        const string num = "345";
        MySQLDatabase data = new();
        const int faultNum = -1;
        Assert.Equal(faultNum, data.CustomerCreation(num));
    }

    [Fact]
    public void LastThreeDaysTest()
    {
        const string custID = "789";
        MySQLDatabase data = new();
        KeyValuePair<string, string> response = (data.GetLastThreeEntries(custID))[0];
        KeyValuePair<string, string> response2 = (data.GetLastThreeEntries(custID))[1];
        KeyValuePair<string, string> response3 = (data.GetLastThreeEntries(custID))[2];
        Assert.Equal("05/08/2023", response.Key);
        Assert.Equal("78.00000", response.Value);
        Assert.Equal("05/07/2023", response2.Key);
        Assert.Equal("256.00000", response2.Value);
        Assert.Equal("-1", response3.Key);
        Assert.Equal("-1", response3.Value);
    }

    [Fact]
    public void GetAllTotalEntriesTest()
    {
        const string custID = "1234";
        MySQLDatabase data = new();
        KeyValuePair<string, string> response = (data.GetLastThreeEntries(custID))[0];
        Assert.Equal("05/08/2023", response.Key);
        Assert.Equal("799.62150", response.Value);
    }

    [Fact]
    public void DrivingCalculationTest()
    {
        Functions data = new();
        string end = data.DrivingCalculation("25","1500");
        Assert.Equal("730.032", end);
    }

    [Fact]
    public void FoodCalculationTest()
    {
        Functions data = new();
        string end = data.FoodCalculation("Lamb","4");
        Assert.Equal("0.2336", end);
    }

    [Fact]
    public void ClothesCalculationTest()
    {
        Functions data = new();
        string end = data.ClothesCalculation("Cotton","5");
        Assert.Equal("41.5", end);
    }
}