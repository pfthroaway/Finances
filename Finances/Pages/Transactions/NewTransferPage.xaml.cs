using Extensions;
using Extensions.DataTypeHelpers;
using Extensions.Enums;
using Finances.Classes;
using Finances.Classes.Data;
using Finances.Pages.Accounts;
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

        internal ViewAccountPage PreviousWindow { private get; set; }

        private async Task<bool> AddTransfer()
        {
            Transaction transferFrom = new Transaction(
                date: DateTimeHelper.Parse(TransferDate.SelectedDate),
                payee: "Transfer",
                majorCategory: "Transfer",
                minorCategory: "Transfer",
                memo: _transferToAccount.Name,
                outflow: DecimalHelper.Parse(TxtTransferAmount.Text),
                inflow: 0.00M,
                account: _transferFromAccount.Name);
            _transferFromAccount.AddTransaction(transferFrom);
            Transaction transferTo = new Transaction(
                date: DateTimeHelper.Parse(TransferDate.SelectedDate),
                payee: "Transfer",
                majorCategory: "Transfer",
                minorCategory: "Transfer",
                memo: _transferFromAccount.Name,
                outflow: 0.00M,
                inflow: DecimalHelper.Parse(TxtTransferAmount.Text),
                account: _transferToAccount.Name);
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
                CloseWindow();
            else if (CmbTransferFrom.SelectedValue == CmbTransferTo.SelectedValue)
                AppState.DisplayNotification("The source account and the destination account cannot be the same.", "Finances");
            else
                AppState.DisplayNotification("Unable to process transfer.", "Finances");
        }

        private async void BtnSaveAndDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTransferFrom.SelectedValue == CmbTransferTo.SelectedValue || !await AddTransfer())
            {
                if (CmbTransferFrom.SelectedValue == CmbTransferTo.SelectedValue)
                    AppState.DisplayNotification("The source account and the destination account cannot be the same.", "Finances");
                else
                    AppState.DisplayNotification("Unable to process transfer.", "Finances");
            }
        }

        private async void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTransferFrom.SelectedValue != CmbTransferTo.SelectedValue && await AddTransfer())
            {
                Reset();
                CmbTransferFrom.Focus();
            }
            else if (CmbTransferFrom.SelectedValue == CmbTransferTo.SelectedValue)
                AppState.DisplayNotification("The source account and the destination account cannot be the same.", "Finances"
                     );
            else
                AppState.DisplayNotification("Unable to process transfer.", "Finances");
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            if (TransferDate.SelectedDate != null && CmbTransferFrom.SelectedIndex >= 0 && CmbTransferFrom.SelectedIndex >= 0 && TxtTransferAmount.Text.Length > 0)
                ToggleButtons(true);
            else
                ToggleButtons(false);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

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

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            PreviousWindow.RefreshItemsSource();
            AppState.GoBack();
        }

        public NewTransferPage()
        {
            InitializeComponent();
            CmbTransferFrom.ItemsSource = _allAccounts;
            CmbTransferTo.ItemsSource = _allAccounts;
        }

        private void TxtTransferAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Decimals);
        }

        private void TxtTransferAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        #endregion Window-Manipulation Methods

        private void NewTransferPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
        }
    }
}