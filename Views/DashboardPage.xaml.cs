using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static PersonalFinanceApp.MainWindow;

namespace PersonalFinanceApp
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            CalculateTotals();
            LoadRecentTransactions();
            LoadExpenseBreakdown();
        }

        private void LoadRecentTransactions()
        {
            try
            {
                // Fetch recent transactions from the database (limit to 50)
                string recentTransactionsQuery = "SELECT Category, Amount, Date, Description FROM Transactions ORDER BY Date DESC LIMIT 50;";
                var transactionsTable = DatabaseHelper.ExecuteQuery(recentTransactionsQuery);

                // Clear any existing mock data
                RecentActivityList.Children.Clear();

                // Populate the recent transactions list
                foreach (DataRow row in transactionsTable.Rows)
                {
                    var transactionItem = new Border
                    {
                        Background = System.Windows.Media.Brushes.Gray,
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(10),
                        Margin = new Thickness(5)
                    };

                    var transactionDetails = new StackPanel();
                    transactionDetails.Children.Add(new TextBlock
                    {
                        Text = $"{row["Category"]} - ${row["Amount"]:0.00}",
                        Foreground = System.Windows.Media.Brushes.White,
                        FontSize = 14
                    });
                    transactionDetails.Children.Add(new TextBlock
                    {
                        Text = row["Date"].ToString(),
                        Foreground = System.Windows.Media.Brushes.LightGray,
                        FontSize = 12
                    });

                    transactionItem.Child = transactionDetails;

                    // Add hover event to display description
                    transactionItem.MouseEnter += (s, e) =>
                    {
                        PopupText.Text = row["Description"].ToString();
                        TransactionPopup.IsOpen = true;
                    };
                    transactionItem.MouseLeave += (s, e) =>
                    {
                        TransactionPopup.IsOpen = false;
                    };

                    RecentActivityList.Children.Add(transactionItem);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error loading recent transactions: {ex.Message}", NotificationType.Error);
                Console.WriteLine($"Error: {ex}");
            }
        }


        private void LoadExpenseBreakdown()
        {
            try
            {
                // Step 1: Retrieve all transactions from the database
                string transactionQuery = "SELECT Category, Amount, Date FROM Transactions;";
                var transactionsTable = DatabaseHelper.ExecuteQuery(transactionQuery);

                // Step 2: Create a list of transactions for further processing
                var transactions = new List<Transaction>();
                foreach (DataRow row in transactionsTable.Rows)
                {
                    try
                    {
                        // Parse transaction details
                        var transaction = new Transaction
                        {
                            Category = row["Category"].ToString(),
                            Amount = Convert.ToDouble(row["Amount"]),
                            Date = row["Date"].ToString()
                        };
                        transactions.Add(transaction);
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"Error parsing transaction row: {parseEx.Message}");
                    }
                }

                // Step 3: Filter transactions for the last month
                var lastMonthTransactions = transactions
                    .Where(t => DateTime.TryParse(t.Date, out var date)
                                && date >= DateTime.Now.AddMonths(-1).Date
                                && date < DateTime.Now.Date)
                    .ToList();

                // Step 4: Group by category and sum amounts
                var categoryTotals = lastMonthTransactions
                    .GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                // Step 5: Predefined categories with default values
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

                // Update categories with actual data
                foreach (var categoryTotal in categoryTotals)
                {
                    if (categories.ContainsKey(categoryTotal.Key))
                    {
                        categories[categoryTotal.Key] = categoryTotal.Value;
                    }
                }

                // Step 6: Check if there's data to display
                if (categories.Values.All(value => value == 0))
                {
                    ExpenseBreakdownChart.Visibility = Visibility.Collapsed;
                    NoDataMessage.Visibility = Visibility.Visible;
                    Console.WriteLine("No data available for expense breakdown.");
                    return;
                }

                // Step 7: Populate PieChart
                ExpenseBreakdownChart.Visibility = Visibility.Visible;
                NoDataMessage.Visibility = Visibility.Collapsed;

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
                foreach (var category in categories.Where(c => c.Value > 0))
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
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error loading expense breakdown: {ex.Message}", NotificationType.Error);
                Console.WriteLine($"Error: {ex}");
            }
        }

        private void CalculateTotals()
        {
            try
            {
                // Fetch all transactions
                string expenseQuery = "SELECT SUM(Amount) as TotalExpenses FROM Transactions;";
                var expenseResult = DatabaseHelper.ExecuteScalar(expenseQuery);

                // Fetch all paystubs
                string incomeQuery = "SELECT SUM(Income) as TotalIncome FROM Paystubs;";
                var incomeResult = DatabaseHelper.ExecuteScalar(incomeQuery);

                // Parse results
                double totalExpenses = expenseResult != DBNull.Value ? Convert.ToDouble(expenseResult) : 0.0;
                double totalIncome = incomeResult != DBNull.Value ? Convert.ToDouble(incomeResult) : 0.0;

                // Calculate Net Income
                double netIncome = totalIncome - totalExpenses;

                // Display totals
                TotalExpenses.Text = $"${totalExpenses:0.00}";
                TotalIncome.Text = $"${totalIncome:0.00}";
                NetIncome.Text = $"${netIncome:0.00}";
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error calculating totals: {ex.Message}", NotificationType.Error);
                Console.WriteLine($"Error: {ex}");
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
