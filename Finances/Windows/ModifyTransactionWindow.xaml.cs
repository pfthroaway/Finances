﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>
    /// Interaction logic for ModifyTransactionWindow.xaml
    /// </summary>
    public partial class ModifyTransactionWindow : Window, INotifyPropertyChanged
    {
        private List<Account> AllAccounts = AppState.AllAccounts;
        private List<Category> AllCategories = AppState.AllCategories;
        private Category selectedCategory = new Category();
        private Account selectedAccount = new Account();

        private Transaction modifyTransaction = new Transaction();
        internal ViewAccountWindow RefToViewAccountWindow { get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        internal void SetCurrentTransaction(Transaction setTransaction, Account setAccount)
        {
            datePicker.SelectedDate = setTransaction.Date;
            cmbAccount.SelectedValue = setAccount;
            cmbMajorCategory.SelectedItem = AllCategories.Find(category => category.Name == setTransaction.MajorCategory);
            cmbMinorCategory.SelectedItem = setTransaction.MinorCategory;
            txtPayee.Text = setTransaction.Payee;
            txtOutflow.Text = setTransaction.Outflow.ToString();
            txtInflow.Text = setTransaction.Inflow.ToString();
            modifyTransaction = setTransaction;
            selectedAccount = setAccount;
        }

        #region Button-Click Methods

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Transaction newTransaction = new Transaction(
                date: DateTimeHelper.Parse(datePicker.SelectedDate),
                payee: txtPayee.Text,
                majorCategory: cmbMajorCategory.SelectedItem.ToString(),
                minorCategory: cmbMinorCategory.SelectedItem.ToString(),
                memo: txtMemo.Text,
                outflow: DecimalHelper.Parse(txtOutflow.Text),
                inflow: DecimalHelper.Parse(txtInflow.Text),
                account: selectedAccount.Name);

            if (newTransaction != modifyTransaction)
            {
                selectedAccount.ModifyTransaction(selectedAccount.AllTransactions.IndexOf(modifyTransaction), newTransaction);
                if (await AppState.ModifyTransaction(newTransaction, modifyTransaction, selectedAccount))
                    CloseWindow();
                else
                    MessageBox.Show("Unable to modify transaction.", "Finances", MessageBoxButton.OK);
            }
            else
                MessageBox.Show("This transaction has not been modified.", "Finances", MessageBoxButton.OK);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            if (datePicker.SelectedDate != null && cmbMajorCategory.SelectedIndex >= 0 && cmbMinorCategory.SelectedIndex >= 0 && txtPayee.Text.Length > 0 && (txtInflow.Text.Length > 0 | txtOutflow.Text.Length > 0) && cmbAccount.SelectedIndex >= 0)
                btnSave.IsEnabled = true;
            else
                btnSave.IsEnabled = false;
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void txtInflow_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtInflow.Text = new string((from c in txtInflow.Text
                                         where char.IsDigit(c) || c.IsPeriod()
                                         select c).ToArray());
            txtInflow.CaretIndex = txtInflow.Text.Length;
            TextChanged();
        }

        private void txtOutflow_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtOutflow.Text = new string((from c in txtOutflow.Text
                                          where char.IsDigit(c) || c.IsPeriod()
                                          select c).ToArray());
            txtOutflow.CaretIndex = txtOutflow.Text.Length;
            TextChanged();
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMajorCategory.SelectedIndex >= 0)
            {
                cmbMinorCategory.IsEnabled = true;
                selectedCategory = (Category)cmbMajorCategory.SelectedItem;
                cmbMinorCategory.ItemsSource = selectedCategory.MinorCategories;
            }
            else
            {
                cmbMinorCategory.IsEnabled = false;
                selectedCategory = new Category();
                cmbMinorCategory.ItemsSource = selectedCategory.MinorCategories;
            }

            TextChanged();
        }

        private void cmbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAccount.SelectedIndex >= 0)
                selectedAccount = (Account)cmbAccount.SelectedValue;
            else
                selectedAccount = new Account();
            TextChanged();
        }

        #endregion Text/Selection Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public ModifyTransactionWindow()
        {
            InitializeComponent();
            cmbAccount.ItemsSource = AllAccounts;
            cmbMajorCategory.ItemsSource = AllCategories;
            cmbMinorCategory.ItemsSource = selectedCategory.MinorCategories;
        }

        private void windowModifyTransaction_Closing(object sender, CancelEventArgs e)
        {
            RefToViewAccountWindow.RefreshItemsSource();
            RefToViewAccountWindow.Show();
        }

        private void txtInflowOutflow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key k = e.Key;

            List<bool> keys = AppState.GetListOfKeys(Key.Back, Key.Delete, Key.Home, Key.End, Key.LeftShift, Key.RightShift, Key.Enter, Key.Tab, Key.LeftAlt, Key.RightAlt, Key.Left, Key.Right, Key.LeftCtrl, Key.RightCtrl, Key.Escape);

            if (keys.Any(key => key == true) || (Key.D0 <= k && k <= Key.D9) || (Key.NumPad0 <= k && k <= Key.NumPad9) || k == Key.Decimal || k == Key.OemPeriod)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void txtMemo_GotFocus(object sender, RoutedEventArgs e)
        {
            txtMemo.SelectAll();
        }

        private void txtPayee_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPayee.SelectAll();
        }

        private void txtOutflow_GotFocus(object sender, RoutedEventArgs e)
        {
            txtOutflow.SelectAll();
        }

        private void txtInflow_GotFocus(object sender, RoutedEventArgs e)
        {
            txtInflow.SelectAll();
        }

        #endregion Window-Manipulation Methods
    }
}