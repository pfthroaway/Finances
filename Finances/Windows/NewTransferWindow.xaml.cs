using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>
    /// Interaction logic for NewTransferWindow.xaml
    /// </summary>
    public partial class NewTransferWindow : Window
    {
        private List<Account> AllAccounts = AppState.AllAccounts;
        private Account transferFromAccount = new Account();
        private Account transferToAccount = new Account();

        internal ViewAccountWindow RefToViewAccountWindow { get; set; }

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            if (datePicker.SelectedDate != null && cmbTransferFrom.SelectedIndex >= 0 && cmbTransferFrom.SelectedIndex >= 0 && txtTransferAmount.Text.Length > 0)
                btnSubmit.IsEnabled = true;
            else
                btnSubmit.IsEnabled = false;
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

        private void txtMemo_TextChanged(object sender, TextChangedEventArgs e)
        {
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

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            Transaction transferFrom = new Transaction(DateTimeHelper.Parse(datePicker.SelectedDate), "Transfer", "Transfer", transferToAccount.Name, txtMemo.Text, DecimalHelper.Parse(txtTransferAmount.Text), 0.00M);
            transferFromAccount.AddTransaction(transferFrom);
            Transaction transferTo = new Transaction(DateTimeHelper.Parse(datePicker.SelectedDate), "Transfer", "Transfer", transferFromAccount.Name, txtMemo.Text, 0.00M, DecimalHelper.Parse(txtTransferAmount.Text));
            transferToAccount.AddTransaction(transferTo);
            if (await AppState.AddTransaction(transferFrom, transferFromAccount))
            {
                if (await AppState.AddTransaction(transferTo, transferToAccount))
                {
                    RefToViewAccountWindow.RefreshItemsSource();
                    CloseWindow();
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            cmbTransferFrom.SelectedIndex = -1;
            cmbTransferTo.SelectedIndex = -1;
            txtMemo.Text = "";
            txtTransferAmount.Text = "";
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

            if (keys.Any(key => key == true) || (Key.D0 <= k && k <= Key.D9) || (Key.NumPad0 <= k && k <= Key.NumPad9) || k == Key.Decimal || k == Key.OemPeriod)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void txtTransferAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            txtTransferAmount.SelectAll();
        }

        private void txtMemo_GotFocus(object sender, RoutedEventArgs e)
        {
            txtMemo.SelectAll();
        }

        private void windowNewTransfer_Closing(object sender, CancelEventArgs e)
        {
            RefToViewAccountWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}