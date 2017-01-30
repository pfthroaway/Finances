using Extensions;
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

        internal ViewAccountWindow RefToViewAccountWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        private async Task<bool> AddTransaction()
        {
            Transaction newTransaction = new Transaction(
                date: DateTimeHelper.Parse(datePicker.SelectedDate),
                payee: txtPayee.Text,
                majorCategory: cmbMajorCategory.SelectedValue.ToString(),
                minorCategory: cmbMinorCategory.SelectedValue.ToString(),
                memo: txtMemo.Text,
                outflow: DecimalHelper.Parse(txtOutflow.Text),
                inflow: DecimalHelper.Parse(txtInflow.Text),
                account: _selectedAccount.Name);
            _selectedAccount.AddTransaction(newTransaction);
            AppState.AllTransactions.Add(newTransaction);

            return await AppState.AddTransaction(newTransaction, _selectedAccount);
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
        }

        /// <summary>Toggles Buttons on the Window.</summary>
        /// <param name="enabled">Should Button be enabled?</param>
        private void ToggleButtons(bool enabled)
        {
            btnSaveAndDone.IsEnabled = enabled;
            btnSaveAndDuplicate.IsEnabled = enabled;
            btnSaveAndNew.IsEnabled = enabled;
        }

        #region Button-Click Methods

        private async void btnSaveAndDone_Click(object sender, RoutedEventArgs e)
        {
            if (await AddTransaction())
                CloseWindow();
            else
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void btnSaveAndDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (!await AddTransaction())
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void btnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (await AddTransaction())
            {
                Reset();
                cmbMinorCategory.Focus();
            }
            else
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
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

        #region Text/Selection Changed

        /// <summary>Checks whether or not the Submit button should be enabled.</summary>
        private void TextChanged()
        {
            if (datePicker.SelectedDate != null && cmbMajorCategory.SelectedIndex >= 0 && cmbMinorCategory.SelectedIndex >= 0 && txtPayee.Text.Length > 0 && (txtInflow.Text.Length > 0 | txtOutflow.Text.Length > 0) && cmbAccount.SelectedIndex >= 0)
                ToggleButtons(true);
            else
                ToggleButtons(false);
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

        public NewTransactionWindow()
        {
            InitializeComponent();
            cmbAccount.ItemsSource = _allAccounts;
            cmbMajorCategory.ItemsSource = _allCategories;
            cmbMinorCategory.ItemsSource = _selectedCategory.MinorCategories;
        }

        private void windowNewTransaction_Closing(object sender, CancelEventArgs e)
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