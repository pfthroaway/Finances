using Finances.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace Finances
{
    /// <summary>
    /// Interaction logic for ManageCategoriesWindow.xaml
    /// </summary>
    public partial class ManageCategoriesWindow : Window
    {
        private List<Category> AllCatogories = AppState.AllCategories;
        private Category selectedCategory = new Category();
        private String lastSelected;

        internal MainWindow RefToMainWindow { get; set; }

        #region Text/Selection Changed
        private void lvMajor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCategory = (Category) lvMajor.SelectedValue;
            lastSelected = selectedCategory.Name;
            lvMinor.ItemsSource = selectedCategory.MinorCategories;
        }

        private void lvMinor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelected = lvMinor.SelectedItem.ToString();
        }
        #endregion

        #region Window Manipulation

        private void CloseWindow()
        {
            this.Close();
        }

        private void winManageCategories_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }
        #endregion

        #region Button-Click Handlers

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            CategoryRenamePopup categoryRenamePopup = new CategoryRenamePopup();
            categoryRenamePopup.RefToCategories = this;
            categoryRenamePopup.category = lastSelected;
            categoryRenamePopup.Show();
            this.Focusable = false;
        }

        #endregion

        #region Constructor
        public ManageCategoriesWindow()
        {
            InitializeComponent();
            lvMajor.ItemsSource = AllCatogories;
            lvMinor.ItemsSource = selectedCategory.MinorCategories;
        }

        #endregion       
    }
}
