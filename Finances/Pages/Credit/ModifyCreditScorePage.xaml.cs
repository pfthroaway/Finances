using Extensions;
using Extensions.DataTypeHelpers;
using Extensions.Enums;
using Finances.Classes;
using Finances.Classes.Data;
using Finances.Classes.Enums;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finances.Pages.Credit
{
    /// <summary>Interaction logic for ModifyCreditScorePage.xaml</summary>
    public partial class ModifyCreditScorePage
    {
        internal CreditScore SelectedCreditScore { get; set; }

        private void TextChanged() => BtnSave.IsEnabled = ScoreDate.SelectedDate != DateTime.MinValue &&
            TxtSource.Text.Length > 0 && TxtScore.Text.Length > 0 && CmbProvider.SelectedIndex >= 0;

        /// <summary>Resets all controls to their default state.</summary>
        private void Reset()
        {
            ScoreDate.SelectedDate = SelectedCreditScore.Date;
            TxtSource.Text = SelectedCreditScore.Source;
            TxtScore.Text = SelectedCreditScore.Score.ToString();
            CmbProvider.SelectedItem = SelectedCreditScore.ProviderToString;
            ChkFICO.IsChecked = SelectedCreditScore.FICO;
            ScoreDate.Focus();
        }

        /// <summary>Adds credit score to database.</summary>
        private async Task<bool> ModifyCreditScore()
        {
            if (await AppState.ModifyCreditScore(SelectedCreditScore, new CreditScore(DateTimeHelper.Parse(ScoreDate.SelectedDate),
                TxtSource.Text.Trim(), Int32Helper.Parse(TxtScore.Text.Trim()),
                EnumHelper.Parse<Providers>(CmbProvider.SelectedItem.ToString()), ChkFICO?.IsChecked ?? false)))
                return true;

            AppState.DisplayNotification("Unable to modify credit score.", "Finances");
            return false;
        }

        #region Button-Click Methods

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (await ModifyCreditScore())
                ClosePage();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e) => Reset();

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ClosePage();

        #endregion Button-Click Methods

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public ModifyCreditScorePage()
        {
            InitializeComponent();
            ScoreDate.Focus();
            CmbProvider.Items.Add("TransUnion");
            CmbProvider.Items.Add("Experian");
            CmbProvider.Items.Add("Equifax");
        }

        private void AddCreditScorePage_Loaded(object sender, RoutedEventArgs e)
        {
            Reset();
            AppState.CalculateScale(Grid);
        }

        private void CmbProvider_SelectionChanged(object sender, SelectionChangedEventArgs e) => TextChanged();

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => TextChanged();

        private void Txt_TxtChanged(object sender, TextChangedEventArgs e) => TextChanged();

        private void Txt_GotFocus(object sender, RoutedEventArgs e) => Functions.TextBoxGotFocus(sender);

        private void Integer_PreviewKeyDown(object sender, KeyEventArgs e) => Functions.PreviewKeyDown(e,
            KeyType.Integers);

        #endregion Page-Manipulation Methods
    }
}