using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Categories;
using Finances.Classes.Data;
using Finances.Classes.Sorting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages.Reports
{
    /// <summary>Interaction logic for MonthlyReportWindow.xaml</summary>
    public partial class MonthlyReportPage : INotifyPropertyChanged
    {
        private readonly List<Month> _allMonths = AppState.AllMonths;
        private ListViewSort _sort = new ListViewSort();

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Click methods

        private void BtnViewCategorizedReport_Click(object sender, RoutedEventArgs e)
        {
            Month selectedMonth = (Month)LVMonths.SelectedValue;
            List<CategorizedExpense> categorizedExpenses = new List<CategorizedExpense>();
            foreach (Category category in AppState.AllCategories)
            {
                if (category.Name != "Transfer")
                {
                    categorizedExpenses.Add(new CategorizedExpense(category.Name, "", 0.00M, 0.00M));
                    foreach (MinorCategory minorCategory in category.MinorCategories)
                        categorizedExpenses.Add(new CategorizedExpense(category.Name, minorCategory.Name, 0.00M, 0.00M));
                }
            }

            foreach (Transaction transaction in selectedMonth.AllTransactions)
            {
                if (transaction.MajorCategory != "Transfer")
                {
                    categorizedExpenses.Find(expense => expense.MajorCategory == transaction.MajorCategory && expense.MinorCategory == transaction.MinorCategory).AddTransactionValues(transaction.Outflow, transaction.Inflow);
                    categorizedExpenses.Find(expense => expense.MajorCategory == transaction.MajorCategory && expense.MinorCategory?.Length == 0).AddTransactionValues(transaction.Outflow, transaction.Inflow);
                }
            }

            categorizedExpenses = categorizedExpenses.OrderBy(expense => expense.MajorCategory).ThenBy(expense => expense.MinorCategory).ToList();

            CategorizedMonthlyReportPage categorizedMonthlyReportWindow = new CategorizedMonthlyReportPage();
            categorizedMonthlyReportWindow.LoadMonth(selectedMonth, categorizedExpenses);

            AppState.Navigate(categorizedMonthlyReportWindow);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => ClosePage();

        private void LVMonthsColumnHeader_Click(object sender, RoutedEventArgs e) => _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVMonths, "#CCCCCC");

        private void LVMonths_SelectionChanged(object sender, SelectionChangedEventArgs e) => BtnViewCategorizedReport.IsEnabled = LVMonths.SelectedIndex >= 0;

        #endregion Click methods

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public MonthlyReportPage()
        {
            InitializeComponent();
            LVMonths.ItemsSource = _allMonths;
        }

        private void MonthlyReportPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        #endregion Page-Manipulation Methods
    }
}