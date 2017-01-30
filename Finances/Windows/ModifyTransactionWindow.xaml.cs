using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances
{
    /// <summary>
    /// Interaction logic for ModifyTransactionWindow.xaml
    /// </summary>
    public partial class ModifyTransactionWindow : INotifyPropertyChanged
    {
        private readonly List<Account> AllAccounts = AppState.AllAccounts;
        private readonly List<Category> AllCategories = AppState.AllCategories;
        private Category selectedCategory = new Category();
        private Account selectedAccount = new Account();

        private Transaction modifyTransaction = new Transaction();
        internal ViewAccountWindow RefToViewAccountWindow { private get; set; }

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
            txtOutflow.Text = setTransaction.Outflow.ToString(CultureInfo.InvariantCulture);
            txtInflow.Text = setTransaction.Inflow.ToString(CultureInfo.InvariantCulture);
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
                if (newTransaction.Account != modifyTransaction.Account)
                {
                    int index = AllAccounts.FindIndex(account => account.Name == modifyTransaction.Account);
                    AllAccounts[index].ModifyTransaction(AllAccounts[index].AllTransactions.IndexOf(modifyTransaction),
                        newTransaction);
                    index = AllAccounts.FindIndex(account => account.Name == newTransaction.Account);
                    AllAccounts[index].AddTransaction(newTransaction);
                }
                else
                {
                    int index = AllAccounts.FindIndex(account => account.Name == selectedAccount.Name);
                    AllAccounts[index].ModifyTransaction(AllAccounts[index].AllTransactions.IndexOf(modifyTransaction), newTransaction);
                }
                if (await AppState.ModifyTransaction(newTransaction, modifyTransaction))
                    CloseWindow();
                else
                    new Notification("Unable to modify transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
            }
            else
                new Notification("This transaction has not been modified.", "Finances", NotificationButtons.OK, this).ShowDialog();
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
            cmbMinorCategory.IsEnabled = cmbMajorCategory.SelectedIndex >= 0;
            selectedCategory = cmbMajorCategory.SelectedIndex >= 0
                ? (Category)cmbMajorCategory.SelectedItem
                : new Category();

            cmbMinorCategory.ItemsSource = selectedCategory.MinorCategories;
            TextChanged();
        }

        private void cmbAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedAccount = cmbAccount.SelectedIndex >= 0 ? (Account)cmbAccount.SelectedValue : new Account();
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

        private void txtInflowOutflow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Decimals);
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void windowModifyTransaction_Closing(object sender, CancelEventArgs e)
        {
            RefToViewAccountWindow.RefreshItemsSource();
            RefToViewAccountWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}