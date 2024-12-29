using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace PersonalFinanceApp
{
    public partial class TransactionsPage : Page
    {
        public TransactionsPage()
        {
            InitializeComponent();
        }

        private void SaveTransaction(object sender, RoutedEventArgs e)
        {
            // Retrieve form data
            string amountText = AmountInput.Text;
            string category = (CategoryDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime? date = TransactionDate.SelectedDate;
            string description = DescriptionInput.Text;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(amountText) || !decimal.TryParse(amountText, out decimal amount))
            {
                MessageBox.Show("Please enter a valid amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (category == null)
            {
                MessageBox.Show("Please select a category.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (date == null)
            {
                MessageBox.Show("Please select a date.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Save to SQLite database
            string connectionString = "Data Source=PersonalFinance.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(connection)
                {
                    CommandText = @"
                        INSERT INTO Transactions (Amount, Category, Date, Description)
                        VALUES (@amount, @category, @date, @description);
                    "
                };
                command.Parameters.AddWithValue("@amount", amount);
                command.Parameters.AddWithValue("@category", category);
                command.Parameters.AddWithValue("@date", date.Value.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@description", description);
                command.ExecuteNonQuery();
            }

            // Clear form and confirm success
            ClearForm();
            MessageBox.Show("Transaction saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelTransaction(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            AmountInput.Text = string.Empty;
            CategoryDropdown.SelectedIndex = -1;
            TransactionDate.SelectedDate = null;
            DescriptionInput.Text = string.Empty;
        }
    }
}
