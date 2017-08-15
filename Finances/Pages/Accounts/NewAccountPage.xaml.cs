using Extensions;
using Extensions.DataTypeHelpers;
using Extensions.Enums;
using Finances.Classes;
using Finances.Classes.Data;
using Finances.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages.Accounts
{
    /// <summary>Interaction logic for NewAccountWindow.xaml</summary>
    public partial class NewAccountPage
    {
        private readonly List<string> _allAccountTypes = AppState.AllAccountTypes;

        #region Button-Click Methods

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.AllAccounts.All(account => account.Name != TxtAccountName.Text))
            {
                Enum.TryParse(CmbAccountTypes.SelectedValue.ToString().Replace(" ", ""), out AccountTypes currentType);
                Account newAccount = new Account(TxtAccountName.Text, currentType, new List<Transaction>());
                Transaction newTransaction = new Transaction(await AppState.GetNextTransactionsIndex(), DateTime.Now,
                    "Income", "Income", "Starting Balance", "", 0.00M, DecimalHelper.Parse(TxtBalance.Text),
                    newAccount.Name);
                newAccount.AddTransaction(newTransaction);
                if (await AppState.AddAccount(newAccount))
                {
                    if (await AppState.AddTransaction(newTransaction, newAccount))
                        ClosePage();
                    else
                        AppState.DisplayNotification("Unable to process new account.", "Finances");
                }
            }
            else
                AppState.DisplayNotification("That account name already exists.", "Finances");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ClosePage();

        #endregion Button-Click Methods

        #region Text Changed

        private void TextChanged() => BtnSubmit.IsEnabled = TxtAccountName.Text.Length > 0 && CmbAccountTypes.SelectedIndex >= 0 && TxtBalance.Text.Length > 0;

        private void TxtAccountName_TextChanged(object sender, TextChangedEventArgs e) => TextChanged();

        private void TxtBalance_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.TextBoxTextChanged(sender, KeyType.NegativeDecimals);
            TextChanged();
        }

        #endregion Text Changed

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public NewAccountPage()
        {
            InitializeComponent();
            TxtAccountName.Focus();
            CmbAccountTypes.ItemsSource = _allAccountTypes;
        }

        private void NewAccountPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        private void CmbAccountTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) => TextChanged();

        private void Txt_GotFocus(object sender, RoutedEventArgs e) => Functions.TextBoxGotFocus(sender);

        #endregion Page-Manipulation Methods
    }
}