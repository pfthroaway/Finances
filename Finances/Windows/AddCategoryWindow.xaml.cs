using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            txtName.Focus();
        }

        #region Button-Click Methods

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_isMajor)
            {
                if (AppState.AllCategories.All(category => category.Name != txtName.Text))
                    if (await AppState.AddCategory(_majorCategory, txtName.Text, _isMajor))
                        CloseWindow();
            }
            else
            {
                if (!_majorCategory.MinorCategories.Contains(txtName.Text))
                    if (await AppState.AddCategory(_majorCategory, txtName.Text, _isMajor))
                        CloseWindow();
            }
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

        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        private void txtName_OnGotFocus(object sender, RoutedEventArgs e)
        {
            txtName.SelectAll();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSubmit.IsEnabled = txtName.Text.Length > 0;
        }

        private void windowAddCategory_Closing(object sender, CancelEventArgs e)
        {
            RefToManageCategoriesWindow.Show();
            RefToManageCategoriesWindow.RefreshItemsSource();
        }

        #endregion Window-Manipulation Methods
    }
}