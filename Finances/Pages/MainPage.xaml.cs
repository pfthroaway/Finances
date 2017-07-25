using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using Finances.Pages.Accounts;
using Finances.Pages.Categories;
using Finances.Pages.Reports;
using Finances.Pages.Transactions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages
{
    /// <summary>Interaction logic for MainPage.xaml</summary>
    public partial class MainPage
    {
        private List<Account> _allAccounts = new List<Account>();
        private ListViewSort _sort = new ListViewSort();

        #region Button-Click Methods

        private void BtnNewAccount_Click(object sender, RoutedEventArgs e)
        {
            AppState.Navigate(new NewAccountPage { PreviousWindow = this });
        }

        private void LVAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnViewTransactions.IsEnabled = LVAccounts.SelectedIndex >= 0;
        }

        private void BtnManageCategories_Click(object sender, RoutedEventArgs e)
        {
            AppState.Navigate(new ManageCategoriesPage());
        }

        private void BtnMonthlyReport_Click(object sender, RoutedEventArgs e)
        {
            AppState.Navigate(new MonthlyReportPage());
        }

        private void BtnViewAccount_Click(object sender, RoutedEventArgs e)
        {
            Account selectedAccount = (Account)(LVAccounts.SelectedValue);
            ViewAccountPage viewAccountWindow = new ViewAccountPage { PreviousWindow = this };
            viewAccountWindow.LoadAccount(selectedAccount);
            AppState.Navigate(viewAccountWindow);
        }

        private void BtnViewAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            AppState.Navigate(new ViewAllTransactionsPage());
        }

        #endregion Button-Click Methods

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            _allAccounts = AppState.AllAccounts;
            LVAccounts.ItemsSource = _allAccounts;
            LVAccounts.Items.Refresh();
        }

        private void LVAccountsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVAccounts, "#BDC7C1");
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
            _allAccounts = AppState.AllAccounts;
            LVAccounts.ItemsSource = _allAccounts;
        }
    }
}