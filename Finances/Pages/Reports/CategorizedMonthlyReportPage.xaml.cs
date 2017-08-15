using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Sorting;
using System.Collections.Generic;
using System.Windows;

namespace Finances.Pages.Reports
{
    /// <summary>Interaction logic for DetailedMonthlyReportWindow.xaml</summary>
    public partial class CategorizedMonthlyReportPage
    {
        private Month _currentMonth = new Month();
        private List<CategorizedExpense> _allCategorizedExpenses = new List<CategorizedExpense>();
        private ListViewSort _sort = new ListViewSort();

        internal void LoadMonth(Month selectedMonth, List<CategorizedExpense> expenses)
        {
            _allCategorizedExpenses = expenses;
            _currentMonth = selectedMonth;
            LVCategorized.ItemsSource = _allCategorizedExpenses;
            DataContext = _currentMonth;
        }

        #region Click

        private void BtnBack_Click(object sender, RoutedEventArgs e) => ClosePage();

        private void LVCategorizedColumnHeader_Click(object sender, RoutedEventArgs e) => _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVCategorized, "#CCCCCC");

        #endregion Click

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public CategorizedMonthlyReportPage() => InitializeComponent();

        private void CategorizedMonthlyReportPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        #endregion Page-Manipulation Methods
    }
}