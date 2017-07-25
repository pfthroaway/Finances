using Extensions;
using Finances.Classes;
using Finances.Classes.Categories;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages.Categories
{
    /// <summary>Interaction logic for CategoryRenamePopup.xaml</summary>
    public partial class RenameCategoryPage
    {
        private Category _selectedCategory;
        private string _minorCategory;
        private bool _isMajor;

        internal ManageCategoriesPage PreviousWindow { get; set; }

        /// <summary>Loads the necessary components for the Window.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category</param>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        internal void LoadWindow(Category selectedCategory, string minorCategory, bool isMajor)
        {
            _selectedCategory = selectedCategory;
            _minorCategory = minorCategory;
            _isMajor = isMajor;

            TxtName.Text = _isMajor ? _selectedCategory.Name : _minorCategory;
            TxtName.Focus();
        }

        #region Button-Click Methods

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_isMajor)
            {
                if (await AppState.RenameCategory(_selectedCategory, TxtName.Text, _selectedCategory.Name, _isMajor))
                    CloseWindow();
            }
            else
            {
                if (await AppState.RenameCategory(_selectedCategory, TxtName.Text, _minorCategory, _isMajor))
                    CloseWindow();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Window Manipulation

        private void CloseWindow()
        {
            PreviousWindow.RefreshItemsSource();
            AppState.GoBack();
        }

        public RenameCategoryPage()
        {
            InitializeComponent();
        }

        private void TxtName_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSubmit.IsEnabled = TxtName.Text.Length > 0;
        }

        #endregion Window Manipulation

        private void RenameCategoryPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
        }
    }
}