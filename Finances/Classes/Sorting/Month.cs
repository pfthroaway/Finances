using Finances.Classes.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Finances.Classes.Sorting
{
    /// <summary>Represents a month to help determine income and expenses of transactions.</summary>
    internal class Month : INotifyPropertyChanged
    {
        private DateTime _monthStart;
        private List<Transaction> _allTransactions = new List<Transaction>();

        #region Modifying Properties

        /// <summary>First day of the month</summary>
        public DateTime MonthStart
        {
            get => _monthStart;
            private set
            {
                _monthStart = value;
                OnPropertyChanged("MonthStart");
            }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Collection of all the transactions that occurred in the month</summary>
        internal ReadOnlyCollection<Transaction> AllTransactions => new ReadOnlyCollection<Transaction>(
            _allTransactions);

        /// <summary>Income for  month</summary>
        public decimal Income => AllTransactions.Where(transaction => transaction.MajorCategory != "Transfer").Sum(transaction => transaction.Inflow);

        /// <summary>Income for  month, formatted to currency</summary>
        public string IncomeToString => Income.ToString("C2");

        /// <summary>Income for  month, formatted to currency, with preceding text</summary>
        public string IncomeToStringWithText => $"Income: {Income:C2}";

        /// <summary>Expenses for  month</summary>
        public decimal Expenses => AllTransactions.Where(transaction => transaction.MajorCategory != "Transfer").Sum(transaction => transaction.Outflow);

        /// <summary>Expenses for  month, formatted to currency</summary>
        public string ExpensesToString => Expenses.ToString("C2");

        /// <summary>Expenses for  month, formatted to currency, with preceding text</summary>
        public string ExpensesToStringWithText => $"Expenses: {Expenses:C2}";

        /// <summary>Last day of the month</summary>
        public DateTime MonthEnd => new DateTime(MonthStart.Year, MonthStart.Month,
            DateTime.DaysInMonth(MonthStart.Year, MonthStart.Month));

        /// <summary>Formatted text representing the year and month</summary>
        public string FormattedMonth => MonthStart.ToString("yyyy/MM");

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Transaction Management

        /// <summary>Adds a transaction to  month.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void AddTransaction(Transaction transaction)
        {
            _allTransactions.Add(transaction);
            Sort();
            OnPropertyChanged("BalanceToStringWithText");
        }

        /// <summary>Modifies a transaction in  account.</summary>
        /// <param name="index">Index of transaction to be modified</param>
        /// <param name="transaction">Transaction to replace current in list</param>
        internal void ModifyTransaction(int index, Transaction transaction) => _allTransactions[index] = transaction;

        /// <summary>Removes a transaction to  account.</summary>
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

        private static bool Equals(Month left, Month right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return left.MonthStart == right.MonthStart && left.Income == right.Income && left.Expenses == right.Expenses;
        }

        public sealed override bool Equals(object obj) => Equals(this, obj as Month);

        public bool Equals(Month otherMonth) => Equals(this, otherMonth);

        public static bool operator ==(Month left, Month right) => Equals(left, right);

        public static bool operator !=(Month left, Month right) => !Equals(left, right);

        public sealed override int GetHashCode() => base.GetHashCode() ^ 17;

        public sealed override string ToString() => FormattedMonth;

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Month.</summary>
        public Month()
        {
        }

        /// <summary>Initializes an instance of Month by assigning Properties.</summary>
        /// <param name="monthStart">First day of the month</param>
        /// <param name="transactions">Transactions during  month</param>
        public Month(DateTime monthStart, IEnumerable<Transaction> transactions)
        {
            MonthStart = monthStart;
            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _allTransactions = newTransactions;
        }

        /// <summary>Replaces  instance of Account with another instance</summary>
        /// <param name="other">Month to replace  instance</param>
        public Month(Month other) : this(other.MonthStart, other.AllTransactions)
        {
        }

        #endregion Constructors
    }
}