using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private List<Account> AllAccounts = new List<Account>();
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click Methods

        private void btnNewAccount_Click(object sender, RoutedEventArgs e)
        {
            NewAccountWindow newAccountWindow = new NewAccountWindow { RefToMainWindow = this };
            newAccountWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void lvAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnViewTransactions.IsEnabled = lvAccounts.SelectedIndex >= 0;
        }

        private void btnManageCategories_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnMonthlyReport_Click(object sender, RoutedEventArgs e)
        {
            MonthlyReportWindow monthlyReportWindow = new MonthlyReportWindow { RefToMainWindow = this };
            monthlyReportWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnViewAccount_Click(object sender, RoutedEventArgs e)
        {
            Account selectedAccount = (Account)(lvAccounts.SelectedValue);
            ViewAccountWindow viewAccountWindow = new ViewAccountWindow { RefToMainWindow = this };
            viewAccountWindow.LoadAccount(selectedAccount);
            viewAccountWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnViewAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            ViewAllTransactionsWindow viewAllTransactionsWindow = new ViewAllTransactionsWindow { RefToMainWindow = this };
            viewAllTransactionsWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            AllAccounts = AppState.AllAccounts;
            lvAccounts.ItemsSource = AllAccounts;
            lvAccounts.Items.Refresh();
        }

        private async void windowMain_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadAll();
            AllAccounts = AppState.AllAccounts;
            lvAccounts.ItemsSource = AllAccounts;
        }

        private void lvAccountsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    lvAccounts.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                lvAccounts.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        #endregion Window-Manipulation Methods
    }
}