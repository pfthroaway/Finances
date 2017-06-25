using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Windows
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private List<Account> _allAccounts = new List<Account>();
        private ListViewSort _sort = new ListViewSort();

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
            Accounts.NewAccountWindow newAccountWindow = new Accounts.NewAccountWindow { PreviousWindow = this };
            newAccountWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void LVAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnViewTransactions.IsEnabled = LVAccounts.SelectedIndex >= 0;
        }

        private void BtnManageCategories_Click(object sender, RoutedEventArgs e)
        {
            Categories.ManageCategoriesWindow manageCategoriesWindow = new Categories.ManageCategoriesWindow { RefToMainWindow = this };
            manageCategoriesWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnMonthlyReport_Click(object sender, RoutedEventArgs e)
        {
            Reports.MonthlyReportWindow monthlyReportWindow = new Reports.MonthlyReportWindow { PreviousWindow = this };
            monthlyReportWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnViewAccount_Click(object sender, RoutedEventArgs e)
        {
            Account selectedAccount = (Account)(LVAccounts.SelectedValue);
            Accounts.ViewAccountWindow viewAccountWindow = new Accounts.ViewAccountWindow { PreviousWindow = this };
            viewAccountWindow.LoadAccount(selectedAccount);
            viewAccountWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnViewAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            Transactions.ViewAllTransactionsWindow viewAllTransactionsWindow = new Transactions.ViewAllTransactionsWindow { PreviousWindow = this };
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
            _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVAccounts, "#BDC7C1");
        }

        #endregion Window-Manipulation Methods
    }
}