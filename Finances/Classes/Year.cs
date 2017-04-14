using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Finances
{
    /// <summary>Represents a year to help determine income and expenses of transactions.</summary>
    internal class Year : INotifyPropertyChanged
    {
        private DateTime _yearStart;
        private List<Transaction> _allTransactions = new List<Transaction>();

        #region Modifying Properties

        /// <summary>First day of the year</summary>
        public DateTime YearStart
        {
            get => _yearStart;
            private set { _yearStart = value; OnPropertyChanged("YearStart"); }
        }

        /// <summary>Collection of all the transactions that occurred in the year</summary>
        internal ReadOnlyCollection<Transaction> AllTransactions => new ReadOnlyCollection<Transaction>(_allTransactions);

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Income for this year</summary>
        public decimal Income
        {
            get
            {
                decimal income = 0.00M;
                foreach (Transaction transaction in AllTransactions)
                {
                    if (transaction.MajorCategory != "Transfer")
                        income += transaction.Inflow;
                }
                return income;
            }
        }

        /// <summary>Income for this year, formatted to currency</summary>
        public string IncomeToString => Income.ToString("C2");

        /// <summary>Income for this year, formatted to currency, with preceding text</summary>
        public string IncomeToStringWithText => $"Income: {Income:C2}";

        /// <summary>Expenses for this year</summary>
        public decimal Expenses
        {
            get
            {
                decimal expenses = 0.00M;
                foreach (Transaction transaction in AllTransactions)
                {
                    if (transaction.MajorCategory != "Transfer")
                        expenses += transaction.Outflow;
                }
                return expenses;
            }
        }

        /// <summary>Expenses for this year, formatted to currency</summary>
        public string ExpensesToString => Expenses.ToString("C2");

        /// <summary>Expenses for this year, formatted to currency, with preceding text</summary>
        public string ExpensesToStringWithText => $"Expenses: {Expenses:C2}";

        /// <summary>Last day of the year</summary>
        public DateTime YearEnd => new DateTime(YearStart.Year, 12, 31);

        /// <summary>Formatted text representing the year and year</summary>
        public string FormattedYear => YearStart.ToString("yyyy/MM");

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Transaction Management

        /// <summary>Adds a transaction to this year.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void AddTransaction(Transaction transaction)
        {
            _allTransactions.Add(transaction);
            Sort();
            OnPropertyChanged("BalanceToStringWithText");
        }

        /// <summary>Modifies a transaction in this account.</summary>
        /// <param name="index">Index of transaction to be modified</param>
        /// <param name="transaction">Transaction to replace current in list</param>
        internal void ModifyTransaction(int index, Transaction transaction)
        {
            _allTransactions[index] = transaction;
        }

        /// <summary>Removes a transaction to this account.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void RemoveTransaction(Transaction transaction)
        {
            _allTransactions.Remove(transaction);
            OnPropertyChanged("BalanceToStringWithText");
        }

        #endregion Transaction Management

        private void Sort()
        {
            _allTransactions = _allTransactions.OrderByDescending(transaction => transaction.Date).ToList();
        }

        #region Override Operators

        private static bool Equals(Year left, Year right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return left.YearStart == right.YearStart && left.Income == right.Income && left.Expenses == right.Expenses;
        }

        public sealed override bool Equals(object obj)
        {
            return Equals(this, obj as Year);
        }

        public bool Equals(Year otherYear)
        {
            return Equals(this, otherYear);
        }

        public static bool operator ==(Year left, Year right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Year left, Year right)
        {
            return !Equals(left, right);
        }

        public sealed override int GetHashCode()
        {
            return base.GetHashCode() ^ 17;
        }

        public sealed override string ToString()
        {
            return FormattedYear;
        }

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Year.</summary>
        public Year()
        {
        }

        /// <summary>Initializes an instance of Year by assigning Properties.</summary>
        /// <param name="yearStart">First day of the year</param>
        /// <param name="transactions">Transactions during this year</param>
        public Year(DateTime yearStart, IEnumerable<Transaction> transactions)
        {
            YearStart = yearStart;
            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _allTransactions = newTransactions;
        }

        /// <summary>Replaces this instance of Account with another instance</summary>
        /// <param name="otherYear">Year to replace this instance</param>
        public Year(Year otherYear)
        {
            YearStart = otherYear.YearStart;
            _allTransactions = new List<Transaction>(otherYear.AllTransactions);
        }

        #endregion Constructors
    }
}