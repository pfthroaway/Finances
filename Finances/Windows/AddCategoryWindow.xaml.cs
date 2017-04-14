using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Extensions;

namespace Finances
{
    /// <summary>Interaction logic for AddCategoryWindow.xaml</summary>
    public partial class AddCategoryWindow
    {
        private Category _majorCategory;
        private bool _isMajor;
        internal ManageCategoriesWindow RefToManageCategoriesWindow { get; set; }

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
                        CloseWindow();
            }
            else
            {
                if (!_majorCategory.MinorCategories.Contains(TxtName.Text))
                    if (await AppState.AddCategory(_majorCategory, TxtName.Text, _isMajor))
                        CloseWindow();
            }
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

        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        private void TxtName_OnGotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSubmit.IsEnabled = TxtName.Text.Length > 0;
        }

        private void WindowAddCategory_Closing(object sender, CancelEventArgs e)
        {
            RefToManageCategoriesWindow.Show();
            RefToManageCategoriesWindow.RefreshItemsSource();
        }

        #endregion Window-Manipulation Methods
    }
}