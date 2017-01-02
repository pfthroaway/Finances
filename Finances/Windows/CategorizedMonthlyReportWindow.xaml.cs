using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for DetailedMonthlyReportWindow.xaml</summary>
    public partial class CategorizedMonthlyReportWindow
    {
        private Month currentMonth = new Month();
        private List<CategorizedExpense> AllCategorizedExpenses = new List<CategorizedExpense>();
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

        internal MonthlyReportWindow RefToMonthlyReportWindow { private get; set; }

        internal void LoadMonth(Month selectedMonth, List<CategorizedExpense> expenses)
        {
            AllCategorizedExpenses = expenses;
            currentMonth = selectedMonth;
            lvCategorized.ItemsSource = AllCategorizedExpenses;
            DataContext = currentMonth;
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

        public CategorizedMonthlyReportWindow()
        {
            InitializeComponent();
        }

        private void lvCategorizedColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    lvCategorized.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                lvCategorized.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        private void windowCategorizedMonthlyReport_Closing(object sender, CancelEventArgs e)
        {
            RefToMonthlyReportWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}