﻿using System;
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
            LoadCategories();
        }

        private void LoadCategories()
        {
            CategoryDropdown.Items.Clear();
            foreach (var category in DatabaseHelper.GetCategories())
            {
                CategoryDropdown.Items.Add(new ComboBoxItem { Content = category });
            }
        }

        private void SaveTransaction(object sender, RoutedEventArgs e)
        {
            // Reset all borders to default at the start
            AmountInput.BorderBrush = System.Windows.Media.Brushes.Gray;
            AmountInput.BorderThickness = new Thickness(1);

            CategoryDropdown.BorderBrush = System.Windows.Media.Brushes.Gray;
            CategoryDropdown.BorderThickness = new Thickness(1);

            TransactionDate.BorderBrush = System.Windows.Media.Brushes.Gray;
            TransactionDate.BorderThickness = new Thickness(1);

            // Retrieve form data
            string amountText = AmountInput.Text;
            string category = (CategoryDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime? date = TransactionDate.SelectedDate;
            string description = DescriptionInput.Text;

            // Validate amount
            if (string.IsNullOrWhiteSpace(amountText) || !decimal.TryParse(amountText, out decimal amount))
            {
                AmountInput.BorderBrush = System.Windows.Media.Brushes.Red;
                AmountInput.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please enter a valid amount.", MainWindow.NotificationType.Error);
                return;
            }

            // Validate category
            if (category == null)
            {
                CategoryDropdown.BorderBrush = System.Windows.Media.Brushes.Red;
                CategoryDropdown.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please select a category.", MainWindow.NotificationType.Error);
                return;
            }

            // Validate date
            if (date == null)
            {
                TransactionDate.BorderBrush = System.Windows.Media.Brushes.Red;
                TransactionDate.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please select a date.", MainWindow.NotificationType.Error);
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
                MainWindow.Instance.ShowNotification("Transaction saved successfully!", MainWindow.NotificationType.Success);
            }
            catch(Exception ex)
            {
                MainWindow.Instance.ShowNotification("Transaction couldn't be saved due to an error.", MainWindow.NotificationType.Error);
                Console.WriteLine($"{ex.Message}");
            }
        }

        private void CancelTransaction(object sender, RoutedEventArgs e)
        {
            ClearForm();
            MainWindow.Instance.ShowNotification("Transaction canceled.", MainWindow.NotificationType.Info);
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
