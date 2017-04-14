using Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances
{
    /// <summary>Interaction logic for NewAccountWindow.xaml</summary>
    public partial class NewAccountWindow
    {
        internal MainWindow PreviousWindow { private get; set; }

        private readonly List<string> _allAccountTypes = AppState.AllAccountTypes;

        #region Button-Click Methods

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.AllAccounts.All(account => account.Name != TxtAccountName.Text))
            {
                Enum.TryParse(CmbAccountTypes.SelectedValue.ToString().Replace(" ", ""), out AccountTypes currentType);
                Account newAccount = new Account(TxtAccountName.Text, currentType, new List<Transaction>());
                Transaction newTransaction = new Transaction(
                    date: DateTime.Now,
                    payee: "Income",
                    majorCategory: "Income",
                    minorCategory: "Starting Balance",
                    memo: "",
                    outflow: 0.00M,
                    inflow: DecimalHelper.Parse(TxtBalance.Text),
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

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text Changed

        private void TextChanged()
        {
            BtnSubmit.IsEnabled = TxtAccountName.Text.Length > 0 && CmbAccountTypes.SelectedIndex >= 0 && TxtBalance.Text.Length > 0;
        }

        private void TxtAccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void TxtBalance_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.TextBoxTextChanged(sender, KeyType.NegativeDecimals);
            TextChanged();
        }

        #endregion Text Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public NewAccountWindow()
        {
            InitializeComponent();
            TxtAccountName.Focus();
            CmbAccountTypes.ItemsSource = _allAccountTypes;
        }

        private void CmbAccountTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void WindowNewAccount_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}