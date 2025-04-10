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
            string createCategoriesTable = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE NOT NULL
                );";

            ExecuteNonQuery(createTransactionsTable);
            ExecuteNonQuery(createPaystubsTable);
            ExecuteNonQuery(createBudgetsTable);
            ExecuteNonQuery(createCategoriesTable);


            // Check if there are no categories, if there aren't populate it with the default categories.
            if (GetCategories().Count == 0)
            {
                InitializeCategories();
            }

            Console.WriteLine("Database initialized successfully.");
        }

        private static void InitializeCategories()
        {
            foreach (var category in defaultCategories)
            {
                AddCategory(category);
            }
        }

        public static List<string> GetCategories()
        {
            List<string> categories = new List<string>();
            string query = "SELECT Name FROM Categories";
            DataTable dt = ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                categories.Add(row["Name"].ToString());
            }
            return categories;
        }

        public static void AddCategory(string category)
        {
            string query = "INSERT OR IGNORE INTO Categories (Name) VALUES (@Name)";
            var parameters = new Dictionary<string, object> { { "@Name", category } };
            ExecuteNonQuery(query, parameters);
        }

        public static void RenameCategory(string oldName, string newName)
        {
            string checkIfExists = "SELECT COUNT(*) FROM Categories WHERE Name = @NewName";
            var checkParams = new Dictionary<string, object> { { "@NewName", newName } };
            int exists = Convert.ToInt32(ExecuteScalar(checkIfExists, checkParams));

            if (exists > 0)
            {
                throw new InvalidOperationException("A category with that name already exists.");
            }

            string updateTransactions = "UPDATE Transactions SET Category = @NewName WHERE Category = @OldName";
            string updateBudgets = "UPDATE Budgets SET Category = @NewName WHERE Category = @OldName";
            string updateCategory = "UPDATE Categories SET Name = @NewName WHERE Name = @OldName";

            var parameters = new Dictionary<string, object>
    {
        { "@OldName", oldName },
        { "@NewName", newName }
    };

            ExecuteNonQuery(updateTransactions, parameters);
            ExecuteNonQuery(updateBudgets, parameters);
            ExecuteNonQuery(updateCategory, parameters);
        }


        public static void RemoveCategory(string category)
        {
            string otherCategoryQuery = "SELECT COUNT(*) FROM Categories WHERE Name = 'Other'";
            int otherCategoryExists = Convert.ToInt32(ExecuteScalar(otherCategoryQuery));

            if (otherCategoryExists == 0)
            {
                AddCategory("Other");
            }

            string updateTransactionsQuery = "UPDATE Transactions SET Category = 'Other' WHERE Category = @Category";
            var parameters = new Dictionary<string, object> { { "@Category", category } };
            ExecuteNonQuery(updateTransactionsQuery, parameters);

            string deleteBudgetQuery = "DELETE FROM Budgets WHERE Category = @Category";
            ExecuteNonQuery(deleteBudgetQuery, parameters);

            string deleteCategoryQuery = "DELETE FROM Categories WHERE Name = @Category";
            ExecuteNonQuery(deleteCategoryQuery, parameters);
        }
    }
}
