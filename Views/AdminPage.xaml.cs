using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            LoadCategories();
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

        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Restoring a backup will overwrite your current data.\nAre you sure you want to proceed?", "Confirm Restore", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    DefaultExt = ".db",
                    Filter = "SQLite Database (*.db)|*.db"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        File.Copy(openFileDialog.FileName, databasePath, true);
                        BackupStatus.Text = "Database restored successfully from: " + openFileDialog.FileName;
                        MessageBox.Show("Database restored successfully. You will need to reopen the program.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        BackupStatus.Text = "Restore failed: " + ex.Message;
                    }
                }
            }
        }

        private void ResetData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all data?\nType 'confirm' to proceed.", "Confirm Reset", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Type 'confirm' to reset data:", "Confirm Reset", "");
                if (input.ToLower() == "confirm")
                {
                    try
                    {
                        File.Delete(databasePath);
                        BackupStatus.Text = "Data reset successful. The application will now close. Please restart.";
                        MessageBox.Show("The application will now close. Please restart the program.", "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        BackupStatus.Text = "Reset failed: " + ex.Message;
                    }
                }
                else
                {
                    BackupStatus.Text = "Reset canceled. Incorrect confirmation input.";
                }
            }
        }

        private void LoadCategories()
        {
            CategoryList.ItemsSource = DatabaseHelper.GetCategories();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            // Reset Category border
            CategoryInput.BorderBrush = System.Windows.Media.Brushes.Gray;
            CategoryInput.BorderThickness = new Thickness(1);

            string newCategory = CategoryInput.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory))
            {
                DatabaseHelper.AddCategory(newCategory);
                LoadCategories();
                CategoryInput.Clear();
            }
            else
            {
                CategoryInput.BorderBrush = System.Windows.Media.Brushes.Red;
                CategoryInput.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please enter text into the field to create a category.", MainWindow.NotificationType.Error);
            }
        }

        private void RenameCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem is string selectedCategory)
            {
                string newCategoryName = RenameCategoryInput.Text.Trim();

                if (string.IsNullOrWhiteSpace(newCategoryName))
                {
                    RenameCategoryInput.BorderBrush = System.Windows.Media.Brushes.Red;
                    RenameCategoryInput.BorderThickness = new Thickness(2);
                    MainWindow.Instance.ShowNotification("Please enter a new name for the category.", MainWindow.NotificationType.Error);
                    return;
                }

                if (selectedCategory == "Other")
                {
                    MessageBox.Show("The 'Other' category cannot be renamed.", "Rename Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    DatabaseHelper.RenameCategory(selectedCategory, newCategoryName);
                    LoadCategories();
                    RenameCategoryInput.Clear();
                    MainWindow.Instance.ShowNotification($"Category '{selectedCategory}' renamed to '{newCategoryName}'.", MainWindow.NotificationType.Success);
                }
                catch (Exception ex)
                {
                    MainWindow.Instance.ShowNotification("Rename failed: " + ex.Message, MainWindow.NotificationType.Critical);
                }
            }
            else
            {
                CategoryList.BorderBrush = System.Windows.Media.Brushes.Red;
                CategoryList.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please select a category to rename.", MainWindow.NotificationType.Error);
            }
        }


        private void RemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            CategoryList.BorderBrush = System.Windows.Media.Brushes.Gray;
            CategoryList.BorderThickness = new Thickness(1);
            if (CategoryList.SelectedItem is string selectedCategory)
            {
                if (selectedCategory == "Other")
                {
                    MessageBox.Show("The 'Other' category cannot be deleted, it is used as a default category.", 
                        "Deletion Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DatabaseHelper.RemoveCategory(selectedCategory);
                string message = $"Are you sure you want to delete the category '{selectedCategory}'?\n\n" +
                                 "All transactions under this category will be reassigned to 'Other'.\n" +
                                 "Any budget associated with this category will be deleted permanently.";

                MessageBoxResult result = MessageBox.Show(message, "Confirm Category Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DatabaseHelper.RemoveCategory(selectedCategory);
                    LoadCategories();
                    MessageBox.Show($"Category '{selectedCategory}' has been deleted. All transactions moved to 'Other'.",
                                    "Category Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                CategoryList.BorderBrush = System.Windows.Media.Brushes.Red;
                CategoryList.BorderThickness = new Thickness(2);
                MainWindow.Instance.ShowNotification("Please select a category to remove.", MainWindow.NotificationType.Error);
            }
        }
    }
}