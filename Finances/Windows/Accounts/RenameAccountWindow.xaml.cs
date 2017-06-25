using Finances.Classes;
using Finances.Classes.Data;
using System.ComponentModel;
using System.Windows;

namespace Finances.Windows.Accounts
{
    /// <summary>Interaction logic for RenameAccountWindow.xaml</summary>
    public partial class RenameAccountWindow
    {
        private Account _selectedAccount;
        internal ViewAccountWindow PreviousWindow { private get; set; }

        internal void LoadAccountName(Account currentAccount)
        {
            _selectedAccount = currentAccount;
            TxtAccountName.Text = _selectedAccount.Name;
        }

        #region Button-Click Methods

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (TxtAccountName.Text != _selectedAccount.Name)
            {
                if (await AppState.RenameAccount(_selectedAccount, TxtAccountName.Text))
                {
                    PreviousWindow.RefreshItemsSource();
                    CloseWindow();
                }
                else
                    AppState.DisplayNotification("Unable to process account name change.", "Finances", this);
            }
            else
                AppState.DisplayNotification("The account name can't be changed to what it already is.", "Finances", this);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public RenameAccountWindow()
        {
            InitializeComponent();
        }

        private void WindowRenameAccount_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}