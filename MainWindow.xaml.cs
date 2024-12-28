using System.Windows;
using System.Windows.Controls;

namespace PersonalFinanceApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load the Dashboard by default
            DynamicContentFrame.Navigate(new DashboardPage());
        }

        private void NavigateToDashboard(object sender, RoutedEventArgs e)
        {
            DynamicContentFrame.Navigate(new DashboardPage());
        }

        private void NavigateToTransactions(object sender, RoutedEventArgs e)
        {
            DynamicContentFrame.Navigate(new TransactionsPage());
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            DynamicContentFrame.Navigate(new ReportsPage());
        }

        private void NavigateToBudgets(object sender, RoutedEventArgs e)
        {
            DynamicContentFrame.Navigate(new BudgetsPage());
        }
    }
}
