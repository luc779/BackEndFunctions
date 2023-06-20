using Company.Function;
using Xunit;

namespace BackEndFunctions
{
    public class EmissionCalculatorTest
    {
        [Fact]
        public void CorrectGramsToLbsConversion()
        {
            const double GRAMS = 1;
            const double EXPECTED_RESPONSE = 0.00220462;
            double emissionVal = EmissionCalculator.GramsToLbs(GRAMS);
            Assert.Equal(EXPECTED_RESPONSE, emissionVal);
        }
        [Fact]
        public void CorrectRoundUpDecimals()
        {
            const double TO_CONVERT = 0.1234567891234;
            const double EXPECTED_RESPONSE = 0.1234567892;
            double emissionVal = EmissionCalculator.RoundUpTenDecimals(TO_CONVERT);
            Assert.Equal(EXPECTED_RESPONSE, emissionVal);
        }

        [Theory]
        [InlineData("Beef", 8, 77.4998446156)]
        [InlineData("Lamb", 12, 43.7999121828)]
        [InlineData("Prawns", 4, 10.1749795996)]
        [InlineData("Cheese", 3, 5.2312395116)]
        public void CorrectFoodEmissionOzInput(string FOOD_NAME, double AMOUNT, double EXPECTED_RESPONSE)
        {
            EmissionCalculator calculator = new();
            double emissionVal = calculator.FoodCalculation(FOOD_NAME, AMOUNT);
            Assert.Equal(EXPECTED_RESPONSE, emissionVal);
        }

        [Theory]
        [InlineData("Eggs", 6, 7.0106916)]
        [InlineData("Banana", 3, 0.7275246)]
        [InlineData("Milk", 5, 8.81848)]
        public void CorrectFoodEmissionServingInput(string FOOD_NAME, double AMOUNT, double EXPECTED_RESPONSE)
        {
            EmissionCalculator calculator = new();
            double emissionVal = calculator.FoodCalculation(FOOD_NAME, AMOUNT);
            Assert.Equal(EXPECTED_RESPONSE, emissionVal);
        }
        [Theory]
        [InlineData("Motorbike", 23, 9.2212311282)]
        [InlineData("Truck", 120, 158.3823615087)]
        [InlineData("Bike", 320, 0)]
        public void CorrectTransportEmissionServingInput(string TRANSPORT_TYPE, double MILES, double EXPECTED_RESPONSE)
        {
            EmissionCalculator calculator = new();
            double emissionVal = calculator.DrivingCalculation(TRANSPORT_TYPE, MILES);
            Assert.Equal(EXPECTED_RESPONSE, emissionVal);
        }
        [Fact]
        public void CorrectUtilitiesCalculation()
        {
            const string UTILITY_TYPE = "Electricity";
            const double AMOUNT = 30;
            const double EXPECTED_RESPONSE = 24.5399709045;
            EmissionCalculator calculator = new();
            double emissionVal = calculator.UtilitiesCalculation(UTILITY_TYPE, AMOUNT);
            Assert.Equal(EXPECTED_RESPONSE, emissionVal);
        }
    }
}