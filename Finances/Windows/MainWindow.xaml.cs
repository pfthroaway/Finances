using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Finances
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<Account> AllAccounts = new List<Account>();
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click Methods

        private void btnNewTransaction_Click(object sender, RoutedEventArgs e)
        {
            NewTransactionWindow newTransactionWindow = new NewTransactionWindow();
            newTransactionWindow.RefToMainWindow = this;
            newTransactionWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnNewTransfer_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnNewAccount_Click(object sender, RoutedEventArgs e)
        {
        }

        private void lvAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvAccounts.SelectedIndex >= 0)
                btnViewTransactions.IsEnabled = true;
            else
                btnViewTransactions.IsEnabled = false;
        }

        private void btnManageCategories_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnViewTransactions_Click(object sender, RoutedEventArgs e)
        {
            ViewTransactionsWindow viewTransactionsWindow = new ViewTransactionsWindow();
            viewTransactionsWindow.RefToMainWindow = this;
            viewTransactionsWindow.LoadAccount((Account)lvAccounts.SelectedValue);
            viewTransactionsWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void windowMain_Loaded(object sender, RoutedEventArgs e)
        {
            await AppState.LoadAll();
            AllAccounts = AppState.AllAccounts;
            lvAccounts.ItemsSource = AllAccounts;
        }

        private void lvAccountsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvAccounts.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvAccounts.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        #endregion Window-Manipulation Methods
    }
}