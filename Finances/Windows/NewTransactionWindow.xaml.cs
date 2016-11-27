﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>Interaction logic for NewTransactionWindow.xaml</summary>
    public partial class NewTransactionWindow : Window, INotifyPropertyChanged
    {
        private List<Account> AllAccounts = AppState.AllAccounts;
        private List<Category> AllCategories = AppState.AllCategories;
        private Category selectedCategory = new Category();
        private Account selectedAccount = new Account();

        internal MainWindow RefToMainWindow { get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            Transaction newTransaction = new Transaction(new Transaction(DateTimeHelper.Parse(datePicker.SelectedDate), txtPayee.Text, cmbMajorCategory.SelectedValue.ToString(), cmbMinorCategory.SelectedValue.ToString(), txtMemo.Text, DecimalHelper.Parse(txtOutflow.Text), DecimalHelper.Parse(txtInflow.Text)));
            selectedAccount.AddTransaction(newTransaction);
            if (await AppState.AddTransaction(newTransaction, selectedAccount))
                CloseWindow();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            cmbMajorCategory.SelectedIndex = -1;
            cmbMinorCategory.SelectedIndex = -1;
            txtMemo.Text = "";
            txtPayee.Text = "";
            txtInflow.Text = "";
            txtOutflow.Text = "";
            cmbAccount.SelectedIndex = -1;
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
                btnSubmit.IsEnabled = true;
            else
                btnSubmit.IsEnabled = false;
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
        }

        private void txtOutflow_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtOutflow.Text = new string((from c in txtOutflow.Text
                                          where char.IsDigit(c) || c.IsPeriod()
                                          select c).ToArray());
            txtOutflow.CaretIndex = txtOutflow.Text.Length;
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
                selectedCategory = (Category)cmbMajorCategory.SelectedValue;
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

        public NewTransactionWindow()
        {
            InitializeComponent();
            cmbAccount.ItemsSource = AllAccounts;
            cmbMajorCategory.ItemsSource = AllCategories;
            cmbMinorCategory.ItemsSource = selectedCategory.MinorCategories;
        }

        private void windowNewTransaction_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
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