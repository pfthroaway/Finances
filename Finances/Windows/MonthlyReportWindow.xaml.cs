﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>
    /// Interaction logic for MonthlyReportWindow.xaml
    /// </summary>
    public partial class MonthlyReportWindow : Window, INotifyPropertyChanged
    {
        private List<Month> AllMonths = AppState.AllMonths;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        internal MainWindow RefToMainWindow { get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click methods

        private void btnViewCategorizedReport_Click(object sender, RoutedEventArgs e)
        {
            Month selectedMonth = (Month)lvMonths.SelectedValue;
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

            CategorizedMonthlyReportWindow categorizedMonthlyReportWindow = new CategorizedMonthlyReportWindow();
            categorizedMonthlyReportWindow.RefToMonthlyReportWindow = this;
            categorizedMonthlyReportWindow.LoadMonth(selectedMonth, categorizedExpenses);
            categorizedMonthlyReportWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void lvMonths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvMonths.SelectedIndex >= 0)
                btnViewCategorizedReport.IsEnabled = true;
            else
                btnViewCategorizedReport.IsEnabled = false;
        }

        #endregion Button-Click methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public MonthlyReportWindow()
        {
            InitializeComponent();
            lvMonths.ItemsSource = AllMonths;
        }

        private void lvMonthsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvMonths.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvMonths.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void windowMonthlyReport_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}