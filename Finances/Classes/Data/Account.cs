﻿using Finances.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Finances.Classes.Data
{
    /// <summary>Represents an account where money is credited/debited.</summary>
    public class Account : INotifyPropertyChanged
    {
        private string _name;
        private AccountTypes _accountType;
        private List<Transaction> _allTransactions = new List<Transaction>();

        #region Modifying Properties

        /// <summary>Name of the account</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged("Name"); }
        }

        /// <summary>Type of the account</summary>
        public AccountTypes AccountType
        {
            get => _accountType;
            set { _accountType = value; OnPropertyChanged("AccountType"); }
        }

        /// <summary>Type of the account, formatted</summary>
        public string Type
        {
            get
            {
                switch (AccountType)
                {
                    case AccountTypes.Cash:
                        return "Cash";

                    case AccountTypes.Checking:
                        return "Checking";

                    case AccountTypes.CreditCard:
                        return "Credit Card";

                    case AccountTypes.Merchant:
                        return "Merchant";

                    case AccountTypes.Prepaid:
                        return "Prepaid";

                    case AccountTypes.Savings:
                        return "Savings";

                    default:
                        return "Invalid Account Type";
                }
            }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Collection of all the transactions in the account</summary>
        public ReadOnlyCollection<Transaction> AllTransactions => new ReadOnlyCollection<Transaction>(_allTransactions);

        /// <summary>Balance of the account</summary>
        public decimal Balance => AllTransactions.Sum(transaction => (-1 * transaction.Outflow) + transaction.Inflow);

        /// <summary>Balance of the account, formatted to currency</summary>
        public string BalanceToString => Balance.ToString("C2");

        /// <summary>Balance of the account, formatted to currency, with preceding text</summary>
        public string BalanceToStringWithText => $"Balance: {BalanceToString}";

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

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
            if (transaction.Account == Name)
                _allTransactions[index] = transaction;
            else
                RemoveTransaction(index);
            Sort();
            UpdateTransactions();
        }

        /// <summary>Removes a transaction from this account.</summary>
        /// <param name="transaction">Transaction to be added</param>
        internal void RemoveTransaction(Transaction transaction)
        {
            _allTransactions.Remove(transaction);
            UpdateTransactions();
        }

        /// <summary>Removes a transaction from this account at a specific index.</summary>
        /// <param name="index">Location in the List to remove the transaction</param>
        internal void RemoveTransaction(int index)
        {
            _allTransactions.RemoveAt(index);
            UpdateTransactions();
        }

        /// <summary>Keeps the Transactions List updated when a Transactions is added/removed/modified.</summary>
        private void UpdateTransactions()
        {
            OnPropertyChanged("Transactions");
            OnPropertyChanged("BalanceToStringWithText");
        }

        #endregion Transaction Management

        /// <summary>Sorts all Transactions in the Account by date.</summary>
        internal void Sort() => _allTransactions = _allTransactions.OrderByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.ID).ToList();

        #region Override Operators

        private static bool Equals(Account left, Account right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) && left.AccountType == right.AccountType && left.Balance == right.Balance && (left.AllTransactions.Count == right.AllTransactions.Count && !left.AllTransactions.Except(right.AllTransactions).Any());
        }

        public sealed override bool Equals(object obj) => Equals(this, obj as Account);

        public bool Equals(Account otherAccount) => Equals(this, otherAccount);

        public static bool operator ==(Account left, Account right) => Equals(left, right);

        public static bool operator !=(Account left, Account right) => !Equals(left, right);

        public sealed override int GetHashCode() => base.GetHashCode() ^ 17;

        public sealed override string ToString() => Name;

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Account.</summary>
        public Account()
        {
        }

        /// <summary>Initializes an instance of Account by assigning Properties.</summary>
        /// <param name="name">Name of the account</param>
        /// <param name="accountType">Type of Account</param>
        /// <param name="transactions">Collection of all the transactions in the account</param>
        public Account(string name, AccountTypes accountType, IEnumerable<Transaction> transactions)
        {
            Name = name;
            AccountType = accountType;
            List<Transaction> newTransactions = new List<Transaction>();
            newTransactions.AddRange(transactions);
            _allTransactions = newTransactions;
        }

        /// <summary>Replaces this instance of Account with another instance</summary>
        /// <param name="other">Account to replace this instance</param>
        public Account(Account other) : this(other.Name, other.AccountType, other.AllTransactions)
        {
        }

        #endregion Constructors
    }
}