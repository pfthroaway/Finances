using Extensions;
using Extensions.DataTypeHelpers;
using Extensions.Enums;
using Finances.Classes;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances.Pages.Transactions
{
    /// <summary>Interaction logic for NewTransferWindow.xaml</summary>
    public partial class NewTransferPage
    {
        private readonly List<Account> _allAccounts = AppState.AllAccounts;
        private Account _transferFromAccount = new Account();
        private Account _transferToAccount = new Account();

        /// <summary>Adds a transfer to the database.</summary>
        /// <returns>True if successful</returns>
        private async Task<bool> AddTransfer()
        {
            Transaction transferFrom = new Transaction(await AppState.GetNextTransactionsIndex(), DateTimeHelper.Parse(TransferDate.SelectedDate), "Transfer", "Transfer", "Transfer", _transferToAccount.Name, DecimalHelper.Parse(TxtTransferAmount.Text), 0.00M, _transferFromAccount.Name);
            _transferFromAccount.AddTransaction(transferFrom);
            Transaction transferTo = new Transaction(await AppState.GetNextTransactionsIndex() + 1, DateTimeHelper.Parse(TransferDate.SelectedDate), "Transfer", "Transfer", "Transfer", _transferFromAccount.Name, 0.00M, DecimalHelper.Parse(TxtTransferAmount.Text), _transferToAccount.Name);
            _transferToAccount.AddTransaction(transferTo);
            if (await AppState.AddTransaction(transferFrom, _transferFromAccount))
            {
                if (await AppState.AddTransaction(transferTo, _transferToAccount))
                    return true;
            }
            return false;
        }

        /// <summary>Resets all values to default status.</summary>
        private void Reset()
        {
            TransferDate.Text = "";
            CmbTransferFrom.SelectedIndex = -1;
            CmbTransferTo.SelectedIndex = -1;
            TxtTransferAmount.Text = "";
        }

        /// <summary>Toggles Buttons on the Window.</summary>
        /// <param name="enabled">Should Button be enabled?</param>
        private void ToggleButtons(bool enabled)
        {
            BtnSaveAndDone.IsEnabled = enabled;
            BtnSaveAndDuplicate.IsEnabled = enabled;
            BtnSaveAndNew.IsEnabled = enabled;
        }

        #region Button-Click Methods

        private async void BtnSaveAndDone_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTransferFrom.SelectedValue != CmbTransferTo.SelectedValue && await AddTransfer())
                ClosePage();
        }

        private async void BtnSaveAndDuplicate_Click(object sender, RoutedEventArgs e) => await AddTransfer();

        private async void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTransferFrom.SelectedValue != CmbTransferTo.SelectedValue && await AddTransfer())
            {
                Reset();
                CmbTransferFrom.Focus();
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e) => Reset();

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ClosePage();

        #endregion Button-Click Methods

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            if (TransferDate.SelectedDate != null && CmbTransferFrom.SelectedIndex >= 0 && CmbTransferFrom.SelectedIndex >= 0 && TxtTransferAmount.Text.Length > 0 && CmbTransferFrom.SelectedValue != CmbTransferTo.SelectedValue)
                ToggleButtons(true);
            else
                ToggleButtons(false);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => TextChanged();

        private void CmbTransferFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTransferFrom.SelectedIndex >= 0)
                _transferFromAccount = (Account)CmbTransferFrom.SelectedValue;
            else
                _transferFromAccount = new Account();
            TextChanged();
        }

        private void CmbTransferTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTransferTo.SelectedIndex >= 0)
                _transferToAccount = (Account)CmbTransferTo.SelectedValue;
            else
                _transferToAccount = new Account();
            TextChanged();
        }

        private void TxtTransferAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.TextBoxTextChanged(sender, KeyType.Decimals);
            TextChanged();
        }

        #endregion Text/Selection Changed

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public NewTransferPage()
        {
            InitializeComponent();
            CmbTransferFrom.ItemsSource = _allAccounts;
            CmbTransferTo.ItemsSource = _allAccounts;
        }

        private void NewTransferPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        private void TxtTransferAmount_PreviewKeyDown(object sender, KeyEventArgs e) => Functions.PreviewKeyDown(e, KeyType.Decimals);

        private void TxtTransferAmount_GotFocus(object sender, RoutedEventArgs e) => Functions.TextBoxGotFocus(sender);

        #endregion Page-Manipulation Methods
    }
}