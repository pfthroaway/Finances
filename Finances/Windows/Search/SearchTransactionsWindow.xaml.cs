using Extensions;
using Extensions.DataTypeHelpers;
using Finances.Classes;
using Finances.Classes.Categories;
using Finances.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Extensions.Enums;

namespace Finances.Windows.Search
{
    /// <summary>Interaction logic for SearchTransactionsWindow.xaml</summary>
    public partial class SearchTransactionsWindow
    {
        private readonly List<Account> _allAccounts = AppState.AllAccounts;
        private readonly List<Category> _allCategories = AppState.AllCategories;
        private Category _selectedCategory = new Category();
        private Account _selectedAccount = new Account();
        private List<Transaction> _matchingTransactions = new List<Transaction>(AppState.AllTransactions);

        internal Accounts.ViewAccountWindow PreviousWindow { private get; set; }

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
            DateTime selectedDate = TransactionDate.SelectedDate != null ? DateTimeHelper.Parse(TransactionDate.SelectedDate) : DateTime.MinValue;
            string payee = TxtPayee.Text.ToLower();
            string majorCategory = CmbMajorCategory.SelectedIndex != -1 ? CmbMajorCategory.SelectedValue.ToString().ToLower() : "";
            string minorCategory = CmbMinorCategory.SelectedIndex != -1 ? CmbMinorCategory.SelectedValue.ToString().ToLower() : "";
            string memo = TxtMemo.Text.ToLower();
            decimal outflow = DecimalHelper.Parse(TxtOutflow.Text.ToLower());
            decimal inflow = DecimalHelper.Parse(TxtInflow.Text.ToLower());
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
            CmbMajorCategory.SelectedIndex = -1;
            CmbMinorCategory.SelectedIndex = -1;
            TxtMemo.Text = "";
            TxtPayee.Text = "";
            TxtInflow.Text = "";
            TxtOutflow.Text = "";
            CmbAccount.SelectedIndex = -1;
            _matchingTransactions = new List<Transaction>(AppState.AllTransactions);
        }

        #region Button-Click Methods

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTransaction())
            {
                CmbMinorCategory.Focus();
                Search.SearchResultsWindow searchResultsWindow = new Search.SearchResultsWindow
                {
                    PreviousWindow = this
                };
                searchResultsWindow.LoadWindow(_matchingTransactions);
                searchResultsWindow.Show();
                Visibility = Visibility.Hidden;
            }
            else
                AppState.DisplayNotification("No results found matching your search criteria.", "Finances", this);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        #endregion Button-Click Methods

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            BtnSearch.IsEnabled = (TransactionDate.SelectedDate != null | CmbMajorCategory.SelectedIndex >= 0 |
                                   CmbMinorCategory.SelectedIndex >= 0 | TxtPayee.Text.Length > 0 |
                                   TxtInflow.Text.Length > 0 | TxtOutflow.Text.Length > 0 |
                                   CmbAccount.SelectedIndex >= 0);
        }

        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void TxtInOutflow_TextChanged(object sender, TextChangedEventArgs e)
        {
            Functions.TextBoxTextChanged(sender, KeyType.Decimals);
            TextChanged();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMajorCategory.SelectedIndex >= 0)
            {
                CmbMinorCategory.IsEnabled = true;
                _selectedCategory = (Category)CmbMajorCategory.SelectedValue;
                CmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
            }
            else
            {
                CmbMinorCategory.IsEnabled = false;
                _selectedCategory = new Category();
                CmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
            }

            TextChanged();
        }

        private void CmbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbAccount.SelectedIndex >= 0)
                _selectedAccount = (Account)CmbAccount.SelectedValue;
            else
                _selectedAccount = new Account();
            TextChanged();
        }

        #endregion Text/Selection Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public SearchTransactionsWindow()
        {
            InitializeComponent();
            CmbAccount.ItemsSource = _allAccounts;
            CmbMajorCategory.ItemsSource = _allCategories;
            CmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
        }

        private void WindowSearchTransactions_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        private void TxtInflowOutflow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Decimals);
        }

        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        #endregion Window-Manipulation Methods
    }
}