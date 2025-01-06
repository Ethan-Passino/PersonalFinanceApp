using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using static PersonalFinanceApp.MainWindow;

namespace PersonalFinanceApp
{ 
    public partial class ReportsPage : Page
    {
        private List<Transaction> Transactions;
        private List<Paystub> Paystubs;

        public ReportsPage()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                Transactions = LoadTransactions();
                Paystubs = LoadPaystubs();

                // If dates are not already set, default to the past month
                if (ReportsPageState.StartDate == null || ReportsPageState.EndDate == null)
                {
                    var today = DateTime.Today;
                    ReportsPageState.EndDate = today;
                    ReportsPageState.StartDate = today.AddMonths(-1); // Start date is one month earlier
                }

                // Set the date pickers to the stored dates
                StartDatePicker.SelectedDate = ReportsPageState.StartDate;
                EndDatePicker.SelectedDate = ReportsPageState.EndDate;

                // Update reports with the selected range
                UpdateReports();

                if(Paystubs.Count == 0 && Transactions.Count == 0)
                {
                    MainWindow.Instance.ShowNotification("There is no data to process.", NotificationType.Warning);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification("Error loading data. Check console for details.", NotificationType.Error);
                Console.WriteLine(ex.Message);
            }
        }



        private List<Transaction> LoadTransactions()
        {
            try
            {
                var query = "SELECT Id, Category, Amount, Date, Description FROM Transactions";
                var transactionsTable = DatabaseHelper.ExecuteQuery(query);

                return transactionsTable.AsEnumerable().Select(row => new Transaction
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Category = row["Category"].ToString(),
                    Amount = row["Amount"].ToString(),
                    Date = DateTime.Parse(row["Date"].ToString()), // Convert to DateTime
                    Description = row["Description"].ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}");
                throw;
            }
        }

        private List<Paystub> LoadPaystubs()
        {
            try
            {
                var query = "SELECT Id, Date, Income, Employer, Description FROM Paystubs";
                var paystubsTable = DatabaseHelper.ExecuteQuery(query);

                return paystubsTable.AsEnumerable().Select(row => new Paystub
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Date = DateTime.Parse(row["Date"].ToString()), // Convert to DateTime
                    Income = row["Income"].ToString(),
                    Employer = row["Employer"].ToString(),
                    Description = row["Description"].ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading paystubs: {ex.Message}");
                throw;
            }
        }

        private void UpdateReports()
        {
            try
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MainWindow.Instance.ShowNotification("Please select a valid date range.", NotificationType.Warning);
                    return;
                }

                DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
                DateTime endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

                if (startDate > endDate)
                {
                    MainWindow.Instance.ShowNotification("Start date cannot be after end date.", NotificationType.Error);
                    return;
                }

                var filteredTransactions = Transactions
                    .Where(t => t.Date >= startDate && t.Date <= endDate)
                    .ToList();

                var filteredPaystubs = Paystubs
                    .Where(p => p.Date >= startDate && p.Date <= endDate)
                    .ToList();

                double totalIncome = filteredPaystubs.Sum(p => double.Parse(p.Income));
                double totalExpenses = filteredTransactions.Sum(t => double.Parse(t.Amount));
                double netIncome = totalIncome - totalExpenses;

                TotalIncome.Text = $"${totalIncome:0.00}";
                TotalExpenses.Text = $"${totalExpenses:0.00}";
                NetIncome.Text = $"${netIncome:0.00}";

                UpdateIncomeExpensesChart(filteredTransactions, filteredPaystubs);
                UpdateExpenseBreakdownChart(filteredTransactions);
                UpdateMonthlyAveragesChart(filteredTransactions, filteredPaystubs);
                UpdateExpenseDistributionChart(filteredTransactions);
                UpdateExpenseHeatmap(filteredTransactions);

            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification("An error occurred while updating the reports.", NotificationType.Error);
                Console.WriteLine($"Error in UpdateReports: {ex.Message}");
            }
        }

        private void OnDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDatePicker.SelectedDate != null && EndDatePicker.SelectedDate != null)
            {
                ReportsPageState.StartDate = StartDatePicker.SelectedDate;
                ReportsPageState.EndDate = EndDatePicker.SelectedDate;
                UpdateReports();
            }
        }



        private void UpdateIncomeExpensesChart(List<Transaction> transactions, List<Paystub> paystubs)
        {
            try
            {
                var incomeData = paystubs
                    .GroupBy(p => p.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(p => double.Parse(p.Income)));

                var expenseData = transactions
                    .GroupBy(t => t.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(t => double.Parse(t.Amount)));

                var allDates = incomeData.Keys.Union(expenseData.Keys).OrderBy(date => date).ToList();

                var incomeValues = new ChartValues<double>();
                var expenseValues = new ChartValues<double>();

                foreach (var date in allDates)
                {
                    incomeValues.Add(incomeData.ContainsKey(date) ? incomeData[date] : 0);
                    expenseValues.Add(expenseData.ContainsKey(date) ? expenseData[date] : 0);
                }

                IncomeExpensesChart.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Income",
                        Values = incomeValues,
                        StrokeThickness = 2,
                        Fill = System.Windows.Media.Brushes.Transparent,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 5,
                        Stroke = System.Windows.Media.Brushes.LightGreen
                    },
                    new LineSeries
                    {
                        Title = "Expenses",
                        Values = expenseValues,
                        StrokeThickness = 2,
                        Fill = System.Windows.Media.Brushes.Transparent,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 5,
                        Stroke = System.Windows.Media.Brushes.Red
                    }
                };

                IncomeExpensesChart.AxisX.Clear();
                IncomeExpensesChart.AxisX.Add(new Axis
                {
                    Title = "Date",
                    Labels = allDates.Select(date => date.ToString("MMM dd")).ToList(),
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                });

                IncomeExpensesChart.AxisY.Clear();
                IncomeExpensesChart.AxisY.Add(new Axis
                {
                    Title = "Amount ($)",
                    LabelFormatter = value => $"${value:N2}",
                    MinValue = 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateIncomeExpensesChart: {ex.Message}");
                MainWindow.Instance.ShowNotification("An error occurred while updating the income vs expenses chart.", NotificationType.Error);
            }
        }

        private void UpdateExpenseBreakdownChart(List<Transaction> transactions)
        {
            try
            {
                // Group transactions by category and calculate total amounts
                var categoryTotals = transactions
                    .GroupBy(t => t.Category)
                    .Select(g => new { Category = g.Key, Total = g.Sum(t => double.Parse(t.Amount)) })
                    .Where(g => g.Total > 0) // Only include categories with non-zero totals
                    .OrderByDescending(g => g.Total);

                // Clear previous chart data
                ExpenseBreakdownChart.Series = new SeriesCollection();

                // Populate the PieChart with the calculated data
                foreach (var category in categoryTotals)
                {
                    ExpenseBreakdownChart.Series.Add(new PieSeries
                    {
                        Title = category.Category,
                        Values = new ChartValues<double> { category.Total },
                        DataLabels = true // Enable labels to show values on the chart
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateExpenseBreakdownChart: {ex.Message}");
                MainWindow.Instance.ShowNotification("An error occurred while updating the expense breakdown chart.", NotificationType.Error);
            }
        }

        private void UpdateMonthlyAveragesChart(List<Transaction> transactions, List<Paystub> paystubs)
        {
            var monthlyIncome = paystubs
                .GroupBy(p => new { p.Date.Year, p.Date.Month })
                .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:00}", AverageIncome = g.Average(p => double.Parse(p.Income)) })
                .ToList();

            var monthlyExpenses = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:00}", AverageExpense = g.Average(t => double.Parse(t.Amount)) })
                .ToList();

            var allMonths = monthlyIncome.Select(mi => mi.Month).Union(monthlyExpenses.Select(me => me.Month)).Distinct().OrderBy(m => m).ToList();

            var incomeValues = new ChartValues<double>();
            var expenseValues = new ChartValues<double>();

            foreach (var month in allMonths)
            {
                incomeValues.Add(monthlyIncome.FirstOrDefault(mi => mi.Month == month)?.AverageIncome ?? 0);
                expenseValues.Add(monthlyExpenses.FirstOrDefault(me => me.Month == month)?.AverageExpense ?? 0);
            }

            MonthlyAveragesChart.Series = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Income",
            Values = incomeValues,
            Fill = System.Windows.Media.Brushes.LightGreen
        },
        new ColumnSeries
        {
            Title = "Expenses",
            Values = expenseValues,
            Fill = System.Windows.Media.Brushes.Red
        }
    };

            MonthlyAveragesChart.AxisX[0].Labels = allMonths;
        }

        private void UpdateExpenseDistributionChart(List<Transaction> transactions)
        {
            var categoryMonthlyTotals = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month, t.Category })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:00}",
                    Category = g.Key.Category,
                    Total = g.Sum(t => double.TryParse(t.Amount, out var amount) ? amount : 0)
                })
                .GroupBy(g => g.Month)
                .ToDictionary(g => g.Key, g => g.ToDictionary(c => c.Category, c => c.Total));

            var allMonths = categoryMonthlyTotals.Keys.OrderBy(m => m).ToList();
            var categories = categoryMonthlyTotals.Values.SelectMany(dict => dict.Keys).Distinct().ToList();

            var seriesCollection = new SeriesCollection();
            foreach (var category in categories)
            {
                var values = new ChartValues<double>();
                foreach (var month in allMonths)
                {
                    values.Add(categoryMonthlyTotals.ContainsKey(month) && categoryMonthlyTotals[month].ContainsKey(category)
                        ? categoryMonthlyTotals[month][category]
                        : 0);
                }

                seriesCollection.Add(new StackedColumnSeries
                {
                    Title = category,
                    Values = values
                });
            }

            ExpenseDistributionChart.Series = seriesCollection;

            ExpenseDistributionChart.AxisX.Clear();
            ExpenseDistributionChart.AxisX.Add(new Axis
            {
                Title = "Month",
                Labels = allMonths
            });

            ExpenseDistributionChart.AxisY.Clear();
            ExpenseDistributionChart.AxisY.Add(new Axis
            {
                Title = "Amount ($)",
                LabelFormatter = value => $"${value:N2}"
            });
        }

        private void UpdateExpenseHeatmap(List<Transaction> transactions)
        {
            try
            {
                // Group transactions by month and day
                var groupedData = transactions
                    .GroupBy(t =>
                    {
                        var date = (t.Date); // Parse Date
                        return new { Month = date.Month, Day = date.Day };
                    })
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(t =>
                        {
                            // Parse Amount and handle invalid values
                            if (double.TryParse(t.Amount, out double amount))
                                return amount;
                            return 0; // Default to 0 if parsing fails
                        })
                    );

                // Define rows and columns for the heatmap
                var months = groupedData.Keys.Select(k => k.Month).Distinct().OrderBy(m => m).ToList();
                var days = Enumerable.Range(1, 31).ToList();

                // Initialize heatmap data
                var heatmapSeries = new LiveCharts.SeriesCollection();

                foreach (var month in months)
                {
                    var values = new ChartValues<double>();
                    foreach (var day in days)
                    {
                        // Add expense data or 0 if no data for the day
                        var key = new { Month = month, Day = day };
                        values.Add(groupedData.ContainsKey(key) ? groupedData[key] : 0);
                    }

                    heatmapSeries.Add(new ColumnSeries
                    {
                        Title = new DateTime(2025, month, 1).ToString("MMM"),
                        Values = values,
                        StrokeThickness = 0, // No border for cells
                        Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 168)) // Base color
                    });
                }

                // Set the series and configure the axes
                ExpenseHeatmap.Series = heatmapSeries;

                // X-axis: Days of the month
                ExpenseHeatmap.AxisX.Clear();
                ExpenseHeatmap.AxisX.Add(new Axis
                {
                    Labels = days.Select(d => d.ToString()).ToList(),
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                });

                // Y-axis: Months
                ExpenseHeatmap.AxisY.Clear();
                ExpenseHeatmap.AxisY.Add(new Axis
                {
                    Labels = months.Select(m => new DateTime(2025, m, 1).ToString("MMM")).ToList()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateExpenseHeatmap: {ex.Message}");
                MainWindow.Instance.ShowNotification("An error occurred while updating the expense heatmap.", NotificationType.Error);
            }
        }






        private void OnDownloadCsv(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MainWindow.Instance.ShowNotification("Please select a valid date range.", NotificationType.Warning);
                    return;
                }

                DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
                DateTime endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

                var filteredTransactions = Transactions.Where(t => t.Date >= startDate && t.Date <= endDate).ToList();
                var filteredPaystubs = Paystubs.Where(p => p.Date >= startDate && p.Date <= endDate).ToList();

                if (!filteredTransactions.Any() && !filteredPaystubs.Any())
                {
                    MainWindow.Instance.ShowNotification("No data available for the selected date range.", NotificationType.Warning);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Save Report Data",
                    FileName = $"Report_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string csvData = GenerateCsvData(filteredTransactions, filteredPaystubs);
                    File.WriteAllText(saveFileDialog.FileName, csvData);
                    MainWindow.Instance.ShowNotification("CSV file saved successfully!", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDownloadCsv: {ex.Message}");
                MainWindow.Instance.ShowNotification("An error occurred while downloading the CSV.", NotificationType.Error);
            }
        }

        private string GenerateCsvData(List<Transaction> transactions, List<Paystub> paystubs)
        {
            var csv = new List<string>
            {
                "Transactions:",
                "Id,Category,Amount,Date,Description"
            };

            csv.AddRange(transactions.Select(t =>
                $"{t.Id},{t.Category},{t.Amount},{t.Date:yyyy-MM-dd},{t.Description}"));

            csv.Add(string.Empty);

            csv.Add("Paystubs:");
            csv.Add("Id,Date,Income,Employer,Description");
            csv.AddRange(paystubs.Select(p =>
                $"{p.Id},{p.Date:yyyy-MM-dd},{p.Income},{p.Employer},{p.Description}"));

            return string.Join(Environment.NewLine, csv);
        }

    }

    public class Transaction
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }

    public class Paystub
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Income { get; set; }
        public string Employer { get; set; }
        public string Description { get; set; }
    }

    public static class ReportsPageState
    {
        public static DateTime? StartDate { get; set; } = null;
        public static DateTime? EndDate { get; set; } = null;
    }

}
