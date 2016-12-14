using System.ComponentModel;

namespace Finances
{
    internal class CategorizedExpense : INotifyPropertyChanged
    {
        private string _majorCategory, _minorCategory;
        private decimal _expenses, _income;

        /// <summary>Primary category</summary>
        public string MajorCategory
        {
            get { return _majorCategory; }
            set { _majorCategory = value; OnPropertyChanged("MajorCategory"); }
        }

        /// <summary>Secondary category</summary>
        public string MinorCategory
        {
            get { return _minorCategory; }
            set { _minorCategory = value; OnPropertyChanged("MinorCategory"); }
        }

        /// <summary>Income for this month</summary>
        public decimal Income
        {
            get { return _income; }
            set { _income = value; OnPropertyChanged("Income"); OnPropertyChanged("IncomeToString"); }
        }

        /// <summary>Income for this month.</summary>
        public string IncomeToString
        {
            get { return Income.ToString("C2"); }
        }

        /// <summary>Expenses for this month</summary>
        public decimal Expenses
        {
            get { return _expenses * -1; }
            set { _expenses = value; OnPropertyChanged("Expenses"); OnPropertyChanged("ExpensesToString"); }
        }

        /// <summary>Expenses for this month, formatted to currency</summary>
        public string ExpensesToString
        {
            get { return Expenses.ToString("C2"); }
        }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        /// <summary>Adds the values of a transaction to the month's total income/expenses.</summary>
        /// <param name="expenses">Expense value to be added</param>
        /// <param name="income">Income value to be added</param>
        internal void AddTransactionValues(decimal expenses, decimal income)
        {
            _expenses += expenses;
            Income += income;
        }

        public override string ToString()
        {
            return MajorCategory + " - " + MinorCategory;
        }

        /// <summary>Initializes a default instance of CategorizedExpense.</summary>
        public CategorizedExpense()
        {
        }

        /// <summary>Initializes an instance of CategorizedExpense by assigning Properties.</summary>
        /// <param name="majorCategory">Primary category</param>
        /// <param name="minorCategory">Secondary category</param>
        /// <param name="expenses">Expenses</param>
        /// <param name="income">Income</param>
        public CategorizedExpense(string majorCategory, string minorCategory, decimal expenses, decimal income)
        {
            MajorCategory = majorCategory;
            MinorCategory = minorCategory;
            Expenses = expenses;
            Income = income;
        }

        /// <summary>Replaces this instance of CategorizedExpense with another instance</summary>
        /// <param name="otherCategorizedExpense">Instance of CategorizedExpense to replace this instance</param>
        public CategorizedExpense(CategorizedExpense otherCategorizedExpense)
        {
            MajorCategory = otherCategorizedExpense.MajorCategory;
            MinorCategory = otherCategorizedExpense.MinorCategory;
            Expenses = otherCategorizedExpense.Expenses;
            Income = otherCategorizedExpense.Income;
        }
    }
}