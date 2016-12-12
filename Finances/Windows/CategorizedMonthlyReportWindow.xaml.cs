using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for DetailedMonthlyReportWindow.xaml</summary>
    public partial class CategorizedMonthlyReportWindow : Window
    {
        private Month currentMonth = new Month();
        private List<CategorizedExpense> AllCategorizedExpenses = new List<CategorizedExpense>();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        internal MonthlyReportWindow RefToMonthlyReportWindow { get; set; }

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
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvCategorized.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvCategorized.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void windowCategorizedMonthlyReport_Closing(object sender, CancelEventArgs e)
        {
            RefToMonthlyReportWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}