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

        internal MainWindow PreviousWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click methods

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
                    categorizedExpenses.Find(expense => expense.MajorCategory == transaction.MajorCategory && expense.MinorCategory == "").AddTransactionValues(transaction.Outflow, transaction.Inflow);
                }
            }

            categorizedExpenses = categorizedExpenses.OrderBy(expense => expense.MajorCategory).ThenBy(expense => expense.MinorCategory).ToList();

            CategorizedMonthlyReportPage categorizedMonthlyReportWindow = new CategorizedMonthlyReportPage
            {
                PreviousWindow = this
            };
            categorizedMonthlyReportWindow.LoadMonth(selectedMonth, categorizedExpenses);

            AppState.Navigate(categorizedMonthlyReportWindow);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void LVMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnViewCategorizedReport.IsEnabled = LVMonths.SelectedIndex >= 0;
        }

        #endregion Button-Click methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            AppState.GoBack();
        }

        public MonthlyReportPage()
        {
            InitializeComponent();
            LVMonths.ItemsSource = _allMonths;
        }

        private void LVMonthsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVMonths, "#BDC7C1");
        }

        #endregion Window-Manipulation Methods

        private void MonthlyReportPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
        }
    }
}