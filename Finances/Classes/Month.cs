using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Finances
{
    /// <summary>Represents a month to help determine income and expenses of transactions.</summary>
    internal class Month : INotifyPropertyChanged
    {
        private DateTime _monthStart = new DateTime();
        private List<Transaction> _allTransactions = new List<Transaction>();

        #region Modifying Properties

        /// <summary>First day of the month</summary>
        public DateTime MonthStart
        {
            get { return _monthStart; }
            set { _monthStart = value; OnPropertyChanged("MonthStart"); }
        }

        /// <summary>Collection of all the transactions that occurred in the month</summary>
        internal ReadOnlyCollection<Transaction> AllTransactions
        {
            get { return new ReadOnlyCollection<Transaction>(_allTransactions); }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Income for this month</summary>
        public decimal Income
        {
            get
            {
                decimal income = 0.00M;
                for (int i = 0; i < AllTransactions.Count; i++)
                {
                    if (AllTransactions[i].MajorCategory != "Transfer")
                        income += AllTransactions[i].Inflow;
                }
                return income;
            }
        }

        /// <summary>Income for this month, formatted to currency</summary>
        public string IncomeToString
        {
            get { return Income.ToString("C2"); }
        }

        /// <summary>Income for this month, formatted to currency, with preceding text</summary>
        public string IncomeToStringWithText
        {
            get { return "Income: " + Income.ToString("C2"); }
        }

        /// <summary>Expenses for this month</summary>
        public decimal Expenses
        {
            get
            {
                decimal expenses = 0.00M;
                for (int i = 0; i < AllTransactions.Count; i++)
                {
                    if (AllTransactions[i].MajorCategory != "Transfer")
                        expenses += AllTransactions[i].Outflow;
                }
                return expenses * -1;
            }
        }

        /// <summary>Expenses for this month, formatted to currency</summary>
        public string ExpensesToString
        {
            get { return Expenses.ToString("C2"); }
        }

        /// <summary>Expenses for this month, formatted to currency, with preceding text</summary>
        public string ExpensesToStringWithText
        {
            get { return "Expenses: " + Expenses.ToString("C2"); }
        }

        /// <summary>Last day of the month</summary>
        public DateTime MonthEnd
        {
            get
            {
                return new DateTime(
                    year: MonthStart.Year,
                    month: MonthStart.Month,
                    day: DateTime.DaysInMonth(MonthStart.Year, MonthStart.Month));
            }
        }

        /// <summary>Formatted text representing the year and month</summary>
        public string FormattedMonth
        {
            get { return MonthStart.ToString("yyyy/MM"); }
        }

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Transaction Management

        /// <summary>Adds a transaction to this month.</summary>
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

        internal void Sort()
        {
            _allTransactions = _allTransactions.OrderByDescending(transaction => transaction.Date).ToList();
        }

        #region Override Operators

        public static bool Equals(Month left, Month right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return left.MonthStart == right.MonthStart && left.Income == right.Income && left.Expenses == right.Expenses;
        }

        public sealed override bool Equals(object obj)
        {
            return Equals(this, obj as Month);
        }

        public bool Equals(Month otherMonth)
        {
            return Equals(this, otherMonth);
        }

        public static bool operator ==(Month left, Month right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Month left, Month right)
        {
            return !Equals(left, right);
        }

        public sealed override int GetHashCode()
        {
            return base.GetHashCode() ^ 17;
        }

        public sealed override string ToString()
        {
            return FormattedMonth;
        }

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Month.</summary>
        public Month()
        {
        }

        /// <summary>Initializes an instance of Month by assigning Properties.</summary>
        /// <param name="monthStart">First day of the month</param>
        /// <param name="income">Income for this month</param>
        /// <param name="expenses">Expenses for this month</param>
        public Month(DateTime monthStart, IEnumerable<Transaction> transactions)
        {
            MonthStart = monthStart;
            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _allTransactions = newTransactions;
        }

        /// <summary>Replaces this instance of Account with another instance</summary>
        /// <param name="otherMonth">Month to replace this instance</param>
        public Month(Month otherMonth)
        {
            MonthStart = otherMonth.MonthStart;
            _allTransactions = new List<Transaction>(otherMonth.AllTransactions);
        }

        #endregion Constructors
    }
}