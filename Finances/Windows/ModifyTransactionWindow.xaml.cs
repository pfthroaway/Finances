using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>Interaction logic for ModifyTransactionWindow.xaml</summary>
    public partial class ModifyTransactionWindow : INotifyPropertyChanged
    {
        private readonly List<Account> _allAccounts = AppState.AllAccounts;
        private readonly List<Category> _allCategories = AppState.AllCategories;
        private Category _selectedCategory = new Category();
        private Account _selectedAccount = new Account();
        private Transaction _modifyTransaction = new Transaction();
        internal ViewAccountWindow PreviousWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        internal void SetCurrentTransaction(Transaction setTransaction, Account setAccount)
        {
            TransactionDate.SelectedDate = setTransaction.Date;
            CmbAccount.SelectedValue = setAccount;
            CmbMajorCategory.SelectedItem = _allCategories.Find(category => category.Name == setTransaction.MajorCategory);
            CmbMinorCategory.SelectedItem = setTransaction.MinorCategory;
            TxtPayee.Text = setTransaction.Payee;
            TxtOutflow.Text = setTransaction.Outflow.ToString(CultureInfo.InvariantCulture);
            TxtInflow.Text = setTransaction.Inflow.ToString(CultureInfo.InvariantCulture);
            _modifyTransaction = setTransaction;
            _selectedAccount = setAccount;
        }

        #region Button-Click Methods

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Transaction newTransaction = new Transaction(
                date: DateTimeHelper.Parse(TransactionDate.SelectedDate),
                payee: TxtPayee.Text,
                majorCategory: CmbMajorCategory.SelectedItem.ToString(),
                minorCategory: CmbMinorCategory.SelectedItem.ToString(),
                memo: TxtMemo.Text,
                outflow: DecimalHelper.Parse(TxtOutflow.Text),
                inflow: DecimalHelper.Parse(TxtInflow.Text),
                account: _selectedAccount.Name);

            if (newTransaction != _modifyTransaction)
            {
                if (newTransaction.Account != _modifyTransaction.Account)
                {
                    int index = _allAccounts.FindIndex(account => account.Name == _modifyTransaction.Account);
                    _allAccounts[index].ModifyTransaction(_allAccounts[index].AllTransactions.IndexOf(_modifyTransaction),
                        newTransaction);
                    index = _allAccounts.FindIndex(account => account.Name == newTransaction.Account);
                    _allAccounts[index].AddTransaction(newTransaction);
                }
                else
                {
                    int index = _allAccounts.FindIndex(account => account.Name == _selectedAccount.Name);
                    _allAccounts[index].ModifyTransaction(_allAccounts[index].AllTransactions.IndexOf(_modifyTransaction), newTransaction);
                }
                if (await AppState.ModifyTransaction(newTransaction, _modifyTransaction))
                    CloseWindow();
                else
                    new Notification("Unable to modify transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
            }
            else
                new Notification("This transaction has not been modified.", "Finances", NotificationButtons.OK, this).ShowDialog();
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
            if (TransactionDate.SelectedDate != null && CmbMajorCategory.SelectedIndex >= 0 && CmbMinorCategory.SelectedIndex >= 0 && TxtPayee.Text.Length > 0 && (TxtInflow.Text.Length > 0 | TxtOutflow.Text.Length > 0) && CmbAccount.SelectedIndex >= 0)
                BtnSave.IsEnabled = true;
            else
                BtnSave.IsEnabled = false;
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
            CmbMinorCategory.IsEnabled = CmbMajorCategory.SelectedIndex >= 0;
            _selectedCategory = CmbMajorCategory.SelectedIndex >= 0
                ? (Category)CmbMajorCategory.SelectedItem
                : new Category();

            CmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
            TextChanged();
        }

        private void CmbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedAccount = CmbAccount.SelectedIndex >= 0 ? (Account)CmbAccount.SelectedValue : new Account();
            TextChanged();
        }

        #endregion Text/Selection Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public ModifyTransactionWindow()
        {
            InitializeComponent();
            CmbAccount.ItemsSource = _allAccounts;
            CmbMajorCategory.ItemsSource = _allCategories;
            CmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
        }

        private void TxtInflowOutflow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Decimals);
        }

        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void WindowModifyTransaction_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}