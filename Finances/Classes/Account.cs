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
        private decimal _balance;
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
            get { return CalculateBalance(); }
        }

        /// <summary>Collection of all the transactions in the account</summary>
        public ReadOnlyCollection<Transaction> AllTransactions
        {
            get { return new ReadOnlyCollection<Transaction>(_allTransactions); }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Balance of the account, formatted to currency</summary>
        public string BalanceToString
        {
            get { return Balance.ToString("C2"); }
        }

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Account Management

        /// <summary>Calculates the balance based on all transactions in this account.</summary>
        private decimal CalculateBalance()
        {
            decimal balance = 0.00M;
            for (int i = 0; i < AllTransactions.Count; i++)
                balance += (-1 * AllTransactions[i].Outflow) + AllTransactions[i].Inflow;
            return balance;
        }

        /// <summary>Adds a transaction to this account.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void AddTransaction(Transaction transaction)
        {
            _allTransactions.Add(transaction);
            OnPropertyChanged("BalanceToString");
        }

        /// <summary>Removes a transaction to this account.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void RemoveTransaction(Transaction transaction)
        {
            _allTransactions.Remove(transaction);
        }

        #endregion Account Management

        #region Override Operators

        public static bool Equals(Account left, Account right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) && left.Balance == right.Balance && ((left.AllTransactions.Count() == right.AllTransactions.Count()) && !left.AllTransactions.Except(right.AllTransactions).Any());
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
        /// <param name="balance">Balance of the account</param>
        /// <param name="transactions">Collection of all the transactions in the account</param>
        public Account(string name, decimal balance, IEnumerable<Transaction> transactions)
        {
            Name = name;
            _balance = balance;
            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _allTransactions = newTransactions;
        }

        /// <summary>Replaces this instance of Account with another instance</summary>
        /// <param name="otherAccount">Account to replace this instance</param>
        public Account(Account otherAccount)
        {
            Name = otherAccount.Name;
            _balance = otherAccount.Balance;
            _allTransactions = new List<Transaction>(otherAccount.AllTransactions);
        }

        #endregion Constructors
    }
}