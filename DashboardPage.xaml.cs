using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PersonalFinanceApp
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadMockData();
        }

        private void LoadMockData()
        {
            // Mock Data for Total Income and Expenses
            TotalIncome.Text = "$5000.00";
            TotalExpenses.Text = "$3200.00";

            // Add a large number of mock transactions
            var recentTransactions = new List<Transaction>();
            for (int i = 1; i <= 50; i++)
            {
                recentTransactions.Add(new Transaction
                {
                    Category = "Rent",
                    Amount = 1200,
                    Date = $"01/{i % 30 + 1}/2024",
                    Description = $"Monthly rent payment for January {i}"
                });
                recentTransactions.Add(new Transaction
                {
                    Category = "Groceries",
                    Amount = 250,
                    Date = $"01/{i % 30 + 1}/2024",
                    Description = $"Grocery shopping for week {i}"
                });
                recentTransactions.Add(new Transaction
                {
                    Category = "Entertainment",
                    Amount = 100,
                    Date = $"01/{i % 30 + 1}/2024",
                    Description = $"Entertainment expenses for event {i}"
                });
                recentTransactions.Add(new Transaction
                {
                    Category = "Gas",
                    Amount = 50,
                    Date = $"01/{i % 30 + 1}/2024",
                    Description = $"Fuel for the car trip {i}"
                });
            }

            // Adding transactions to the scrollable list
            foreach (var transaction in recentTransactions)
            {
                var transactionItem = new Border
                {
                    Background = System.Windows.Media.Brushes.Gray,
                    CornerRadius = new System.Windows.CornerRadius(5),
                    Padding = new Thickness(10),
                    Margin = new Thickness(5)
                };

                var transactionDetails = new StackPanel();
                transactionDetails.Children.Add(new TextBlock
                {
                    Text = $"{transaction.Category} - ${transaction.Amount}",
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14
                });
                transactionDetails.Children.Add(new TextBlock
                {
                    Text = transaction.Date,
                    Foreground = System.Windows.Media.Brushes.LightGray,
                    FontSize = 12
                });

                transactionItem.Child = transactionDetails;

                // Add hover event to display description
                transactionItem.MouseEnter += (s, e) =>
                {
                    PopupText.Text = transaction.Description;
                    TransactionPopup.IsOpen = true;
                };
                transactionItem.MouseLeave += (s, e) =>
                {
                    TransactionPopup.IsOpen = false;
                };

                RecentActivityList.Children.Add(transactionItem);
            }

            // Predefined categories for Pie Chart
            var categories = new Dictionary<string, double>
            {
                { "Rent", 0 },
                { "Gas", 0 },
                { "Food", 0 },
                { "Entertainment", 0 },
                { "Savings", 0 },
                { "Monthly", 0 },
                { "Maintenance", 0 },
                { "Other", 0 }
            };

            foreach (var transaction in recentTransactions)
            {
                if (categories.ContainsKey(transaction.Category))
                {
                    categories[transaction.Category] += transaction.Amount;
                }
            }

            // Pie Chart Colors
            var colors = new[]
            {
                System.Windows.Media.Brushes.LightGreen,
                System.Windows.Media.Brushes.Orange,
                System.Windows.Media.Brushes.LightBlue,
                System.Windows.Media.Brushes.Purple,
                System.Windows.Media.Brushes.Red,
                System.Windows.Media.Brushes.Yellow,
                System.Windows.Media.Brushes.Cyan,
                System.Windows.Media.Brushes.Magenta
            };

            ExpenseBreakdownChart.Series = new LiveCharts.SeriesCollection();

            int colorIndex = 0;
            foreach (var category in categories)
            {
                ExpenseBreakdownChart.Series.Add(new LiveCharts.Wpf.PieSeries
                {
                    Title = category.Key,
                    Values = new LiveCharts.ChartValues<double> { category.Value },
                    Fill = colors[colorIndex % colors.Length]
                });
                colorIndex++;
            }
        }

        public class Transaction
        {
            public string Category { get; set; }
            public double Amount { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
        }
    }
}
