﻿using System.ComponentModel;

namespace Finances.Classes.Sorting
{
    /// <summary>Represents a collection of expenses sorted by category.</summary>
    internal class CategorizedExpense : INotifyPropertyChanged
    {
        private string _majorCategory, _minorCategory;
        private decimal _expenses, _income;

        #region Modifying Properties

        /// <summary>Primary category</summary>
        public string MajorCategory
        {
            get => _majorCategory;
            private set
            {
                _majorCategory = value;
                OnPropertyChanged("MajorCategory");
            }
        }

        /// <summary>Secondary category</summary>
        public string MinorCategory
        {
            get => _minorCategory;
            private set
            {
                _minorCategory = value;
                OnPropertyChanged("MinorCategory");
            }
        }

        /// <summary>Income for this month</summary>
        public decimal Income
        {
            get => _income;
            set
            {
                _income = value;
                OnPropertyChanged("Income");
                OnPropertyChanged("IncomeToString");
            }
        }

        /// <summary>Expenses for this month</summary>
        public decimal Expenses
        {
            get => _expenses * -1;
            set
            {
                _expenses = value;
                OnPropertyChanged("Expenses");
                OnPropertyChanged("ExpensesToString");
            }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Expenses for this month, formatted to currency</summary>
        public string ExpensesToString => Expenses.ToString("C2");

        /// <summary>Income for this month.</summary>
        public string IncomeToString => Income.ToString("C2");

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        /// <summary>Adds the values of a transaction to the month's total income/expenses.</summary>
        /// <param name="expenses">Expense value to be added</param>
        /// <param name="income">Income value to be added</param>
        internal void AddTransactionValues(decimal expenses, decimal income)
        {
            _expenses += expenses;
            Income += income;
        }

        public override string ToString() => $"{MajorCategory} - {MinorCategory}";

        #region Constructors

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
        /// <param name="other">Instance of CategorizedExpense to replace this instance</param>
        public CategorizedExpense(CategorizedExpense other) : this(other.MajorCategory, other.MinorCategory, other.Expenses, other.Income)
        {
        }

        #endregion Constructors
    }
}