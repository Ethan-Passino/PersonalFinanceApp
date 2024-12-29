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

            // Dynamically set the window size to 80% of the screen size
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Set window dimensions as a percentage of screen size
            this.Width = screenWidth * 0.8;
            this.Height = screenHeight * 0.8;

            // Optionally, center the window on the screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
