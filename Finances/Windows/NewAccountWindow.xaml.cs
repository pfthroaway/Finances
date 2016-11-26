using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
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
    /// Interaction logic for NewAccountWindow.xaml
    /// </summary>
    public partial class NewAccountWindow : Window
    {
        internal MainWindow RefToMainWindow { get; set; }
              
        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            Account newAccount = new Account(txtAccName.Text, DecimalHelper.Parse(txtBalance.Text), new List<Transaction>());
            AppState.AllAccounts.Add(newAccount);
            if (await AppState.AddAccount(newAccount))                
                CloseWindow();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion

        #region Text Changed

        private void TextChanged()
        {
            if (txtAccName.Text.Length > 0 && txtBalance.Text.Length > 0)
                btnSubmit.IsEnabled = true;
             else
                btnSubmit.IsEnabled = false;            
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        #endregion

        #region Window-Manipulation Methods

        public NewAccountWindow()
        {
            InitializeComponent();
        }

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        private void windowNewAccount_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }

        #endregion
    }
}
