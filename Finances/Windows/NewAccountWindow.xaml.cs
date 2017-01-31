using Extensions;
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
    public partial class NewAccountWindow
    {
        internal MainWindow RefToMainWindow { private get; set; }

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.AllAccounts.All(account => account.Name != txtAccountName.Text))
            {
                Account newAccount = new Account(txtAccountName.Text, new List<Transaction>());
                Transaction newTransaction = new Transaction(
                    date: DateTime.Now,
                    payee: "Income",
                    majorCategory: "Income",
                    minorCategory: "Starting Balance",
                    memo: "",
                    outflow: 0.00M,
                    inflow: DecimalHelper.Parse(txtBalance.Text),
                    account: newAccount.Name);
                newAccount.AddTransaction(new Transaction(newTransaction));
                if (await AppState.AddAccount(newAccount))
                {
                    if (await AppState.AddTransaction(newTransaction, newAccount))
                        CloseWindow();
                    else
                        new Notification("Unable to process new account.", "Finances", NotificationButtons.OK, this).ShowDialog();
                }
            }
            else
                new Notification("That account name already exists.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text Changed

        private void TextChanged()
        {
            btnSubmit.IsEnabled = txtAccountName.Text.Length > 0 && txtBalance.Text.Length > 0;
        }

        private void txtAccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void txtBalance_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.TextBoxTextChanged(sender, KeyType.NegativeDecimals);
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
            txtAccountName.Focus();
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void windowNewAccount_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.RefreshItemsSource();
            RefToMainWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}