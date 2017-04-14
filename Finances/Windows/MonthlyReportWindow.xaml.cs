using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for MonthlyReportWindow.xaml</summary>
    public partial class MonthlyReportWindow : INotifyPropertyChanged
    {
        private readonly List<Month> _allMonths = AppState.AllMonths;
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

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
                    foreach (string minorCategory in category.MinorCategories)
                        categorizedExpenses.Add(new CategorizedExpense(category.Name, minorCategory, 0.00M, 0.00M));
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

            CategorizedMonthlyReportWindow categorizedMonthlyReportWindow = new CategorizedMonthlyReportWindow
            {
                PreviousWindow = this
            };
            categorizedMonthlyReportWindow.LoadMonth(selectedMonth, categorizedExpenses);
            categorizedMonthlyReportWindow.Show();
            Visibility = Visibility.Hidden;
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
            Close();
        }

        public MonthlyReportWindow()
        {
            InitializeComponent();
            LVMonths.ItemsSource = _allMonths;
        }

        private void LVMonthsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    LVMonths.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                LVMonths.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        private void WindowMonthlyReport_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}