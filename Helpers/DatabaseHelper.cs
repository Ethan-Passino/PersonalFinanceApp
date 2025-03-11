using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace PersonalFinanceApp
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = "Data Source=finance.db;Version=3;";
        private static readonly string categoriesFile = "categories.txt";
        private static readonly List<string> defaultCategories = new List<string>
        {
            "Rent", "Gas", "Food", "Entertainment", "Savings", "Monthly", "Maintenance", "Other"
        };

        /// <summary>
        /// Opens a connection to the SQLite database.
        /// </summary>
        /// <returns>SQLiteConnection</returns>
        private static SQLiteConnection OpenConnection()
        {
            try
            {
                var connection = new SQLiteConnection(connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification("DATABASE FAILED TO OPEN", MainWindow.NotificationType.Critical);
                Console.WriteLine($"Error opening database connection: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <param name="parameters">Optional SQLiteParameters</param>
        public static void ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = OpenConnection())
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a query and returns a DataTable.
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <param name="parameters">Optional SQLiteParameters</param>
        /// <returns>DataTable with query results</returns>
        public static DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = OpenConnection())
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a scalar query and returns the result.
        /// </summary>
        /// <param name="query">SQL query string</param>
        /// <param name="parameters">Optional SQLiteParameters</param>
        /// <returns>Single object result</returns>
        public static object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = OpenConnection())
            {
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }
                    return command.ExecuteScalar();
                }
            }
        }


        /// <summary>
        /// Initializes the database by creating required tables if they do not exist.
        /// </summary>
        public static void InitializeDatabase()
        {
            string createTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Amount REAL,
                    Category TEXT,
                    Date TEXT,
                    Description TEXT
                );";

            string createPaystubsTable = @"
                CREATE TABLE IF NOT EXISTS Paystubs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Income REAL,
                    Employer TEXT,
                    Description TEXT
                );";
            string createBudgetsTable = @"
                CREATE TABLE IF NOT EXISTS Budgets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT,
                    Amount REAL NOT NULL DEFAULT 0
                  );";

            ExecuteNonQuery(createTransactionsTable);
            ExecuteNonQuery(createPaystubsTable);
            ExecuteNonQuery(createBudgetsTable);

            Console.WriteLine("Database initialized successfully.");
        }

        public static void InitializeCategories()
        {
            if (!File.Exists(categoriesFile))
            {
                File.WriteAllLines(categoriesFile, defaultCategories);
            }
        }

        public static List<string> GetCategories()
        {
            return File.Exists(categoriesFile) ? File.ReadAllLines(categoriesFile).ToList() : new List<string>(defaultCategories);
        }

        public static void AddCategory(string category)
        {
            var categories = GetCategories();
            if (!categories.Contains(category))
            {
                categories.Add(category);
                File.WriteAllLines(categoriesFile, categories);
            }
        }

        public static void RemoveCategory(string category)
        {
            var categories = GetCategories();
            if (categories.Contains(category))
            {
                categories.Remove(category);
                File.WriteAllLines(categoriesFile, categories);
            }
        }
    }
}
