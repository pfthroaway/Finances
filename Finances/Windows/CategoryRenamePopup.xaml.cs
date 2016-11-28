using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Finances.Windows
{
    /// <summary>
    /// Interaction logic for CategoryRenamePopup.xaml
    /// </summary>
    public partial class CategoryRenamePopup : Window
    {
        internal String category { get; set; }
        internal ManageCategoriesWindow RefToCategories { get; set; }

        #region Text Changed
        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSubmit.IsEnabled = txtName.Text.Length > 0;
        }
        #endregion

        #region Constructor
        public CategoryRenamePopup()
        {
            InitializeComponent();
            txtName.Text = category;
        }
        #endregion

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (await AppState.RenameCategory(category, txtName.Text))
            {
                CloseWindow();
            }            
        }

        #region Window Manipulation
        private void CloseWindow()
        {
            this.Close();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RefToCategories.Focusable = true;
        }       
    }
}
