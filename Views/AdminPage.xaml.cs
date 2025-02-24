using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PersonalFinanceApp.Views
{
    public partial class AdminPage : Page
    {
        private string databasePath = "finance.db"; // Ensure this path points to your SQLite database

        public AdminPage()
        {
            InitializeComponent();
        }

        private void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "finance_backup.db",
                DefaultExt = ".db",
                Filter = "SQLite Database (*.db)|*.db"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.Copy(databasePath, saveFileDialog.FileName, true);
                    BackupStatus.Text = "Backup successful: " + saveFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    BackupStatus.Text = "Backup failed: " + ex.Message;
                }
            }
        }
    }
}
