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
    /// <summary>Interaction logic for AddCreditScorePage.xaml</summary>
    public partial class AddCreditScorePage
    {
        private void TextChanged() => ToggleButtons(ScoreDate.SelectedDate != DateTime.MinValue &&
            TxtSource.Text.Length > 0 && TxtScore.Text.Length > 0 && CmbProvider.SelectedIndex >= 0);

        /// <summary>Toggles whether the Save buttons are enabled.</summary>
        /// <param name="enabled"></param>
        private void ToggleButtons(bool enabled)
        {
            BtnSaveAndDone.IsEnabled = enabled;
            BtnSaveAndDuplicate.IsEnabled = enabled;
            BtnSaveAndNew.IsEnabled = enabled;
        }

        /// <summary>Resets all controls to their default state.</summary>
        private void Reset()
        {
            ScoreDate.Text = "";
            TxtSource.Text = "";
            TxtScore.Text = "";
            CmbProvider.SelectedIndex = -1;
            ChkFICO.IsChecked = false;
            ScoreDate.Focus();
        }

        /// <summary>Adds credit score to database.</summary>
        private async Task<bool> AddCreditScore()
        {
            if (await AppState.AddCreditScore(new CreditScore(DateTimeHelper.Parse(ScoreDate.SelectedDate),
                TxtSource.Text.Trim(), Int32Helper.Parse(TxtScore.Text.Trim()),
                EnumHelper.Parse<Providers>(CmbProvider.SelectedItem.ToString()), ChkFICO?.IsChecked ?? false)))
                return true;

            AppState.DisplayNotification("Unable to add credit score.", "Finances");
            return false;
        }

        #region Button-Click Methods

        private async void BtnSaveAndDone_Click(object sender, RoutedEventArgs e)
        {
            if (await AddCreditScore())
                ClosePage();
        }

        private async void BtnSaveAndDuplicate_Click(object sender, RoutedEventArgs e) => await AddCreditScore();

        private async void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (await AddCreditScore())
                Reset();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e) => Reset();

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ClosePage();

        #endregion Button-Click Methods

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public AddCreditScorePage()
        {
            InitializeComponent();
            ScoreDate.Focus();
            CmbProvider.Items.Add("TransUnion");
            CmbProvider.Items.Add("Experian");
            CmbProvider.Items.Add("Equifax");
        }

        private void AddCreditScorePage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        private void CmbProvider_SelectionChanged(object sender, SelectionChangedEventArgs e) => TextChanged();

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => TextChanged();

        private void Txt_TxtChanged(object sender, TextChangedEventArgs e) => TextChanged();

        private void Txt_GotFocus(object sender, RoutedEventArgs e) => Functions.TextBoxGotFocus(sender);

        private void Integer_PreviewKeyDown(object sender, KeyEventArgs e) => Functions.PreviewKeyDown(e,
            KeyType.Integers);

        #endregion Page-Manipulation Methods
    }
}