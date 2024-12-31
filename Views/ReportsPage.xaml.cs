using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace PersonalFinanceApp
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void OnDownloadCsv(object sender, RoutedEventArgs e)
        {
            // Placeholder for time frame selection
            string selectedTimeFrame = (TimeFrameSelector.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Total";

            // Open SaveFileDialog to pick save location
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Save Report Data",
                FileName = $"Report_{selectedTimeFrame.Replace(" ", "_")}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Generate CSV data (replace with actual data generation logic)
                string csvData = GenerateCsvData(selectedTimeFrame);

                // Write the file
                File.WriteAllText(saveFileDialog.FileName, csvData);

                MessageBox.Show($"CSV file saved to {saveFileDialog.FileName}", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string GenerateCsvData(string timeFrame)
        {
            // Placeholder: Replace with actual logic to fetch and format data for the selected time frame
            return "Category,Amount\nRent,1200\nFood,500\nGas,300\nEntertainment,200\nOther,800";
        }
    }
}
