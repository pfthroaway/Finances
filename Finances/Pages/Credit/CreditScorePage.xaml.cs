using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages.Credit
{
    /// <summary>Interaction logic for CreditScorePage.xaml</summary>
    public partial class CreditScorePage
    {
        private List<CreditScore> _allScores = new List<CreditScore>();
        private ListViewSort _sort = new ListViewSort();
        private CreditScore _selectedScore = new CreditScore();

        #region Click

        private void BtnAddCreditScore_Click(object sender, RoutedEventArgs e) =>
            AppState.Navigate(new AddCreditScorePage());

        private void BtnBack_Click(object sender, RoutedEventArgs e) => AppState.GoBack();

        private async void BtnDeleteCreditScore_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.YesNoNotification(
                "Are you sure you want to delete this credit score? This action cannot be undone.", "Finances"))
            {
                await AppState.DeleteCreditScore(_selectedScore);
                RefreshItemsSource();
            }
        }

        private void BtnModifyCreditScore_Click(object sender, RoutedEventArgs e) => AppState.Navigate(
            new ModifyCreditScorePage { SelectedCreditScore = _selectedScore });

        private void LVScoresColumnHeader_Click(object sender, RoutedEventArgs e) => _sort =
            Functions.ListViewColumnHeaderClick(sender, _sort, LVScores, "#CCCCCC");

        #endregion Click

        #region Page-Manipulation Methods

        /// <summary>Loads all scores from the database.</summary>
        private async Task LoadScores() => _allScores = await AppState.LoadCreditScores();

        /// <summary>Refreshes the ItemsSource of LVScores.</summary>
        private void RefreshItemsSource()
        {
            LVScores.ItemsSource = _allScores;
            LVScores.Items.Refresh();
        }

        public CreditScorePage() => InitializeComponent();

        private async void CreditScorePage_Loaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
            await LoadScores();
            RefreshItemsSource();
        }

        private void LVScores_SelectionChanged(object sender, SelectionChangedEventArgs e) => _selectedScore =
            LVScores.SelectedIndex >= 0 ? (CreditScore)LVScores.SelectedItem : new CreditScore();

        #endregion Page-Manipulation Methods
    }
}