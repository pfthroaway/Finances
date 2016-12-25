using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>
    /// Interaction logic for NewTransferWindow.xaml
    /// </summary>
    public partial class NewTransferWindow
    {
        private readonly List<Account> AllAccounts = AppState.AllAccounts;
        private Account transferFromAccount = new Account();
        private Account transferToAccount = new Account();

        internal ViewAccountWindow RefToViewAccountWindow { private get; set; }

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            if (datePicker.SelectedDate != null && cmbTransferFrom.SelectedIndex >= 0 && cmbTransferFrom.SelectedIndex >= 0 && txtTransferAmount.Text.Length > 0)
            {
                btnSaveAndDone.IsEnabled = true;
                btnSaveAndNew.IsEnabled = true;
            }
            else
            {
                btnSaveAndDone.IsEnabled = false;
                btnSaveAndNew.IsEnabled = false;
            }
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        private void cmbTransferFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTransferFrom.SelectedIndex >= 0)
                transferFromAccount = (Account)cmbTransferFrom.SelectedValue;
            else
                transferFromAccount = new Account();
            TextChanged();
        }

        private void cmbTransferTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTransferTo.SelectedIndex >= 0)
                transferToAccount = (Account)cmbTransferTo.SelectedValue;
            else
                transferToAccount = new Account();
            TextChanged();
        }

        private void txtTransferAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtTransferAmount.Text = new string((from c in txtTransferAmount.Text
                                                 where char.IsDigit(c) || c.IsPeriod()
                                                 select c).ToArray());
            txtTransferAmount.CaretIndex = txtTransferAmount.Text.Length;
            TextChanged();
        }

        #endregion Text/Selection Changed

        private async Task<bool> AddTransfer()
        {
            Transaction transferFrom = new Transaction(
                date: DateTimeHelper.Parse(datePicker.SelectedDate),
                payee: "Transfer",
                majorCategory: "Transfer",
                minorCategory: "Transfer",
                memo: transferToAccount.Name,
                outflow: DecimalHelper.Parse(txtTransferAmount.Text),
                inflow: 0.00M,
                account: transferFromAccount.Name);
            transferFromAccount.AddTransaction(transferFrom);
            Transaction transferTo = new Transaction(
                date: DateTimeHelper.Parse(datePicker.SelectedDate),
                payee: "Transfer",
                majorCategory: "Transfer",
                minorCategory: "Transfer",
                memo: transferFromAccount.Name,
                outflow: 0.00M,
                inflow: DecimalHelper.Parse(txtTransferAmount.Text),
                account: transferToAccount.Name);
            transferToAccount.AddTransaction(transferTo);
            if (await AppState.AddTransaction(transferFrom, transferFromAccount))
            {
                if (await AppState.AddTransaction(transferTo, transferToAccount))
                    return true;
            }
            return false;
        }

        /// <summary>Resets all values to default status.</summary>
        private void Reset()
        {
            cmbTransferFrom.SelectedIndex = -1;
            cmbTransferTo.SelectedIndex = -1;
            txtTransferAmount.Text = "";
        }

        #region Button-Click Methods

        private async void btnSaveAndDone_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTransferFrom.SelectedValue != cmbTransferTo.SelectedValue && await AddTransfer())
                CloseWindow();
            else if (cmbTransferFrom.SelectedValue == cmbTransferTo.SelectedValue)
                new Notification("The source account and the destination account cannot be the same.", "Finances", NotificationButtons.OK, this).ShowDialog();
            else
                new Notification("Unable to process transfer.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void btnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTransferFrom.SelectedValue != cmbTransferTo.SelectedValue && await AddTransfer())
            {
                Reset();
                cmbTransferFrom.Focus();
            }
            else if (cmbTransferFrom.SelectedValue == cmbTransferTo.SelectedValue)
                new Notification("The source account and the destination account cannot be the same.", "Finances",
                    NotificationButtons.OK, this).ShowDialog();
            else
                new Notification("Unable to process transfer.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public NewTransferWindow()
        {
            InitializeComponent();
            cmbTransferFrom.ItemsSource = AllAccounts;
            cmbTransferTo.ItemsSource = AllAccounts;
        }

        private void txtTransferAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key k = e.Key;

            List<bool> keys = AppState.GetListOfKeys(Key.Back, Key.Delete, Key.Home, Key.End, Key.LeftShift, Key.RightShift, Key.Enter, Key.Tab, Key.LeftAlt, Key.RightAlt, Key.Left, Key.Right, Key.LeftCtrl, Key.RightCtrl, Key.Escape);

            if (keys.Any(key => key) || (Key.D0 <= k && k <= Key.D9) || (Key.NumPad0 <= k && k <= Key.NumPad9) || k == Key.Decimal || k == Key.OemPeriod)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void txtTransferAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            txtTransferAmount.SelectAll();
        }

        private void windowNewTransfer_Closing(object sender, CancelEventArgs e)
        {
            RefToViewAccountWindow.RefreshItemsSource();
            RefToViewAccountWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}