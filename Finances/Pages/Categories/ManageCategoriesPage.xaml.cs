using Extensions;
using Extensions.ListViewHelp;
using Finances.Classes;
using Finances.Classes.Categories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances.Pages.Categories
{
    /// <summary>Interaction logic for ManageCategoriesWindow.xaml</summary>
    public partial class ManageCategoriesPage
    {
        private ListViewSort _lvMajor = new ListViewSort();
        private ListViewSort _lvMinor = new ListViewSort();
        private List<Category> _allCategories = AppState.AllCategories;
        private Category _selectedMajorCategory;
        private string _selectedMinorCategory;

        /// <summary>Refreshes the Items Source of the LVMajor ListBox.</summary>
        internal void RefreshItemsSource()
        {
            _allCategories = AppState.AllCategories;
            _selectedMajorCategory = new Category();
            LVMajor.UnselectAll();
            LVMajor.ItemsSource = _allCategories;
            LVMajor.Items.Refresh();
            LVMinor.DataContext = _selectedMajorCategory.MinorCategories;
        }

        #region Control-Toggle Methods

        /// <summary>Toggles the IsEnabled state of all the Window controls relating to major categories.</summary>
        /// <param name="toggle">Toggle control true or false</param>
        private void ToggleMajorEnabled(bool toggle)
        {
            BtnRenameMajor.IsEnabled = toggle;
            BtnRemoveMajor.IsEnabled = toggle;
        }

        /// <summary>Toggles the IsEnabled state of all the Window controls relating to minor categories.</summary>
        /// <param name="toggle">Toggle control true or false</param>
        private void ToggleMinorEnabled(bool toggle)
        {
            BtnAddMinor.IsEnabled = toggle;
            BtnRenameMinor.IsEnabled = toggle;
            BtnRemoveMinor.IsEnabled = toggle;
            LVMinor.IsEnabled = toggle;
        }

        #endregion Control-Toggle Methods

        #region Window-Displaying Methods

        /// <summary>Displays the AddCategoryWindow</summary>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        private void ShowAddCategoryWindow(bool isMajor)
        {
            AddCategoryPage addCategoryWindow = new AddCategoryPage { PreviousWindow = this };
            addCategoryWindow.LoadWindow(_selectedMajorCategory, isMajor);
            AppState.Navigate(addCategoryWindow);
        }

        /// <summary>Displays the AddCategoryWindow</summary>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        private void ShowRenameCategoryWindow(bool isMajor)
        {
            RenameCategoryPage categoryRenameWindow = new RenameCategoryPage { PreviousWindow = this };
            categoryRenameWindow.LoadWindow(_selectedMajorCategory, _selectedMinorCategory, isMajor);
            AppState.Navigate(categoryRenameWindow);
        }

        #endregion Window-Displaying Methods

        #region Click Methods

        private void BtnAddMajor_Click(object sender, RoutedEventArgs e)
        {
            ShowAddCategoryWindow(true);
        }

        private void BtnRenameMajor_Click(object sender, RoutedEventArgs e)
        {
            ShowRenameCategoryWindow(true);
        }

        private async void BtnRemoveMajor_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.YesNoNotification($"This will remove this category forever. Any existing transactions using this category will have their Major and Minor Category data removed. This will affect {AppState.AllTransactions.Count(transaction => transaction.MajorCategory == _selectedMajorCategory.Name)} transactions. Are you sure you want to delete it and all related minor categories?", "Finances"))
                if (await AppState.RemoveMajorCategory(_selectedMajorCategory))
                    RefreshItemsSource();
        }

        private void BtnAddMinor_Click(object sender, RoutedEventArgs e)
        {
            ShowAddCategoryWindow(false);
        }

        private void BtnRenameMinor_Click(object sender, RoutedEventArgs e)
        {
            ShowRenameCategoryWindow(false);
        }

        private async void BtnRemoveMinor_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.YesNoNotification($"This will remove this category forever. Any existing transactions using this minor category will have their Minor Category data removed. This will affect {AppState.AllTransactions.Count(transaction => transaction.MajorCategory == _selectedMajorCategory.Name && transaction.MinorCategory == _selectedMinorCategory)} transactions. Are you sure you want to delete it?", "Finances"))
                if (await AppState.RemoveMinorCategory(_selectedMajorCategory, _selectedMinorCategory))
                    RefreshItemsSource();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void LVMajorColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _lvMajor = Functions.ListViewColumnHeaderClick(sender, _lvMajor, LVMajor, "#BDC7C1");
        }

        private void LVMinorColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _lvMinor = Functions.ListViewColumnHeaderClick(sender, _lvMinor, LVMinor, "#BDC7C1");
        }

        #endregion Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            AppState.GoBack();
        }

        public ManageCategoriesPage()
        {
            InitializeComponent();
            LVMajor.ItemsSource = _allCategories;
        }

        private void LVMajor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LVMajor.SelectedIndex >= 0)
            {
                _selectedMajorCategory = (Category)LVMajor.SelectedValue;
                LVMinor.ItemsSource = _selectedMajorCategory.MinorCategories;
                ToggleMajorEnabled(true);
                BtnAddMinor.IsEnabled = true;
                LVMinor.IsEnabled = true;
            }
            else
            {
                _selectedMajorCategory = new Category();
                LVMinor.ItemsSource = new List<string>();
                ToggleMajorEnabled(false);
                ToggleMinorEnabled(false);
            }

            LVMinor.UnselectAll();
            LVMinor.Items.Refresh();
        }

        private void LVMinor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMinorCategory = LVMinor.SelectedIndex >= 0 ? LVMinor.SelectedItem.ToString() : "";
            ToggleMinorEnabled(LVMinor.SelectedIndex >= 0);
        }

        #endregion Window-Manipulation Methods

        private void ManageCategoriesPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
        }
    }
}