using System;
using System.Windows;
using System.Windows.Controls;
using static PersonalFinanceApp.MainWindow;

namespace PersonalFinanceApp
{
    /// <summary>
    /// Interaction logic for PayStubPage.xaml
    /// </summary>
    public partial class PayStubPage : Page
    {
        public PayStubPage()
        {
            InitializeComponent();
        }

        private void SavePayStub(object sender, RoutedEventArgs e)
        {
            // Reset all field borders
            IncomeAmountInput.BorderBrush = System.Windows.Media.Brushes.Gray;
            IncomeAmountInput.BorderThickness = new Thickness(1);

            IncomeDatePicker.BorderBrush = System.Windows.Media.Brushes.Gray;
            IncomeDatePicker.BorderThickness = new Thickness(1);

            EmployerInput.BorderBrush = System.Windows.Media.Brushes.Gray;
            EmployerInput.BorderThickness = new Thickness(1);

            // Fetch input values
            string incomeAmount = IncomeAmountInput.Text;
            DateTime? selectedDate = IncomeDatePicker.SelectedDate;
            string employer = EmployerInput.Text;
            string description = IncomeDescriptionInput.Text;

            // Validate income amount
            if (string.IsNullOrWhiteSpace(incomeAmount) || !decimal.TryParse(incomeAmount, out decimal income))
            {
                IncomeAmountInput.BorderBrush = System.Windows.Media.Brushes.Red;
                IncomeAmountInput.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please enter a valid income amount.", NotificationType.Error);
                return;
            }

            // Validate date
            if (selectedDate == null)
            {
                IncomeDatePicker.BorderBrush = System.Windows.Media.Brushes.Red;
                IncomeDatePicker.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please select a date.", NotificationType.Error);
                return;
            }

            // Validate employer/source
            if (string.IsNullOrWhiteSpace(employer))
            {
                EmployerInput.BorderBrush = System.Windows.Media.Brushes.Red;
                EmployerInput.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please enter the employer/source.", NotificationType.Error);
                return;
            }

            // Save to database
            try
            {
                string query = @"
            INSERT INTO PayStubs (Income, Date, Employer, Description)
            VALUES (@Income, @Date, @Employer, @Description);";

                var parameters = new System.Collections.Generic.Dictionary<string, object>
        {
            { "@Income", income },
            { "@Date", selectedDate.Value.ToString("yyyy-MM-dd") },
            { "@Employer", employer },
            { "@Description", description }
        };

                DatabaseHelper.ExecuteNonQuery(query, parameters);

                // Clear form and show success notification
                ClearForm();
                MainWindow.Instance.ShowNotification("Pay stub saved successfully!", NotificationType.Success);
            }
            catch (Exception ex)
            {
                // Show error notification
                MainWindow.Instance.ShowNotification($"Error saving pay stub: {ex.Message}", NotificationType.Error);
            }
        }

        private void ClearForm()
        {
            IncomeAmountInput.Text = string.Empty;
            IncomeDatePicker.SelectedDate = null;
            EmployerInput.Text = string.Empty;
            IncomeDescriptionInput.Text = string.Empty;
        }

        private void CancelPayStub(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
    }
}
