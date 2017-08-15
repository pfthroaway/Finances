using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Finances.Pages.Transactions
{
    /// <summary>Interaction logic for ViewAllTransactionsWindow.xaml</summary>
    public partial class ViewAllTransactionsPage : INotifyPropertyChanged
    {
        private readonly List<Transaction> _allTransactions = AppState.AllTransactions.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.ID).ToList();
        private ListViewSort _sort = new ListViewSort();

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Click

        private void BtnBack_Click(object sender, RoutedEventArgs e) => ClosePage();

        private void LVTransactionsColumnHeader_Click(object sender, RoutedEventArgs e) => _sort =
            Functions.ListViewColumnHeaderClick(sender, _sort, LVTransactions, "#CCCCCC");

        #endregion Click

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public ViewAllTransactionsPage()
        {
            InitializeComponent();
            LVTransactions.ItemsSource = _allTransactions;
        }

        private void ViewAllTransactionsPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        #endregion Page-Manipulation Methods
    }
}