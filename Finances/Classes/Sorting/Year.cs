using Finances.Classes.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Finances.Classes.Sorting
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

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Collection of all the transactions that occurred in the year</summary>
        internal ReadOnlyCollection<Transaction> AllTransactions => new ReadOnlyCollection<Transaction>(_allTransactions);

        /// <summary>Income for this year</summary>
        public decimal Income => AllTransactions.Where(transaction => transaction.MajorCategory != "Transfer").Sum(transaction => transaction.Inflow);

        /// <summary>Income for this year, formatted to currency</summary>
        public string IncomeToString => Income.ToString("C2");

        /// <summary>Income for this year, formatted to currency, with preceding text</summary>
        public string IncomeToStringWithText => $"Income: {Income:C2}";

        /// <summary>Expenses for this year</summary>
        public decimal Expenses => AllTransactions.Where(transaction => transaction.MajorCategory != "Transfer").Sum(transaction => transaction.Outflow);

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

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

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
        internal void ModifyTransaction(int index, Transaction transaction) => _allTransactions[index] = transaction;

        /// <summary>Removes a transaction from this account.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void RemoveTransaction(Transaction transaction)
        {
            _allTransactions.Remove(transaction);
            OnPropertyChanged("BalanceToStringWithText");
        }

        #endregion Transaction Management

        /// <summary>Sorts the List by date, newest to oldest.</summary>
        private void Sort() => _allTransactions = _allTransactions.OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.ID).ToList();

        #region Override Operators

        private static bool Equals(Year left, Year right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return left.YearStart == right.YearStart && left.Income == right.Income && left.Expenses == right.Expenses;
        }

        public sealed override bool Equals(object obj) => Equals(this, obj as Year);

        public bool Equals(Year otherYear) => Equals(this, otherYear);

        public static bool operator ==(Year left, Year right) => Equals(left, right);

        public static bool operator !=(Year left, Year right) => !Equals(left, right);

        public sealed override int GetHashCode() => base.GetHashCode() ^ 17;

        public sealed override string ToString() => FormattedYear;

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
        /// <param name="other">Year to replace this instance</param>
        public Year(Year other) : this(other.YearStart, other.AllTransactions)
        {
        }

        #endregion Constructors
    }
}