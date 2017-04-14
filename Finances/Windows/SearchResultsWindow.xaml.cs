using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for SearchResultsWindow.xaml</summary>
    public partial class SearchResultsWindow : INotifyPropertyChanged
    {
        private List<Transaction> _allTransactions;
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;

        internal SearchTransactionsWindow PreviousWindow { private get; set; }

        public string TransactionCount => $"Transaction Count: {_allTransactions.Count}";

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        internal void LoadWindow(List<Transaction> matchingTransactions)
        {
            _allTransactions = matchingTransactions.OrderByDescending(transaction => transaction.Date).ToList();
            LVTransactions.ItemsSource = _allTransactions;
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

        public SearchResultsWindow()
        {
            InitializeComponent();
        }

        private void WindowSearchResults_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
        }

        private void LVTransactionsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    LVTransactions.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                LVTransactions.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        #endregion Window-Manipulation Methods
    }
}