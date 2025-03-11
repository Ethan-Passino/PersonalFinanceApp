﻿using Microsoft.Win32;
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
        private List<string> categories;
        private readonly string categoriesFile = "categories.txt";
        private readonly List<string> defaultCategories = new List<string> { "Rent", "Gas", "Food", "Entertainment", "Savings", "Monthly", "Maintenance", "Other" };

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
            if (File.Exists(categoriesFile))
            {
                categories = File.ReadAllLines(categoriesFile).ToList();
            }
            else
            {
                categories = new List<string>(defaultCategories);
                File.WriteAllLines(categoriesFile, categories);
            }
            CategoryList.ItemsSource = categories;
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string newCategory = CategoryInput.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory) && !categories.Contains(newCategory))
            {
                categories.Add(newCategory);
                File.WriteAllLines(categoriesFile, categories);
                CategoryList.ItemsSource = null;
                CategoryList.ItemsSource = categories;
                CategoryInput.Clear();
            }
        }

        private void RemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryList.SelectedItem is string selectedCategory)
            {
                categories.Remove(selectedCategory);
                File.WriteAllLines(categoriesFile, categories);
                CategoryList.ItemsSource = null;
                CategoryList.ItemsSource = categories;
            }
        }
    }
}