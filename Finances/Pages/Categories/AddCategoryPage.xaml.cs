using Extensions;
using Finances.Classes;
using Finances.Classes.Categories;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages.Categories
{
    /// <summary>Interaction logic for AddCategoryWindow.xaml</summary>
    public partial class AddCategoryPage
    {
        private Category _majorCategory;
        private bool _isMajor;

        internal void LoadWindow(Category selectedMajorCategory, bool isMajor = false)
        {
            _majorCategory = selectedMajorCategory;
            _isMajor = isMajor;
            TxtName.Focus();
        }

        #region Button-Click Methods

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_isMajor)
            {
                if (AppState.AllCategories.All(category => category.Name != TxtName.Text))
                    if (await AppState.AddCategory(_majorCategory, TxtName.Text, _isMajor))
                        ClosePage();
            }
            else
            {
                if (!_majorCategory.MinorCategories.Contains(new MinorCategory(TxtName.Text)))
                    if (await AppState.AddCategory(_majorCategory, TxtName.Text, _isMajor))
                        ClosePage();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ClosePage();

        #endregion Button-Click Methods

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        public AddCategoryPage() => InitializeComponent();

        private void AddCategoryPage_Loaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        private void TxtName_OnGotFocus(object sender, RoutedEventArgs e) => Functions.TextBoxGotFocus(sender);

        private void TxtName_TextChanged(object sender, TextChangedEventArgs e) => BtnSubmit.IsEnabled =
            TxtName.Text.Length > 0;

        #endregion Page-Manipulation Methods
    }
}