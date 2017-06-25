using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Finances.Windows.Search
{
    /// <summary>Interaction logic for SearchResultsWindow.xaml</summary>
    public partial class SearchResultsWindow : INotifyPropertyChanged
    {
        private List<Transaction> _allTransactions;
        private ListViewSort _sort = new ListViewSort();

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
            _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVTransactions, "#BDC7C1");
        }

        #endregion Window-Manipulation Methods
    }
}