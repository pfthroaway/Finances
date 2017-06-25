using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Finances.Windows.Transactions
{
    /// <summary>Interaction logic for ViewAllTransactionsWindow.xaml</summary>
    public partial class ViewAllTransactionsWindow : INotifyPropertyChanged
    {
        private readonly List<Transaction> _allTransactions = AppState.AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
        private ListViewSort _sort = new ListViewSort();

        internal Windows.MainWindow PreviousWindow { private get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

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

        public ViewAllTransactionsWindow()
        {
            InitializeComponent();
            LVTransactions.ItemsSource = _allTransactions;
        }

        private void WindowViewAllTransactions_Closing(object sender, CancelEventArgs e)
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