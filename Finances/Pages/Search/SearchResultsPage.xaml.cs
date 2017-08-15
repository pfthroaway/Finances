using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Finances.Pages.Search
{
    /// <summary>Interaction logic for SearchResultsWindow.xaml</summary>
    public partial class SearchResultsPage : INotifyPropertyChanged
    {
        private List<Transaction> _allTransactions;
        private ListViewSort _sort = new ListViewSort();

        public string TransactionCount => $"Transaction Count: {_allTransactions.Count}";

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        internal void LoadWindow(List<Transaction> matchingTransactions)
        {
            _allTransactions = matchingTransactions.OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.ID).ToList();
            LVTransactions.ItemsSource = _allTransactions;
        }

        #region Click

        private void BtnBack_Click(object sender, RoutedEventArgs e) => ClosePage();

        private void LVTransactionsColumnHeader_Click(object sender, RoutedEventArgs e) => _sort =
            Functions.ListViewColumnHeaderClick(sender, _sort, LVTransactions, "#CCCCCC");

        #endregion Click

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public SearchResultsPage() => InitializeComponent();

        private void SearchResultsPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        #endregion Page-Manipulation Methods
    }
}