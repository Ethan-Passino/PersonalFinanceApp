using System.Windows;
using System.Windows.Controls;

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
            // Fetch input values
            string incomeAmount = IncomeAmountInput.Text;
            string date = IncomeDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "No Date";
            string employer = EmployerInput.Text;
            string description = IncomeDescriptionInput.Text;

            // Save to database or handle the logic (placeholder)
            MessageBox.Show($"Saved Pay Stub:\n\nIncome: {incomeAmount}\nDate: {date}\nEmployer: {employer}\nDescription: {description}");
        }

        private void CancelPayStub(object sender, RoutedEventArgs e)
        {
            // Navigate back to the main page (or previous page)
            NavigationService?.GoBack();
        }
    }
}
