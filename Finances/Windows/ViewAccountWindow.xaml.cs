using Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>
    /// Interaction logic for ViewAccountWindow.xaml
    /// </summary>
    public partial class ViewAccountWindow : INotifyPropertyChanged
    {
        private Account selectedAccount = new Account();
        private Transaction selectedTransaction = new Transaction();
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

        internal MainWindow RefToMainWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        internal void LoadAccount(Account account)
        {
            selectedAccount = account;
            lvTransactions.ItemsSource = selectedAccount.AllTransactions;
            DataContext = selectedAccount;
        }

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            selectedAccount = AppState.AllAccounts.Find(account => account.Name == selectedAccount.Name);
            lvTransactions.ItemsSource = selectedAccount.AllTransactions;
            lvTransactions.Items.Refresh();
            DataContext = selectedAccount;
        }

        #region Button-Click Methods

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private async void btnDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (new Notification("Are you sure you want to delete this account? All transactions associated with it will be lost forever!", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
            {
                if (await AppState.DeleteAccount(selectedAccount))
                {
                    RefToMainWindow.RefreshItemsSource();
                    CloseWindow();
                }
            }
        }

        private async void btnDeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (new Notification("Are you sure you want to delete this transaction? All data associated with it will be lost forever!", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
            {
                selectedAccount.RemoveTransaction(selectedTransaction);
                if (await AppState.DeleteTransaction(selectedTransaction, selectedAccount))
                {
                    lvTransactions.UnselectAll();
                    RefreshItemsSource();
                }
            }
        }

        private void btnModifyTransaction_Click(object sender, RoutedEventArgs e)
        {
            ModifyTransactionWindow modifyTransactionWindow = new ModifyTransactionWindow
            {
                RefToViewAccountWindow = this
            };
            modifyTransactionWindow.SetCurrentTransaction(selectedTransaction, selectedAccount);
            modifyTransactionWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnNewTransaction_Click(object sender, RoutedEventArgs e)
        {
            NewTransactionWindow newTransactionWindow = new NewTransactionWindow { RefToViewAccountWindow = this };
            newTransactionWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnNewTransfer_Click(object sender, RoutedEventArgs e)
        {
            NewTransferWindow newTransferWindow = new NewTransferWindow { RefToViewAccountWindow = this };
            newTransferWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnRenameAccount_Click(object sender, RoutedEventArgs e)
        {
            RenameAccountWindow renameAccountWindow = new RenameAccountWindow { RefToViewAccountWindow = this };
            renameAccountWindow.LoadAccountName(selectedAccount);
            renameAccountWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnSearchTransactions_Click(object sender, RoutedEventArgs e)
        {
            SearchTransactionsWindow searchTransactionsWindow = new SearchTransactionsWindow
            {
                RefToViewAccountWindow = this
            };
            searchTransactionsWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public ViewAccountWindow()
        {
            InitializeComponent();
        }

        private void windowViewAccount_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
            RefToMainWindow.RefreshItemsSource();
        }

        #endregion Window-Manipulation Methods

        private void lvTransactionsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    lvTransactions.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                lvTransactions.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        private void lvTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTransactions.SelectedIndex >= 0)
            {
                selectedTransaction = (Transaction)lvTransactions.SelectedValue;
                btnDeleteTransaction.IsEnabled = true;
                btnModifyTransaction.IsEnabled = true;
            }
            else
            {
                selectedTransaction = new Transaction();
                btnDeleteTransaction.IsEnabled = false;
                btnModifyTransaction.IsEnabled = false;
            }
        }
    }
}