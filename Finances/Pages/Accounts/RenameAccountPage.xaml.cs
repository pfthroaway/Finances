using Finances.Classes;
using Finances.Classes.Data;
using System.Windows;

namespace Finances.Pages.Accounts
{
    /// <summary>Interaction logic for RenameAccountWindow.xaml</summary>
    public partial class RenameAccountPage
    {
        private Account _selectedAccount;
        internal ViewAccountPage PreviousWindow { private get; set; }

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
                    AppState.DisplayNotification("Unable to process account name change.", "Finances");
            }
            else
                AppState.DisplayNotification("The account name can't be changed to what it already is.", "Finances");
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
            AppState.GoBack();
        }

        public RenameAccountPage()
        {
            InitializeComponent();
        }

        #endregion Window-Manipulation Methods

        private void RenameAccountPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
        }
    }
}