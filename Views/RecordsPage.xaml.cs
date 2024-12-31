using System.Collections.Generic;
using System.Windows.Controls;

namespace PersonalFinanceApp
{
    public partial class RecordsPage : Page
    {
        public RecordsPage()
        {
            InitializeComponent();
            LoadMockData();
        }
        private void LoadMockData()
        {
            // Mock data for Transactions
            TransactionsGrid.ItemsSource = new List<Transaction>
    {
        new Transaction { Category = "Rent", Amount = 1200, Date = "01/01/2024", Description = "Monthly Rent Payment" },
        new Transaction { Category = "Food", Amount = 300, Date = "01/02/2024", Description = "Groceries for the week" },
        new Transaction { Category = "Gas", Amount = 50, Date = "01/03/2024", Description = "Gas for the car" },
        new Transaction { Category = "Entertainment", Amount = 75, Date = "01/04/2024", Description = "Movie night with friends" },
        new Transaction { Category = "Savings", Amount = 500, Date = "01/05/2024", Description = "Transfer to savings account" },
        new Transaction { Category = "Maintenance", Amount = 200, Date = "01/06/2024", Description = "Car maintenance" },
        new Transaction { Category = "Other", Amount = 100, Date = "01/07/2024", Description = "Miscellaneous expenses" },
        new Transaction { Category = "Food", Amount = 250, Date = "01/08/2024", Description = "Groceries for the week" },
        new Transaction { Category = "Entertainment", Amount = 100, Date = "01/09/2024", Description = "Concert tickets" },
        new Transaction { Category = "Gas", Amount = 60, Date = "01/10/2024", Description = "Gas for the car" },
        new Transaction { Category = "Rent", Amount = 1200, Date = "02/01/2024", Description = "Monthly Rent Payment" },
        new Transaction { Category = "Savings", Amount = 600, Date = "02/02/2024", Description = "Transfer to savings account" },
        new Transaction { Category = "Maintenance", Amount = 150, Date = "02/03/2024", Description = "House maintenance" },
        new Transaction { Category = "Other", Amount = 90, Date = "02/04/2024", Description = "Gift for a friend" },
        new Transaction { Category = "Food", Amount = 280, Date = "02/05/2024", Description = "Groceries for the week" },
        new Transaction { Category = "Entertainment", Amount = 120, Date = "02/06/2024", Description = "Sports game tickets" },
        new Transaction { Category = "Gas", Amount = 55, Date = "02/07/2024", Description = "Gas for the car" },
        new Transaction { Category = "Food", Amount = 310, Date = "02/08/2024", Description = "Groceries for the week" },
        new Transaction { Category = "Entertainment", Amount = 90, Date = "02/09/2024", Description = "Netflix subscription" },
        new Transaction { Category = "Rent", Amount = 1250, Date = "03/01/2024", Description = "Monthly Rent Payment" },
        new Transaction { Category = "Gas", Amount = 70, Date = "03/02/2024", Description = "Gas for the car" },
        new Transaction { Category = "Savings", Amount = 700, Date = "03/03/2024", Description = "Transfer to savings account" },
        new Transaction { Category = "Maintenance", Amount = 300, Date = "03/04/2024", Description = "Car repairs" },
    };

            // Mock data for Paystubs
            PaystubsGrid.ItemsSource = new List<Paystub>
    {
        new Paystub { Date = "01/01/2024", Income = 3000, Employer = "ABC Corporation", Description = "January Salary" },
        new Paystub { Date = "01/15/2024", Income = 500, Employer = "Freelance Project", Description = "Website Development Payment" },
        new Paystub { Date = "02/01/2024", Income = 3000, Employer = "ABC Corporation", Description = "February Salary" },
        new Paystub { Date = "02/10/2024", Income = 400, Employer = "Tutoring Services", Description = "Part-time tutoring" },
        new Paystub { Date = "02/20/2024", Income = 800, Employer = "Writing Agency", Description = "Freelance article writing" },
        new Paystub { Date = "03/01/2024", Income = 3200, Employer = "ABC Corporation", Description = "March Salary with Bonus" },
        new Paystub { Date = "03/15/2024", Income = 300, Employer = "Delivery Service", Description = "Delivery part-time job" },
        new Paystub { Date = "03/25/2024", Income = 700, Employer = "Consulting Firm", Description = "Business consulting session" },
        new Paystub { Date = "04/01/2024", Income = 3000, Employer = "ABC Corporation", Description = "April Salary" },
        new Paystub { Date = "04/15/2024", Income = 600, Employer = "Design Studio", Description = "Freelance graphic design" },
        new Paystub { Date = "04/25/2024", Income = 400, Employer = "Music Lessons", Description = "Teaching piano lessons" },
        new Paystub { Date = "05/01/2024", Income = 3100, Employer = "ABC Corporation", Description = "May Salary" },
        new Paystub { Date = "05/10/2024", Income = 500, Employer = "Blog Writing", Description = "Freelance blogging project" },
        new Paystub { Date = "05/20/2024", Income = 900, Employer = "Small Business", Description = "Freelance consulting for small business" },
    };
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
            public string PayPeriod { get; set; }     // Pay period (e.g., "01/01/2024 - 01/15/2024")
            public bool IsBonus { get; set; }         // Indicator if this is a bonus
        }

    }
}
