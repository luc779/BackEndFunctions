namespace Company.Function;

    internal class Functions
    {
        // variable to convert gallon to liters
        private readonly double literInGallon = 4.54;

        // variable to convert liter to Kg of Co2
        private readonly double literToKgCo2 = 2.68;

        // data for vechile average emissions, values are in g per km
        readonly Dictionary<string, double> vehicleType = new()
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
        readonly Dictionary<string, double> foodEmissions = new()
        {
            { "Beef", 0.155 },
            { "Lamb", 0.0584 },
            { "Prawns", 0.0407 },
            { "Cheese", 0.0279 },
            { "Pork", 0.024 },
            { "Chicken", 0.0182 },
            { "Fish", 0.0134 },
            { "Chocolate", 0.019 },
            { "Egg", 0.0053 },
            { "Tomato", 0.00213 },
            { "Berries", 0.001527 },
            { "Rice", 0.0016 },
            { "Banana", 0.11 },
            { "Tofu", 0.0008 },
            { "Apple", 0.06 },
            { "Brassicas", 0.0005 },
            { "Nuts", 0.0005 },
            { "Orange", 0.05 },
            { "Potatoes", 0.0005 },
            { "Root Vegetables", 0.0004 }
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
            // round up 5 decimals
            return Math.Ceiling(kgOfCo2 * 100000) / 100000;
        }
        // food calculations, returns a string of total emissions from variables
        public string FoodCalculation(string foodName, string amount)
        {
            double emissionPerUnitOfFood = foodEmissions[foodName];
            double kgOfCo2 = emissionPerUnitOfFood * int.Parse(amount);
            // double roundUp5Decimals = Math.Ceiling(kgOfCo2 * 100000) / 100000;
            return kgOfCo2.ToString();
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
