using Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>
    /// Interaction logic for SearchTransactionsWindow.xaml
    /// </summary>
    public partial class SearchTransactionsWindow
    {
        private readonly List<Account> _allAccounts = AppState.AllAccounts;
        private readonly List<Category> _allCategories = AppState.AllCategories;
        private Category _selectedCategory = new Category();
        private Account _selectedAccount = new Account();
        private List<Transaction> _matchingTransactions = new List<Transaction>(AppState.AllTransactions);

        internal ViewAccountWindow RefToViewAccountWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        /// <summary>Searches the list of transactions for specified criteria.</summary>
        /// <returns>Return true if any items match</returns>
        private bool SearchTransaction()
        {
            DateTime selectedDate = datePicker.SelectedDate != null ? DateTimeHelper.Parse(datePicker.SelectedDate) : DateTime.MinValue;
            string payee = txtPayee.Text.ToLower();
            string majorCategory = cmbMajorCategory.SelectedIndex != -1 ? cmbMajorCategory.SelectedValue.ToString().ToLower() : "";
            string minorCategory = cmbMinorCategory.SelectedIndex != -1 ? cmbMinorCategory.SelectedValue.ToString().ToLower() : "";
            string memo = txtMemo.Text.ToLower();
            decimal outflow = DecimalHelper.Parse(txtOutflow.Text.ToLower());
            decimal inflow = DecimalHelper.Parse(txtInflow.Text.ToLower());
            string account = _selectedAccount.Name?.ToLower() ?? "";

            if (selectedDate != DateTime.MinValue)
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.Date == selectedDate).ToList();

            if (!string.IsNullOrWhiteSpace(payee))
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.Payee.ToLower().Contains(payee)).ToList();

            if (!string.IsNullOrWhiteSpace(majorCategory))
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.MajorCategory.ToLower() == majorCategory).ToList();

            if (!string.IsNullOrWhiteSpace(minorCategory))
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.MinorCategory.ToLower() == minorCategory).ToList();

            if (!string.IsNullOrWhiteSpace(memo))
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.Memo.ToLower().Contains(memo)).ToList();

            if (outflow != 0M)
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.Outflow == outflow).ToList();

            if (inflow != 0M)
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.Inflow == inflow).ToList();

            if (!string.IsNullOrWhiteSpace(account))
                _matchingTransactions = _matchingTransactions.Where(transaction => transaction.Account.ToLower() == account).ToList();

            return _matchingTransactions.Count > 0;
        }

        /// <summary>Resets all values to default status.</summary>
        private void Reset()
        {
            cmbMajorCategory.SelectedIndex = -1;
            cmbMinorCategory.SelectedIndex = -1;
            txtMemo.Text = "";
            txtPayee.Text = "";
            txtInflow.Text = "";
            txtOutflow.Text = "";
            cmbAccount.SelectedIndex = -1;
            _matchingTransactions = new List<Transaction>(AppState.AllTransactions);
        }

        #region Button-Click Methods

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTransaction())
            {
                cmbMinorCategory.Focus();
                SearchResultsWindow searchResultsWindow = new SearchResultsWindow
                {
                    RefToSearchTransactionsWindowWindow = this
                };
                searchResultsWindow.LoadWindow(_matchingTransactions);
                searchResultsWindow.Show();
                this.Visibility = Visibility.Hidden;
            }
            else
                new Notification("No results found matching your search criteria.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        #endregion Button-Click Methods

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            btnSearch.IsEnabled = (datePicker.SelectedDate != null | cmbMajorCategory.SelectedIndex >= 0 |
                                   cmbMinorCategory.SelectedIndex >= 0 | txtPayee.Text.Length > 0 |
                                   txtInflow.Text.Length > 0 | txtOutflow.Text.Length > 0 |
                                   cmbAccount.SelectedIndex >= 0);
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void txtInOutflow_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.TextBoxTextChanged(sender, KeyType.Decimals);
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
                _selectedCategory = (Category)cmbMajorCategory.SelectedValue;
                cmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
            }
            else
            {
                cmbMinorCategory.IsEnabled = false;
                _selectedCategory = new Category();
                cmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
            }

            TextChanged();
        }

        private void cmbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAccount.SelectedIndex >= 0)
                _selectedAccount = (Account)cmbAccount.SelectedValue;
            else
                _selectedAccount = new Account();
            TextChanged();
        }

        #endregion Text/Selection Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public SearchTransactionsWindow()
        {
            InitializeComponent();
            cmbAccount.ItemsSource = _allAccounts;
            cmbMajorCategory.ItemsSource = _allCategories;
            cmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
        }

        private void windowSearchTransactions_Closing(object sender, CancelEventArgs e)
        {
            RefToViewAccountWindow.RefreshItemsSource();
            RefToViewAccountWindow.Show();
        }

        private void txtInflowOutflow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Decimals);
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        #endregion Window-Manipulation Methods
    }
}