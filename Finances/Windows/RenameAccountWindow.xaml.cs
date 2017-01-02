using Extensions;
using System.ComponentModel;
using System.Windows;

namespace Finances
{
    /// <summary>
    /// Interaction logic for RenameAccountWindow.xaml
    /// </summary>
    public partial class RenameAccountWindow
    {
        private Account selectedAccount;
        internal ViewAccountWindow RefToViewAccountWindow { private get; set; }

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
                    new Notification("Unable to process account name change.", "Finances", NotificationButtons.OK, this).ShowDialog();
            }
            else
                new Notification("The account name can't be changed to what it already is.", "Finances", NotificationButtons.OK, this).ShowDialog();
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

        private void windowRenameAccount_Closing(object sender, CancelEventArgs e)
        {
            RefToViewAccountWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}