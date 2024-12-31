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
            try
            {
                string query = @"
                    INSERT INTO Transactions (Amount, Category, Date, Description)
                    VALUES (@Amount, @Category, @Date, @Description);";
                var parameters = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "@Amount", amount },
                        { "@Category", category },
                        { "@Date", date.Value.ToString("yyyy-MM-dd") },
                        { "@Description", description }
                    };

                DatabaseHelper.ExecuteQuery(query, parameters);

                // Clear form and confirm success
                ClearForm();
                MainWindow.Instance.ShowNotification("Transaction saved successfully!", "Success");
            }
            catch(Exception ex)
            {
                MainWindow.Instance.ShowNotification("Transaction couldn't be saved due to an error.", "Error");
                Console.WriteLine($"{ex.Message}");
            }
        }

        private void CancelTransaction(object sender, RoutedEventArgs e)
        {
            ClearForm();
            MainWindow.Instance.ShowNotification("Transaction canceled.", "Info");
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
