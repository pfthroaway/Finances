using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Finances
{
    /// <summary>Interaction logic for ViewAllTransactionsWindow.xaml</summary>
    public partial class ViewAllTransactionsWindow : INotifyPropertyChanged
    {
        private readonly List<Transaction> _allTransactions = AppState.AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
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