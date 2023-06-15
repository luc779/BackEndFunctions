namespace Company.Function;

    internal class Functions
    {
        // variable to convert gallon to liters
        private readonly double literInGallon = 4.54;

        // variable to convert liter to Kg of Co2
        private readonly double literToKgCo2 = 2.68;

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
            { "Brassica, nuts, potatoes", new FoodEmission(50, false) },
            { "Orange",                   new FoodEmission(50, true) },    // 1 orange
            { "Root vegetables",          new FoodEmission(40, false) },
            { "Milk",                     new FoodEmission(800, true) },   // 1 glass
            { "Soy Milk",                 new FoodEmission(250, true) },   // 1 glass
            { "Almond Milk",              new FoodEmission(180, true) }    // 1 glass
        };

        // data for fabric average emissions, values in kg
        readonly Dictionary<string, double> fabrics = new()
        {
            { "Wool", 13.89 },
            { "Acrylic Fabric", 11.53 },
            { "Cotton", 8.3 },
            { "Silk", 7.63 },
            { "Nylon", 7.31 },
            { "Polyester", 6.4 },
            { "Linen", 5.4 }
        };
        // driving calcuation, returns a string of total emissions from varaibles
        public double DrivingCalculation(string vechileType, string distance)
        {
            double vechileTypeEmission = vehicleType[vechileType];
            double distanceKm = double.Parse(distance) * 1.609344;
            double gramsOfCo2 = distanceKm * vechileTypeEmission;
            double kgOfCo2 = gramsOfCo2 / 1000;
            // round up 10 decimals
            return Math.Ceiling(kgOfCo2 * 10000000000) / 10000000000;
        }
        // food calculations, returns a string of total emissions from variables
        public double FoodCalculation(string foodName, string amount)
        {
            FoodEmission foodInfo = foodEmissions[foodName];
            int grams = foodInfo.Emission;
            bool isPerServing = foodInfo.IsPerServing;
            double gramsOfCo2;
            if (isPerServing) {
                gramsOfCo2 = grams * double.Parse(amount);
            } else {
                double amountPerHundredGrams = double.Parse(amount) / 100.0; // 100 because each grams info is per 100 g
                gramsOfCo2 = grams * amountPerHundredGrams;
            }
            double kgOfCo2 = gramsOfCo2 / 1000;
            // round up 10 decimals
            return Math.Ceiling(kgOfCo2 * 10000000000) / 10000000000;
        }
        // clothes calculations, returns a string of total emissions from variables
        public string ClothesCalculation(string clothesMaterial, string amount)
        {
            double emissionPerFabric = fabrics[clothesMaterial];
            double kgOfCo2 = emissionPerFabric * int.Parse(amount);
            // double roundUp5Decimals = Math.Ceiling(kgOfCo2 * 100000) / 100000;
            return kgOfCo2.ToString();
        }
    }
