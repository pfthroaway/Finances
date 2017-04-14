using Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for ViewAccountWindow.xaml</summary>
    public partial class ViewAccountWindow : INotifyPropertyChanged
    {
        private Account _selectedAccount = new Account();
        private Transaction _selectedTransaction = new Transaction();
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

        internal MainWindow PreviousWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        internal void LoadAccount(Account account)
        {
            _selectedAccount = account;
            LVTransactions.ItemsSource = _selectedAccount.AllTransactions;
            DataContext = _selectedAccount;
        }

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            _selectedAccount = AppState.AllAccounts.Find(account => account.Name == _selectedAccount.Name);
            LVTransactions.ItemsSource = _selectedAccount.AllTransactions;
            LVTransactions.Items.Refresh();
            DataContext = _selectedAccount;
        }

        #region Button-Click Methods

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private async void BtnDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (new Notification("Are you sure you want to delete this account? All transactions associated with it will be lost forever!", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
            {
                if (await AppState.DeleteAccount(_selectedAccount))
                {
                    PreviousWindow.RefreshItemsSource();
                    CloseWindow();
                }
            }
        }

        private async void BtnDeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (new Notification("Are you sure you want to delete this transaction? All data associated with it will be lost forever!", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
            {
                _selectedAccount.RemoveTransaction(_selectedTransaction);
                if (await AppState.DeleteTransaction(_selectedTransaction, _selectedAccount))
                {
                    LVTransactions.UnselectAll();
                    RefreshItemsSource();
                }
            }
        }

        private void BtnModifyTransaction_Click(object sender, RoutedEventArgs e)
        {
            ModifyTransactionWindow modifyTransactionWindow = new ModifyTransactionWindow
            {
                PreviousWindow = this
            };
            modifyTransactionWindow.SetCurrentTransaction(_selectedTransaction, _selectedAccount);
            modifyTransactionWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnNewTransaction_Click(object sender, RoutedEventArgs e)
        {
            NewTransactionWindow newTransactionWindow = new NewTransactionWindow { PreviousWindow = this };
            newTransactionWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnNewTransfer_Click(object sender, RoutedEventArgs e)
        {
            NewTransferWindow newTransferWindow = new NewTransferWindow { PreviousWindow = this };
            newTransferWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnRenameAccount_Click(object sender, RoutedEventArgs e)
        {
            RenameAccountWindow renameAccountWindow = new RenameAccountWindow { PreviousWindow = this };
            renameAccountWindow.LoadAccountName(_selectedAccount);
            renameAccountWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnSearchTransactions_Click(object sender, RoutedEventArgs e)
        {
            SearchTransactionsWindow searchTransactionsWindow = new SearchTransactionsWindow
            {
                PreviousWindow = this
            };
            searchTransactionsWindow.Show();
            Visibility = Visibility.Hidden;
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public ViewAccountWindow()
        {
            InitializeComponent();
        }

        private void LVTransactionsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    LVTransactions.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                LVTransactions.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        private void LVTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LVTransactions.SelectedIndex >= 0)
            {
                _selectedTransaction = (Transaction)LVTransactions.SelectedValue;
                BtnDeleteTransaction.IsEnabled = true;
                BtnModifyTransaction.IsEnabled = true;
            }
            else
            {
                _selectedTransaction = new Transaction();
                BtnDeleteTransaction.IsEnabled = false;
                BtnModifyTransaction.IsEnabled = false;
            }
        }

        private void WindowViewAccount_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
            PreviousWindow.RefreshItemsSource();
        }

        #endregion Window-Manipulation Methods
    }
}