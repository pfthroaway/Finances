using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Finances
{
    internal class Account : INotifyPropertyChanged
    {
        private string _name;
        private List<Transaction> _allTransactions = new List<Transaction>();

        #region Modifying Properties

        /// <summary>Name of the account</summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        /// <summary>Balance of the account</summary>
        public decimal Balance
        {
            get
            {
                return AllTransactions.Sum(transaction => (-1 * transaction.Outflow) + transaction.Inflow);
            }
        }

        /// <summary>Collection of all the transactions in the account</summary>
        public ReadOnlyCollection<Transaction> AllTransactions => new ReadOnlyCollection<Transaction>(_allTransactions);

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Balance of the account, formatted to currency</summary>
        public string BalanceToString => Balance.ToString("C2");

        /// <summary>Balance of the account, formatted to currency, with preceding text</summary>
        public string BalanceToStringWithText => "Balance: " + BalanceToString;

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Transaction Management

        /// <summary>Adds a transaction to this account.</summary>
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

        private static bool Equals(Account left, Account right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) && left.Balance == right.Balance && (left.AllTransactions.Count == right.AllTransactions.Count && !left.AllTransactions.Except(right.AllTransactions).Any());
        }

        public sealed override bool Equals(object obj)
        {
            return Equals(this, obj as Account);
        }

        public bool Equals(Account otherAccount)
        {
            return Equals(this, otherAccount);
        }

        public static bool operator ==(Account left, Account right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Account left, Account right)
        {
            return !Equals(left, right);
        }

        public sealed override int GetHashCode()
        {
            return base.GetHashCode() ^ 17;
        }

        public sealed override string ToString()
        {
            return Name;
        }

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Account.</summary>
        public Account()
        {
        }

        /// <summary>Initializes an instance of Account by assigning Properties.</summary>
        /// <param name="name">Name of the account</param>
        /// <param name="transactions">Collection of all the transactions in the account</param>
        public Account(string name, IEnumerable<Transaction> transactions)
        {
            Name = name;
            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _allTransactions = newTransactions;
        }

        /// <summary>Replaces this instance of Account with another instance</summary>
        /// <param name="otherAccount">Account to replace this instance</param>
        public Account(Account otherAccount)
        {
            Name = otherAccount.Name;
            _allTransactions = new List<Transaction>(otherAccount.AllTransactions);
        }

        #endregion Constructors
    }
}