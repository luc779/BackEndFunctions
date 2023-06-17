namespace Company.Function;

    internal class EmissionCalculator
    {
        // conversion rate 1 kg to lbs
        private const double KG_TO_LBS = 2.20462;
        // convserion rate for 1 gram to kg
        private const int GRAM_TO_KG = 1000;
        // food emission class used to seperate grams and serving foods
        class FoodEmission
        {
            public int Emission { get; }
            public bool IsPerServing { get; }

            public FoodEmission(int emission, bool isPerServing)
            {
                Emission = emission;
                IsPerServing = isPerServing;
            }
        }

        // data for vechile average emissions, values are in g per km
        readonly Dictionary<string, int> vehicleType = new()
        {
            { "Truck", 372 },
            { "4 Door Petrol Car", 209 },
            { "Medium Diesel-Powered Car", 171 },
            { "Plane Travel", 146 },
            { "Motorbike", 113 },
            { "Bus", 103 },
            { "Electric Vehicle", 81 },
            { "Train Travel", 37 },
            { "Ferry", 19 },
            { "Bike", 0 }
        };

        // data for food average emissions, values are in g per 100g
        readonly Dictionary<string, FoodEmission> foodEmissions = new()
        {
            { "Beef",                     new FoodEmission(15500, false) },
            { "Lamb",                     new FoodEmission(5840, false) },
            { "Prawns",                   new FoodEmission(4070, false) },
            { "Cheese",                   new FoodEmission(2790, false) },
            { "Pork",                     new FoodEmission(2400, false) },
            { "Chicken",                  new FoodEmission(1820, false) },
            { "Fish",                     new FoodEmission(1340, false) },
            { "Dark Chocolate",           new FoodEmission(950, true) },   // 1 serving
            { "Eggs",                     new FoodEmission(530, true) },   // 1 egg
            { "Berries",                  new FoodEmission(220, true) },   // 1 serving
            { "Rice",                     new FoodEmission(160, false) },
            { "Banana",                   new FoodEmission(110, true) },   // 1 banana
            { "Tofu",                     new FoodEmission(80, false) },
            { "Apple",                    new FoodEmission(60, true) },    // 1 apple
            { "Brassica",                 new FoodEmission(50, false) },
            { "Nuts",                     new FoodEmission(50, false) },
            { "Potatoes",                 new FoodEmission(50, false) },
            { "Orange",                   new FoodEmission(50, true) },    // 1 orange
            { "Root Vegetables",          new FoodEmission(40, false) },
            { "Milk",                     new FoodEmission(800, true) },   // 1 glass
            { "Soy Milk",                 new FoodEmission(250, true) },   // 1 glass
            { "Almond Milk",              new FoodEmission(180, true) }    // 1 glass
        };

        // data for utilities average emissions, values in g per hour
        readonly Dictionary<string, double> utilities = new()
        {
            { "Electrivity", 371.03856 }
        };
        // driving calcuation, returns a string of total emissions from varaibles
        public double DrivingCalculation(string vechileType, string distance)
        {
            // find vechile emission
            double vechileTypeEmission = vehicleType[vechileType];
            // convert miles to km
            double distanceKm = double.Parse(distance) * 1.609344;
            // gram of co2, distance in km * gram per km
            double gramsOfCo2 = distanceKm * vechileTypeEmission;
            // convert grams to kg, then kg to lbs
            double lbsOfCo2 = GramsToLbs(gramsOfCo2);
            // round up 10 decimals
            return RoundUpTenDecimals(lbsOfCo2);
        }
        // food calculations, returns a string of total emissions from variables
        public double FoodCalculation(string foodName, string amount)
        {
            // get food info for food name
            FoodEmission foodInfo = foodEmissions[foodName];
            // get the grams and isPerServing from FoodEmission
            int grams = foodInfo.Emission;
            bool isPerServing = foodInfo.IsPerServing;

            double gramsOfCo2;
            if (isPerServing) {
                // the amount, refers to per serving
                gramsOfCo2 = grams * double.Parse(amount);
            } else {
                // amount, refers to grams
                double amountPerHundredGrams = double.Parse(amount) / 100.0; // 100 because each grams info is per 100 g
                gramsOfCo2 = grams * amountPerHundredGrams;
            }
            // convert grams to kg, then kg to lbs
            double lbsOfCo2 = GramsToLbs(gramsOfCo2);

            // round up 10 decimals
            return RoundUpTenDecimals(lbsOfCo2);
        }
        // clothes calculations, returns a string of total emissions from variables
        public double UtilitiesCalculation(string utility, string kWh)
        {
            // find the emission for the utility
            double emissionPerUtility = utilities[utility];
            // hours times to g per hour
            double gramsOfCo2 = emissionPerUtility * int.Parse(kWh);
            // convert grams to kg, then kg to lbs
            double lbsOfCo2 = GramsToLbs(gramsOfCo2);
            // round up 10 decimals
            return RoundUpTenDecimals(lbsOfCo2);
        }
        private static double GramsToLbs(double gramsOfCo2)
        {
            return gramsOfCo2 / GRAM_TO_KG * KG_TO_LBS;
        }
        private static double RoundUpTenDecimals(double lbsOfCo2)
        {
            return Math.Ceiling(lbsOfCo2 * 10000000000) / 10000000000;
        }
    }
