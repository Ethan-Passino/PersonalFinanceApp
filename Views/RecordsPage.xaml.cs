using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;
using static PersonalFinanceApp.MainWindow;

namespace PersonalFinanceApp
{
    public partial class RecordsPage : Page
    {

        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<Paystub> Paystubs { get; set; }
        public RecordsPage()
        {
            InitializeComponent();
            Transactions = new ObservableCollection<Transaction>();
            Paystubs = new ObservableCollection<Paystub>();
            DataContext = this;
            LoadDataFromDatabase();

            TransactionsGrid.ItemsSource = Transactions;
            PaystubsGrid.ItemsSource = Paystubs;
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                // Load Transactions
                string transactionQuery = "SELECT Amount, Category, Date, Description FROM Transactions;";
                var transactionsTable = DatabaseHelper.ExecuteQuery(transactionQuery);

                foreach (DataRow row in transactionsTable.Rows)
                {
                    Transactions.Add(new Transaction
                    {
                        Amount = Convert.ToDouble(row["Amount"]),
                        Category = row["Category"].ToString(),
                        Date = row["Date"].ToString(),
                        Description = row["Description"].ToString()
                    });

                }

                // Load PayStubs
                string payStubQuery = "SELECT Income, Date, Employer, Description FROM PayStubs;";
                var payStubsTable = DatabaseHelper.ExecuteQuery(payStubQuery);

                foreach (DataRow row in payStubsTable.Rows)
                {
                    Paystubs.Add(new Paystub
                    {
                        Income = Convert.ToDecimal(row["Income"]),
                        Date = row["Date"].ToString(),
                        Description = row["Description"].ToString(),
                        Employer = row["Employer"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Error loading data.", NotificationType.Error);
                Console.WriteLine(ex.ToString());
            }
        }


        public class Transaction
        {
            public string Category { get; set; }
            public double Amount { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
        }

        public class Paystub
        {
            public string Date { get; set; }           // Date of the paycheck
            public decimal Income { get; set; }       // Income amount
            public string Employer { get; set; }      // Employer or source of income
            public string Description { get; set; }   // Additional details or notes

        }
    }
}
