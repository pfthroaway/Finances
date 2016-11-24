using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Finances
{
    /// <summary>
    /// Interaction logic for NewTransactionWindow.xaml
    /// </summary>
    public partial class NewTransactionWindow : Window, INotifyPropertyChanged
    {
        internal MainWindow RefToMainWindow { get; set; }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click Methods

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            cmbMajorCategory.SelectedIndex = -1;
            cmbMinorCategory.SelectedIndex = -1;
            txtMemo.Text = "";
            txtPayee.Text = "";
            txtInflow.Text = "";
            txtOutflow.Text = "";
            cmbAccount.SelectedIndex = -1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text/Selection Changed

        private void TextChanged()
        {
            if (datePicker.SelectedDate != null && cmbMajorCategory.SelectedIndex >= 0 && cmbMinorCategory.SelectedIndex >= 0 && txtPayee.Text.Length > 0 && (txtInflow.Text.Length > 0 | txtOutflow.Text.Length > 0) && cmbAccount.SelectedIndex >= 0)
            { }
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        #endregion Text/Selection Changed

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            this.Close();
        }

        public NewTransactionWindow()
        {
            InitializeComponent();
        }

        private void windowNewTransaction_Closing(object sender, CancelEventArgs e)
        {
            RefToMainWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}