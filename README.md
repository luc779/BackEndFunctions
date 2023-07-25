# NoCO2

<p align="center">
  <img width="460" height="300" src="./src/assets/noco2-logo.png">
</p>

NoCO2 is a React app that allows users to monitor their daily CO2 emissions by visualizing their emission history in a line chart. 
Users can input their daily activities through a form, which is then sent to the backend for CO2 emission calculation. The app also 
provides a dashboard page where users can view statistic cards, such as the highest or average emission activity.

##Table of Contents

-[Overview](#overview)
-[Installation](#installation)
-[Usage](#usage)
-[FrontEnd](#frontend)

## Overview 

- Utilizes AWS relational database servers to host user data. (limited to ~750 hours or 20GB of general purpose SSD)
- Implements Azure Functions to run event-triggered code per user action. (limited to 1 million requests)
- Designed database for user profiles, user daily activities, and user history, in Sql.
- Features calculations for 3 emission types with an average of 11 different options per type.
- Emissions data is based of: https://www.co2everything.com/categories
- Backend Design Document: https://docs.google.com/document/d/160lD5WHeMGaCdqcMrERJ9ZNQLIFcvJkMV08khmIhUjY/edit?usp=sharing

To view deployed react page, please navigate to https://tsechapman.github.io/NoCO2/ 

## Installation

To run the NoCO2 app on your local machine, please follow these steps:

1. Clone the repository (frontend):

```bash
https://github.com/luc779/BackEndFunctions.git
```

2. Navigate to the project directory:

```bash
cd NoCO2
```

3. Install the dependencies:

```bash
dotnet install
```

4. Create database instance:

Use whichever database host to create a database instance and use the backend documentation to create the 3 tables using MySqlWorkBench.

5. Define correct connection string variables. Create a `DatabaseConnector.cs` file. Then, add the following evironment variabls to your backend (Replace `...` accordingly).

```
using MySqlConnector;

namespace DatabaseConnector;

public static class DatabaseConnecter
{
    private static MySqlConnection connection;
    private static string server;
    private static string database;
    private static string uid;
    private static string password;

    public static MySqlConnection MySQLDatabase()
    {
        server = "...";
        database = "...";
        uid = "...";
        password = "...";
        string connectionString = "server=" + server + ";uid=" + uid +";pwd=" + password + ";database=" + database;

        connection = new MySqlConnection(connectionString);
        return connection;
    }
}
```
Connect to database, using MySqlWorkBench to visualize the data within the database.

You can now run all tests for main utilities,and for functions and their utilities. All tests can be run individually, per file, or all using `dotnet test`.

## Frontend

The NoCO2 app frontend is locatied on a different repository. The frontend isbuilt using JavaScript and styled using Tailwind CSS. For more information, please check https://github.com/TseChapman/NoCO2
