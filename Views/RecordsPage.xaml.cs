using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;
using static PersonalFinanceApp.MainWindow;

namespace PersonalFinanceApp
{
    public partial class RecordsPage : Page
    {
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<Paystub> Paystubs { get; set; }
        private bool isEditingTransaction = false;
        private bool isEditingPaystub = false;

        public RecordsPage()
        {
            InitializeComponent();
            Transactions = new ObservableCollection<Transaction>();
            Paystubs = new ObservableCollection<Paystub>();
            DataContext = this;
            LoadDataFromDatabase();

            TransactionsGrid.ItemsSource = Transactions;
            PaystubsGrid.ItemsSource = Paystubs;
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                // Load Transactions
                string transactionQuery = "SELECT Id, Amount, Category, Date, Description FROM Transactions;";
                var transactionsTable = DatabaseHelper.ExecuteQuery(transactionQuery);

                foreach (DataRow row in transactionsTable.Rows)
                {
                    Transactions.Add(new Transaction
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Amount = row["Amount"].ToString(),
                        Category = row["Category"].ToString(),
                        Date = row["Date"].ToString(),
                        Description = row["Description"].ToString()
                    });
                }

                // Load PayStubs
                string payStubQuery = "SELECT Id, Income, Date, Employer, Description FROM PayStubs;";
                var payStubsTable = DatabaseHelper.ExecuteQuery(payStubQuery);

                foreach (DataRow row in payStubsTable.Rows)
                {
                    Paystubs.Add(new Paystub
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Income = row["Income"].ToString(),
                        Date = row["Date"].ToString(),
                        Employer = row["Employer"].ToString(),
                        Description = row["Description"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error loading data: {ex.Message}", NotificationType.Error);
                Console.WriteLine($"Error: {ex}");
            }
        }

        private bool ValidateTransaction(Transaction transaction, out string errorMessage)
        {
            string[] validCategories = { "Rent", "Gas", "Food", "Entertainment", "Savings", "Monthly", "Maintenance", "Other" };
            if (!Array.Exists(validCategories, category => category == transaction.Category))
            {
                errorMessage = "Invalid category. Please select a valid category.";
                return false;
            }

            if (!DateTime.TryParseExact(transaction.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                errorMessage = "Invalid date format. Please use YYYY-MM-DD.";
                return false;
            }

            if (!double.TryParse(transaction.Amount, out double amount) || amount <= 0)
            {
                errorMessage = "Amount must be a positive number.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool ValidatePaystub(Paystub paystub, out string errorMessage)
        {
            if (!DateTime.TryParseExact(paystub.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                errorMessage = "Invalid date format. Please use YYYY-MM-DD.";
                return false;
            }

            if (!decimal.TryParse(paystub.Income, out decimal income) || income <= 0)
            {
                errorMessage = "Income must be a positive number.";
                return false;
            }

            errorMessage = null;
            return true;
        }



        private void TransactionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (isEditingTransaction) return;

            try
            {
                isEditingTransaction = true;

                // Get the edited row and column
                if (e.Row.Item is Transaction editedTransaction)
                {
                    // Keep a copy of the original transaction
                    var originalTransaction = new Transaction
                    {
                        Category = editedTransaction.Category,
                        Amount = editedTransaction.Amount,
                        Date = editedTransaction.Date,
                        Description = editedTransaction.Description
                    };

                    TransactionsGrid.CommitEdit(DataGridEditingUnit.Row, true);

                    if (!ValidateTransaction(editedTransaction, out string errorMessage))
                    {
                        MainWindow.Instance.ShowNotification(errorMessage, NotificationType.Error);

                        // Revert changes to the original values
                        editedTransaction.Category = originalTransaction.Category;
                        editedTransaction.Amount = originalTransaction.Amount;
                        editedTransaction.Date = originalTransaction.Date;
                        editedTransaction.Description = originalTransaction.Description;

                        TransactionsGrid.Items.Refresh(); // Refresh the grid to show reverted values
                        return;
                    }

                    UpdateTransactionInDatabase(editedTransaction);
                }
            }
            finally
            {
                isEditingTransaction = false;
            }
        }

        private void PaystubsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (isEditingPaystub) return;

            try
            {
                isEditingPaystub = true;

                // Get the edited row and column
                if (e.Row.Item is Paystub editedPaystub)
                {
                    // Keep a copy of the original paystub
                    var originalPaystub = new Paystub
                    {
                        Date = editedPaystub.Date,
                        Income = editedPaystub.Income,
                        Employer = editedPaystub.Employer,
                        Description = editedPaystub.Description
                    };

                    PaystubsGrid.CommitEdit(DataGridEditingUnit.Row, true);

                    if (!ValidatePaystub(editedPaystub, out string errorMessage))
                    {
                        MainWindow.Instance.ShowNotification(errorMessage, NotificationType.Error);

                        // Revert changes to the original values
                        editedPaystub.Date = originalPaystub.Date;
                        editedPaystub.Income = originalPaystub.Income;
                        editedPaystub.Employer = originalPaystub.Employer;
                        editedPaystub.Description = originalPaystub.Description;

                        PaystubsGrid.Items.Refresh(); // Refresh the grid to show reverted values
                        return;
                    }

                    UpdatePaystubInDatabase(editedPaystub);
                }
            }
            finally
            {
                isEditingPaystub = false;
            }
        }




        private void UpdateTransactionInDatabase(Transaction transaction)
        {
            try
            {
                // Convert Amount from string to double for database storage
                if (!double.TryParse(transaction.Amount, out double amount))
                {
                    MainWindow.Instance.ShowNotification("Failed to save transaction: Amount is not a valid number.", NotificationType.Error);
                    return;
                }

                string query = "UPDATE Transactions SET Amount = @Amount, Category = @Category, Date = @Date, Description = @Description WHERE Id = @Id";
                var parameters = new Dictionary<string, object>
        {
            { "@Amount", amount }, // Store as a number
            { "@Category", transaction.Category },
            { "@Date", transaction.Date },
            { "@Description", transaction.Description },
            { "@Id", transaction.Id }
        };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MainWindow.Instance.ShowNotification("Transaction updated successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error updating transaction: {ex.Message}", NotificationType.Error);
                Console.WriteLine($"Error: {ex}");
            }
        }


        private void UpdatePaystubInDatabase(Paystub paystub)
        {
            try
            {
                // Convert Income from string to decimal for database storage
                if (!decimal.TryParse(paystub.Income, out decimal income))
                {
                    MainWindow.Instance.ShowNotification("Failed to save paystub: Income is not a valid number.", NotificationType.Error);
                    return;
                }

                string query = "UPDATE Paystubs SET Income = @Income, Date = @Date, Employer = @Employer, Description = @Description WHERE Id = @Id";
                var parameters = new Dictionary<string, object>
        {
            { "@Income", income }, // Store as a number
            { "@Date", paystub.Date },
            { "@Employer", paystub.Employer },
            { "@Description", paystub.Description },
            { "@Id", paystub.Id }
        };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MainWindow.Instance.ShowNotification("Paystub updated successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error updating paystub: {ex.Message}", NotificationType.Error);
                Console.WriteLine($"Error: {ex}");
            }
        }


        public class Transaction
        {
            public int Id { get; set; }
            public string Category { get; set; }
            public string Amount { get; set; } // Changed to string
            public string Date { get; set; }
            public string Description { get; set; }
        }

        public class Paystub
        {
            public int Id { get; set; }
            public string Date { get; set; }
            public string Income { get; set; } // Changed to string
            public string Employer { get; set; }
            public string Description { get; set; }
        }

    }
}
