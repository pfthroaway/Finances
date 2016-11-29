using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances
{
    /// <summary>
    /// Interaction logic for NewAccountWindow.xaml
    /// </summary>
    public partial class NewAccountWindow : Window
    {
        internal MainWindow RefToMainWindow { get; set; }

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!AppState.AllAccounts.Any(account => account.Name == txtAccountName.Text))
            {
                Account newAccount = new Account(txtAccountName.Text, new List<Transaction>());
                Transaction newTransaction = new Transaction(DateTime.Now, "Income", "Income", "Starting Balance", "", 0.00M, DecimalHelper.Parse(txtBalance.Text));
                newAccount.AddTransaction(new Transaction(newTransaction));
                AppState.AllAccounts.Add(newAccount);
                AppState.AllAccounts = AppState.AllAccounts.OrderBy(account => account.Name).ToList();
                if (await AppState.AddAccount(newAccount))
                {
                    if (await AppState.AddTransaction(newTransaction, newAccount))
                    {
                        RefToMainWindow.RefreshItemsSource();
                        CloseWindow();
                    }
                }
            }
            else
                MessageBox.Show("That account name already exists.", "Finances", MessageBoxButton.OK);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text Changed

        private void TextChanged()
        {
            if (txtAccountName.Text.Length > 0 && txtBalance.Text.Length > 0)
                btnSubmit.IsEnabled = true;
            else
                btnSubmit.IsEnabled = false;
        }

        private void txtAccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void txtBalance_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        #endregion Text Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public NewAccountWindow()
        {
            InitializeComponent();
        }

        private void txtAccountName_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAccountName.SelectAll();
        }

        private void txtBalance_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBalance.SelectAll();
        }

        private void windowNewAccount_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}