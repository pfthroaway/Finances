using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using Finances.Pages.Accounts;
using Finances.Pages.Categories;
using Finances.Pages.Credit;
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
        private List<Account> _allAccounts;
        private ListViewSort _sort = new ListViewSort();

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            _allAccounts = AppState.AllAccounts;
            LVAccounts.ItemsSource = _allAccounts;
            LVAccounts.Items.Refresh();
        }

        #region Click Methods

        private void BtnNewAccount_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new NewAccountPage());

        private void LVAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e) => BtnViewTransactions.IsEnabled = LVAccounts.SelectedIndex >= 0;

        private void BtnManageCategories_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new ManageCategoriesPage());

        private void BtnMonthlyReport_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new MonthlyReportPage());

        private void BtnViewAccount_Click(object sender, RoutedEventArgs e)
        {
            Account selectedAccount = (Account)LVAccounts.SelectedValue;
            ViewAccountPage viewAccountWindow = new ViewAccountPage();
            viewAccountWindow.LoadAccount(selectedAccount);
            AppState.Navigate(viewAccountWindow);
        }

        private void BtnViewCreditScores_Click(object sender, RoutedEventArgs e) =>
            AppState.Navigate(new CreditScorePage());

        private void BtnViewAllTransactions_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new ViewAllTransactionsPage());

        private void LVAccountsColumnHeader_Click(object sender, RoutedEventArgs e) => _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVAccounts, "#CCCCCC");

        #endregion Click Methods

        #region Page-Manipulation Methods

        public MainPage() => InitializeComponent();

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
            RefreshItemsSource();
        }

        #endregion Page-Manipulation Methods
    }
}