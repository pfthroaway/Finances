using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Finances
{
    /// <summary>Interaction logic for CategoryRenamePopup.xaml</summary>
    public partial class RenameCategoryWindow
    {
        private Category _selectedCategory;
        private string _minorCategory;
        private bool _isMajor;

        internal ManageCategoriesWindow RefToManageCategoriesWindow { get; set; }

        /// <summary>Loads the necessary components for the Window.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category</param>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        internal void LoadWindow(Category selectedCategory, string minorCategory, bool isMajor)
        {
            _selectedCategory = selectedCategory;
            _minorCategory = minorCategory;
            _isMajor = isMajor;

            txtName.Text = _isMajor ? _selectedCategory.Name : _minorCategory;
            txtName.Focus();
        }

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_isMajor)
            {
                if (await AppState.RenameCategory(_selectedCategory, txtName.Text, _selectedCategory.Name, _isMajor))
                    CloseWindow();
            }
            else
            {
                if (await AppState.RenameCategory(_selectedCategory, txtName.Text, _minorCategory, _isMajor))
                    CloseWindow();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Window Manipulation

        private void CloseWindow()
        {
            this.Close();
        }

        public RenameCategoryWindow()
        {
            InitializeComponent();
        }

        private void txtName_GotFocus(object sender, RoutedEventArgs e)
        {
            txtName.SelectAll();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSubmit.IsEnabled = txtName.Text.Length > 0;
        }

        private void windowRenameCategory_Closing(object sender, CancelEventArgs e)
        {
            RefToManageCategoriesWindow.Show();
            RefToManageCategoriesWindow.RefreshItemsSource();
        }

        #endregion Window Manipulation
    }
}