using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Finances
{
    /// <summary>
    /// Interaction logic for RenameAccountWindow.xaml
    /// </summary>
    public partial class RenameAccountWindow : Window
    {
        private Account selectedAccount;
        internal ViewAccountWindow RefToViewAccountWindow { get; set; }

        internal void LoadAccountName(Account currentAccount)
        {
            selectedAccount = currentAccount;
            txtAccountName.Text = selectedAccount.Name;
        }

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (txtAccountName.Text != selectedAccount.Name)
            {
                if (await AppState.RenameAccount(selectedAccount, txtAccountName.Text))
                {
                    RefToViewAccountWindow.RefreshItemsSource();
                    CloseWindow();
                }
                else
                    MessageBox.Show("Unable to process account name change.", "Finances", MessageBoxButton.OK);
            }
            else
                MessageBox.Show("The account name can't be changed to what it already is.", "Finances", MessageBoxButton.OK);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public RenameAccountWindow()
        {
            InitializeComponent();
        }

        private void windowRenameAccount_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RefToViewAccountWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}