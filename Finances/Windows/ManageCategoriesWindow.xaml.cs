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

        /// <summary>Refreshes the Items Source of the lvMajor ListBox.</summary>
        internal void RefreshItemsSource()
        {
            _allCategories = AppState.AllCategories;
            lvMajor.UnselectAll();
            lvMajor.ItemsSource = _allCategories;
            lvMajor.Items.Refresh();
        }

        #region Control-Toggle Methods

        /// <summary>Toggles the IsEnabled state of all the Window controls relating to major categories.</summary>
        /// <param name="toggle">Toggle control true or false</param>
        private void ToggleMajorEnabled(bool toggle)
        {
            btnRenameMajor.IsEnabled = toggle;
            btnRemoveMajor.IsEnabled = toggle;
        }

        /// <summary>Toggles the IsEnabled state of all the Window controls relating to minor categories.</summary>
        /// <param name="toggle">Toggle control true or false</param>
        private void ToggleMinorEnabled(bool toggle)
        {
            btnAddMinor.IsEnabled = toggle;
            btnRenameMinor.IsEnabled = toggle;
            btnRemoveMinor.IsEnabled = toggle;
            lvMinor.IsEnabled = toggle;
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
            this.Visibility = Visibility.Hidden;
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
            this.Visibility = Visibility.Hidden;
        }

        #endregion Window-Displaying Methods

        #region Button-Click Methods

        private void btnAddMajor_Click(object sender, RoutedEventArgs e)
        {
            ShowAddCategoryWindow(true);
        }

        private void btnRenameMajor_Click(object sender, RoutedEventArgs e)
        {
            ShowRenameCategoryWindow(true);
        }

        private async void btnRemoveMajor_Click(object sender, RoutedEventArgs e)
        {
            if (new Notification("This will remove this category forever. Any existing transactions using this category will have their Major and Minor Category data removed. This will affect " + AppState.AllTransactions.Count(transaction => transaction.MajorCategory == _selectedMajorCategory.Name) + " transactions. Are you sure you want to delete it and all related minor categories?", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
                if (await AppState.RemoveMajorCategory(_selectedMajorCategory))
                    RefreshItemsSource();
        }

        private void btnAddMinor_Click(object sender, RoutedEventArgs e)
        {
            ShowAddCategoryWindow(false);
        }

        private void btnRenameMinor_Click(object sender, RoutedEventArgs e)
        {
            ShowRenameCategoryWindow(false);
        }

        private async void btnRemoveMinor_Click(object sender, RoutedEventArgs e)
        {
            if (new Notification("This will remove this category forever. Any existing transactions using this minor category will have their Minor Category data removed. This will affect " + AppState.AllTransactions.Count(transaction => transaction.MajorCategory == _selectedMajorCategory.Name && transaction.MinorCategory == _selectedMinorCategory) + " transactions. Are you sure you want to delete it?", "Finances", NotificationButtons.YesNo, this).ShowDialog() == true)
                if (await AppState.RemoveMinorCategory(_selectedMajorCategory, _selectedMinorCategory))
                    RefreshItemsSource();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
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

        public ManageCategoriesWindow()
        {
            InitializeComponent();
            lvMajor.ItemsSource = _allCategories;
        }

        private void lvMajor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvMajor.SelectedIndex >= 0)
            {
                _selectedMajorCategory = (Category)lvMajor.SelectedValue;
                lvMinor.ItemsSource = _selectedMajorCategory.MinorCategories;
                ToggleMajorEnabled(true);
                btnAddMinor.IsEnabled = true;
                lvMinor.IsEnabled = true;
            }
            else
            {
                _selectedMajorCategory = new Category();
                lvMinor.ItemsSource = new List<string>();
                ToggleMajorEnabled(false);
                ToggleMinorEnabled(false);
            }

            lvMinor.UnselectAll();
            lvMinor.Items.Refresh();
        }

        private void lvMinor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMinorCategory = lvMinor.SelectedIndex >= 0 ? lvMinor.SelectedItem.ToString() : "";
            ToggleMinorEnabled(lvMinor.SelectedIndex >= 0);
        }

        private void windowManageCategories_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}