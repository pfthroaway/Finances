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
        private Month _currentMonth = new Month();
        private List<CategorizedExpense> _allCategorizedExpenses = new List<CategorizedExpense>();
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

        internal MonthlyReportWindow PreviousWindow { private get; set; }

        internal void LoadMonth(Month selectedMonth, List<CategorizedExpense> expenses)
        {
            _allCategorizedExpenses = expenses;
            _currentMonth = selectedMonth;
            LVCategorized.ItemsSource = _allCategorizedExpenses;
            DataContext = _currentMonth;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public CategorizedMonthlyReportWindow()
        {
            InitializeComponent();
        }

        private void LVCategorizedColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    LVCategorized.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                LVCategorized.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        private void WindowCategorizedMonthlyReport_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}