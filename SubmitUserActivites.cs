using System.Net;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using MySqlConnector;
using Newtonsoft.Json;

namespace Company.Function
{
    public class SubmitUserActivites
    {

        public SubmitUserActivites()
        {
            FirebaseInitializer.Initialize();
        }

        [Function("SubmitUserActivites")]
        public static async Task<IActionResult> SubmitInformationAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit-user-activity")] HttpRequestData req)
        {
           try
           {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                // Get "input" parameter from HTTP request as either parameter or post value
                string userKey = req.Query["userKey"];
                userKey ??= data?.userKey;

                List<dynamic> transports = data.Transports;
                List<dynamic> foods = data.Foods;
                List<dynamic> utilities = data.Utilities;

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(userKey);
                // See the UserRecord reference doc for the contents of userRecord.

                try
                {
                    // find userKey and return the ID
                    int ID = FindUser(userKey);
                    if (ID == -1) {
                        return new BadRequestObjectResult("UserNotFound");
                    }

                    // Check if the database has a user with the same userKey
                    bool isUserAdded = SubmitActivities(ID, transports, foods, utilities);
                    if (isUserAdded) {
                        return new OkObjectResult("Success");
                    } else {
                        return new BadRequestObjectResult("InvalidInput");
                    }
                }
                catch (Exception) {
                    return new BadRequestObjectResult("InternalError");
                }
           }
           catch (ArgumentException) {
                return new BadRequestObjectResult("InvalidArgument");
            }
            catch (FirebaseAuthException) {
                return new BadRequestObjectResult("UserKeyNotAuth");
            }
            throw new NotImplementedException();
        }
        private static int FindUser(string userKey)
        {
            int ID = -1;
            const string query = "SELECT * From Users";

            // open connection
            MySqlConnection connection = DatabaseConnecter.MySQLDatabase();
            connection.Open();

            // set command and reader at the Users Table
            using MySqlCommand command = new(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int currID = reader.GetInt32("ID");
                string currUserKey = reader.GetString("UserKey");

                bool found = BCrypt.Net.BCrypt.Verify(userKey, currUserKey);
                if (found) {
                    ID = currID;
                    break;
                }
            }
            return ID;
        }
        public static bool SubmitActivities(int ID, List<dynamic> transports, List<dynamic> foods, List<dynamic> utilities) {
            MySqlConnection connection = DatabaseConnecter.MySQLDatabase();
            using(connection)
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    DateTime todaysDate = DateTime.UtcNow;
                    string date = todaysDate.ToString("yyyy/MM/dd");

                    // delete previous entries
                    DeleteFromDatabase(connection, ID, date);

                    // check if the new parameters are valid numbers
                    bool validatedNumbers = ValidateNumbers(transports, foods, utilities);
                    if (!validatedNumbers) {
                        throw new Exception();
                    }

                    // input new entries using same query
                    const string query = "INSERT INTO Activities (UserID, ActivityType, Method, Amount, DateTime, Emission) Values (@UserID, @ActivityType, @Method, @Amount, @DateTime, @Emission)";
                    InputTransport(ID, transports, connection, date, query);
                    InputFood(ID, foods, connection, date, query);
                    InputUtilities(ID, utilities, connection, date, query);

                    // update Daily emissions
                    UpdateDailyEmissions(ID, connection, date);

                    // commit and close
                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    connection.Close();
                    return false;
                }
            }
            return true;
        }
        private static bool ValidateNumbers(List<dynamic> transports, List<dynamic> foods, List<dynamic> utilities)
        {
            // call each individual amount validator
            bool transportsValid = ValidateTransportAmounts(transports);
            bool foodsValid = ValidateFoodAmounts(foods);
            bool utilitiesValid = ValidateUtilityAmounts(utilities);

            // check if any are false, return false, otherwise true
            return transportsValid && foodsValid && utilitiesValid;
        }
        private static bool ValidateTransportAmounts(List<dynamic> transports)
        {
            // go through each amount in transports and check if its a number variable
            foreach (var transport in transports)
            {
                string amount = transport.Miles;
                if (!double.TryParse(amount, out _))
                {
                    return false; // Invalid non-number value found, return false
                }
            }
            return true; // All amounts are valid numbers
        }
        private static bool ValidateFoodAmounts(List<dynamic> foods)
        {
            // go through each amount in foods and check if its a number variable
            foreach (var food in foods)
            {
                string amount = food.Amount;
                if (!double.TryParse(amount, out _))
                {
                    return false; // Invalid non-number value found, return false
                }
            }
            return true; // All amounts are valid numbers
        }
        private static bool ValidateUtilityAmounts(List<dynamic> utilities)
        {
            // go through each utilities in transports and check if its a number variable
            foreach (var utility in utilities)
            {
                string amount = utility.Hours;
                if (!double.TryParse(amount, out _))
                {
                    return false; // Invalid non-number value found, return false
                }
            }
            return true; // All amounts are valid numbers
        }
        public static void DeleteFromDatabase(MySqlConnection connection, int ID, string date) {
            // new command
            string query = "DELETE FROM Activities WHERE UserID = '" + ID + "' AND DateTime = '" + date + "'";
            using MySqlCommand command = new(query, connection);
            command.CommandText = query;
            command.ExecuteNonQuery();
        }
        public static void InputTransport(int ID, List<dynamic> transports, MySqlConnection connection, string date, string query) {
            // new command
            using MySqlCommand command = new(query, connection);
            // varaibles that dont change for each unique transport
            int userID = ID;
            const string activityType = "transport";
            Functions calculations = new();

            // add each entry in the table
            foreach (var transport in transports)
            {
                // transportation type
                string method = transport.TransportType;
                // amount of miles driven
                string amount = transport.Miles;

                // calculate emissions with, method (transportation type), and amount (miles driven)
                double emission = calculations.DrivingCalculation(method, amount);

                // insert info in Activities Table
                command.CommandText = query;
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@ActivityType", activityType);
                command.Parameters.AddWithValue("@Method", method);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@DateTime", date);
                command.Parameters.AddWithValue("@Emission", emission);
                command.ExecuteNonQuery();
            }
        }
        public  static void InputFood(int ID, List<dynamic> foods, MySqlConnection connection, string date, string query) {
            // new command
            using MySqlCommand command = new(query, connection);
            // varaibles that dont change for each unique foods
            int userID = ID;
            const string activityType = "foods";
            Functions calculations = new();

            // add each food in foods
            foreach (var food in foods)
            {
                // food type eaten
                string method = food.FoodType;
                // amount of food eaten
                string amount = food.Amount;

                // calculate emissions, method (foodType), amount (servings or grams)
                double emission = calculations.FoodCalculation(method, amount);

                // insert info in Activities Table
                command.CommandText = query;
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@ActivityType", activityType);
                command.Parameters.AddWithValue("@Method", method);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@DateTime", date);
                command.Parameters.AddWithValue("@Emission", emission);
                command.ExecuteNonQuery();
            }
        }
        public static void InputUtilities(int ID, List<dynamic> utilities, MySqlConnection connection, string date, string query) {
            // new command
            using MySqlCommand command = new(query, connection);
            // varaibles that dont change for each unique foods
            int userID = ID;
            const string activityType = "utility";
            Functions calculations = new();

            foreach (var utility in utilities)
            {
                // get the utility type
                string method = utility.UtilityType;
                // get hours used
                string amount = utility.Hours;
                // calculate the utilities emission
                double emission = calculations.UtilitiesCalculation(method, amount);
                if (emission == -1)
                {
                    break;
                }

                // insert info in Activities Table
                command.CommandText = query;
                command.Parameters.AddWithValue("@ID", ID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@ActivityType", activityType);
                command.Parameters.AddWithValue("@Method", method);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@DateTime", date);
                command.Parameters.AddWithValue("@Emission", emission);
                command.ExecuteNonQuery();
            }
        }
        private static void UpdateDailyEmissions(int ID, MySqlConnection connection, string date)
        {
            // get the total Emissions from the day
            double addedTotalEmissions = GetTotalEmissions(ID, connection, date);
            UpdateOrSetDailyEmissions(ID, connection, date, addedTotalEmissions);
        }
        private static double GetTotalEmissions(int ID, MySqlConnection connection, string date)
        {
            // new command
            string query = "SELECT Emission FROM Activities WHERE UserID = '" + ID + "' AND DateTime = '" + date + "'";
            using MySqlCommand command = new(query, connection);

            double totalAddedEmissions = 0;
            // set command and reader at the Users Table
            command.CommandText = query;
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                double currEmission = reader.GetDouble("Emission");
                totalAddedEmissions += currEmission;
            }
            return totalAddedEmissions;
        }
         private static void UpdateOrSetDailyEmissions(int ID, MySqlConnection connection, string date, double addedTotalEmissions)
        {
            // read table DailyEmissions
            string query = "SELECT * FROM DailyEmissions WHERE UserID = '" + ID + "' AND DateTime = '" + date + "'";
            using MySqlCommand command = new(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                // Entry exists, update TotalAmount
                double currentTotalAmount = reader.GetDouble("TotalAmount");
                double newTotalAmount = currentTotalAmount + addedTotalEmissions;

                // Update the TotalAmount for the existing entry
                string updateQuery = "UPDATE DailyEmissions SET TotalAmount = @TotalAmount WHERE UserID = '" + ID + "' AND DateTime = '" + date + "'";

                // command to update table DailyEmissions
                using MySqlCommand updateCommand = new(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@TotalAmount", newTotalAmount);
                updateCommand.Parameters.AddWithValue("@Date", date);
                updateCommand.Parameters.AddWithValue("@ID", ID);
                updateCommand.ExecuteNonQuery();
            }
            else
            {
                // Entry does not exist, insert a new row
                const string insertQuery = "INSERT INTO DailyEmissions (UserID, TotalAmount, Goal, DateTime) VALUES (@UserID, @TotalAmount, @Goal, @DateTime)";

                // command to insert new row to DailyEmissions
                using MySqlCommand insertCommand = new(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@UserID", ID);
                insertCommand.Parameters.AddWithValue("@TotalAmount", addedTotalEmissions);
                // insertCommand.Parameters.AddWithValue("@Goal", goal); // what to do here, let user make goal or set goal from documentation
                insertCommand.Parameters.AddWithValue("@Date", date);
                insertCommand.ExecuteNonQuery();
            }
        }
    }
}