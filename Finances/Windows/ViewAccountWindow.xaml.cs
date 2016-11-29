using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>
    /// Interaction logic for ViewAccountWindow.xaml
    /// </summary>
    public partial class ViewAccountWindow : Window, INotifyPropertyChanged
    {
        private Account selectedAccount = new Account();
        private Transaction selectedTransaction = new Transaction();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        internal MainWindow RefToMainWindow { get; set; }

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

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

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
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvTransactions.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvTransactions.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void btnNewTransaction_Click(object sender, RoutedEventArgs e)
        {
            NewTransactionWindow newTransactionWindow = new NewTransactionWindow();
            newTransactionWindow.RefToViewAccountWindow = this;
            newTransactionWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnModifyTransaction_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnRenameAccount_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void btnDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this account? All transactions associated with it will be lost forever!", "Finances", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (await AppState.DeleteAccount(selectedAccount))
                {
                    RefToMainWindow.RefreshItemsSource();
                    CloseWindow();
                }
            }
        }

        private void btnNewTransfer_Click(object sender, RoutedEventArgs e)
        {
            NewTransferWindow newTransferWindow = new NewTransferWindow();
            newTransferWindow.RefToViewAccountWindow = this;
            newTransferWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnSearchTransactions_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void btnDeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this transaction? All data associated with it will be lost forever!", "Finances", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                selectedAccount.RemoveTransaction(selectedTransaction);
                if (await AppState.DeleteTransaction(selectedTransaction, selectedAccount))
                    lvTransactions.UnselectAll();
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