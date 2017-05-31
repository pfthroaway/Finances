using Extensions;
using Extensions.DataTypeHelpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>Interaction logic for NewTransactionWindow.xaml</summary>
    public partial class NewTransactionWindow : INotifyPropertyChanged
    {
        private readonly List<Account> _allAccounts = AppState.AllAccounts;
        private readonly List<Category> _allCategories = AppState.AllCategories;
        private Category _selectedCategory = new Category();
        private Account _selectedAccount = new Account();

        internal ViewAccountWindow PreviousWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        /// <summary>Attempts to add a Transaction to the database.</summary>
        /// <returns>Returns true if successfully added</returns>
        private async Task<bool> AddTransaction()
        {
            Transaction newTransaction = new Transaction(
                date: DateTimeHelper.Parse(TransactionDate.SelectedDate),
                payee: TxtPayee.Text,
                majorCategory: CmbMajorCategory.SelectedValue.ToString(),
                minorCategory: CmbMinorCategory.SelectedValue.ToString(),
                memo: TxtMemo.Text,
                outflow: DecimalHelper.Parse(TxtOutflow.Text),
                inflow: DecimalHelper.Parse(TxtInflow.Text),
                account: _selectedAccount.Name);
            _selectedAccount.AddTransaction(newTransaction);
            AppState.AllTransactions.Add(newTransaction);

            return await AppState.AddTransaction(newTransaction, _selectedAccount);
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
            if (await AddTransaction())
                CloseWindow();
            else
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void BtnSaveAndDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (!await AddTransaction())
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (await AddTransaction())
            {
                Reset();
                CmbMinorCategory.Focus();
            }
            else
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
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

        /// <summary>Checks whether or not the Save buttons should be enabled.</summary>
        private void TextChanged()
        {
            ToggleButtons(TransactionDate.SelectedDate != null && CmbMajorCategory.SelectedIndex >= 0 && CmbMinorCategory.SelectedIndex >= 0 && TxtPayee.Text.Length > 0 && (TxtInflow.Text.Length > 0 | TxtOutflow.Text.Length > 0) && CmbAccount.SelectedIndex >= 0);
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

        public NewTransactionWindow()
        {
            InitializeComponent();
            CmbAccount.ItemsSource = _allAccounts;
            CmbMajorCategory.ItemsSource = _allCategories;
            CmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
        }

        private void WindowNewTransaction_Closing(object sender, CancelEventArgs e)
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