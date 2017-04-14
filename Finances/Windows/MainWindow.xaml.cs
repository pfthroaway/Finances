using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private List<Account> _allAccounts = new List<Account>();
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

        private void BtnNewAccount_Click(object sender, RoutedEventArgs e)
        {
            NewAccountWindow newAccountWindow = new NewAccountWindow { PreviousWindow = this };
            newAccountWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void LVAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnViewTransactions.IsEnabled = LVAccounts.SelectedIndex >= 0;
        }

        private void BtnManageCategories_Click(object sender, RoutedEventArgs e)
        {
            ManageCategoriesWindow manageCategoriesWindow = new ManageCategoriesWindow { RefToMainWindow = this };
            manageCategoriesWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnMonthlyReport_Click(object sender, RoutedEventArgs e)
        {
            MonthlyReportWindow monthlyReportWindow = new MonthlyReportWindow { PreviousWindow = this };
            monthlyReportWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnViewAccount_Click(object sender, RoutedEventArgs e)
        {
            Account selectedAccount = (Account)(LVAccounts.SelectedValue);
            ViewAccountWindow viewAccountWindow = new ViewAccountWindow { PreviousWindow = this };
            viewAccountWindow.LoadAccount(selectedAccount);
            viewAccountWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnViewAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            ViewAllTransactionsWindow viewAllTransactionsWindow = new ViewAllTransactionsWindow { PreviousWindow = this };
            viewAllTransactionsWindow.Show();
            Visibility = Visibility.Hidden;
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
            _allAccounts = AppState.AllAccounts;
            LVAccounts.ItemsSource = _allAccounts;
            LVAccounts.Items.Refresh();
        }

        private async void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadAll();
            _allAccounts = AppState.AllAccounts;
            LVAccounts.ItemsSource = _allAccounts;
        }

        private void LVAccountsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    LVAccounts.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                LVAccounts.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        #endregion Window-Manipulation Methods
    }
}