using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finances
{
    /// <summary>Interaction logic for ManageCategoriesWindow.xaml</summary>
    public partial class ManageCategoriesWindow
    {
        private List<Category> _allCategories = AppState.AllCategories;
        private Category _selectedMajorCategory;
        private string _selectedMinorCategory;

        internal MainWindow RefToMainWindow { get; set; }

        /// <summary>Refreshes the Items Source of the LVMajor ListBox.</summary>
        internal void RefreshItemsSource()
        {
            _allCategories = AppState.AllCategories;
            LVMajor.UnselectAll();
            LVMajor.ItemsSource = _allCategories;
            LVMajor.Items.Refresh();
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
            AddCategoryWindow addCategoryWindow = new AddCategoryWindow { RefToManageCategoriesWindow = this };
            addCategoryWindow.LoadWindow(_selectedMajorCategory, isMajor);
            addCategoryWindow.Show();
            Visibility = Visibility.Hidden;
        }

        /// <summary>Displays the AddCategoryWindow</summary>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        private void ShowRenameCategoryWindow(bool isMajor)
        {
            RenameCategoryWindow categoryRenameWindow = new RenameCategoryWindow
            {
                RefToManageCategoriesWindow = this
            };
            categoryRenameWindow.LoadWindow(_selectedMajorCategory, _selectedMinorCategory, isMajor);
            categoryRenameWindow.Show();
            Visibility = Visibility.Hidden;
        }

        #endregion Window-Displaying Methods

        #region Button-Click Methods

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
            if (new Notification($"This will remove this category forever. Any existing transactions using this category will have their Major and Minor Category data removed. This will affect {AppState.AllTransactions.Count(transaction => transaction.MajorCategory == _selectedMajorCategory.Name)} transactions. Are you sure you want to delete it and all related minor categories?", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
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
            if (new Notification($"This will remove this category forever. Any existing transactions using this minor category will have their Minor Category data removed. This will affect {AppState.AllTransactions.Count(transaction => transaction.MajorCategory == _selectedMajorCategory.Name && transaction.MinorCategory == _selectedMinorCategory)} transactions. Are you sure you want to delete it?", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
                if (await AppState.RemoveMinorCategory(_selectedMajorCategory, _selectedMinorCategory))
                    RefreshItemsSource();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
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

        public ManageCategoriesWindow()
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

        private void WindowManageCategories_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}