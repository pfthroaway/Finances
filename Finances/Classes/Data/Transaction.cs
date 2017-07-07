﻿using System;
using System.ComponentModel;

namespace Finances.Classes.Data
{
    /// <summary>Represents a monetary transaction in an account.</summary>
    public class Transaction : INotifyPropertyChanged
    {
        private DateTime _date;
        private string _payee, _majorCategory, _minorCategory, _memo, _account;
        private decimal _outflow, _inflow;

        #region Modifying Properties

        /// <summary>Date the transaction occurred</summary>
        public DateTime Date
        {
            get => _date;
            private set { _date = value; OnPropertyChanged("Date"); }
        }

        /// <summary>The entity the transaction revolves around</summary>
        public string Payee
        {
            get => _payee;
            private set { _payee = value; OnPropertyChanged("Payee"); }
        }

        /// <summary>Primary category of which the transaction regards</summary>
        public string MajorCategory
        {
            get => _majorCategory;
            set { _majorCategory = value; OnPropertyChanged("MajorCategory"); }
        }

        /// <summary>Secondary category of which the transaction regards</summary>
        public string MinorCategory
        {
            get => _minorCategory;
            set { _minorCategory = value; OnPropertyChanged("MinorCategory"); }
        }

        /// <summary>Extra information regarding the transaction</summary>
        public string Memo
        {
            get => _memo;
            private set { _memo = value; OnPropertyChanged("Memo"); }
        }

        /// <summary>How much money left the account during this transaction</summary>
        public decimal Outflow
        {
            get => _outflow;
            private set { _outflow = value; OnPropertyChanged("Outflow"); OnPropertyChanged("OutflowToString"); }
        }

        /// <summary>How much money entered the account during this transaction</summary>
        public decimal Inflow
        {
            get => _inflow;
            private set { _inflow = value; OnPropertyChanged("Inflow"); OnPropertyChanged("InflowToString"); }
        }

        /// <summary>Name of the account this transaction is associated with</summary>
        public string Account
        {
            get => _account;
            set { _account = value; OnPropertyChanged("Account"); }
        }

        #endregion Modifying Properties

        #region Helper Properties

        /// <summary>Date the transaction occurred, formatted properly</summary>
        public string DateToString => Date.ToString("yyyy/MM/dd");

        /// <summary>How much money left the account during this transaction, formatted to currency</summary>
        public string OutflowToString => Outflow.ToString("C2");

        /// <summary>How much money entered the account during this transaction, formatted to currency</summary>
        public string InflowToString => Inflow.ToString("C2");

        #endregion Helper Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Override Operators

        private static bool Equals(Transaction left, Transaction right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return DateTime.Equals(left.Date, right.Date) && string.Equals(left.Payee, right.Payee, StringComparison.OrdinalIgnoreCase) && string.Equals(left.MajorCategory, right.MajorCategory, StringComparison.OrdinalIgnoreCase) && string.Equals(left.MinorCategory, right.MinorCategory, StringComparison.OrdinalIgnoreCase) && string.Equals(left.Memo, right.Memo, StringComparison.OrdinalIgnoreCase) && left.Outflow == right.Outflow && left.Inflow == right.Inflow && string.Equals(left.Account, right.Account, StringComparison.OrdinalIgnoreCase);
        }

        public sealed override bool Equals(object obj)
        {
            return Equals(this, obj as Transaction);
        }

        public bool Equals(Transaction otherTransaction)
        {
            return Equals(this, otherTransaction);
        }

        public static bool operator ==(Transaction left, Transaction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Transaction left, Transaction right)
        {
            return !Equals(left, right);
        }

        public sealed override int GetHashCode()
        {
            return base.GetHashCode() ^ 17;
        }

        public sealed override string ToString()
        {
            return string.Join(" - ", DateToString, Account, Payee);
        }

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of Transaction.</summary>
        public Transaction()
        {
        }

        /// <summary>Initializes an instance of Transaction by assigning Properties.</summary>
        /// <param name="date">Date the transaction occurred</param>
        /// <param name="payee">The entity the transaction revolves around</param>
        /// <param name="majorCategory">Primary category of which the transaction regards</param>
        /// <param name="minorCategory">Secondary category of which the transaction regards</param>
        /// <param name="memo">Extra information regarding the transaction</param>
        /// <param name="outflow">How much money left the account during this transaction</param>
        /// <param name="inflow">How much money entered the account during this transaction</param>
        /// <param name="account">Account name related to the transaction</param>
        public Transaction(DateTime date, string payee, string majorCategory, string minorCategory, string memo, decimal outflow, decimal inflow, string account)
        {
            Date = date;
            Payee = payee;
            MajorCategory = majorCategory;
            MinorCategory = minorCategory;
            Memo = memo;
            Outflow = outflow;
            Inflow = inflow;
            Account = account;
        }

        /// <summary>Replaces this instance of Transaction with another instance</summary>
        /// <param name="other">Transaction to replace this instance</param>
        public Transaction(Transaction other) : this(other.Date, other.Payee, other.MajorCategory, other.MinorCategory, other.Memo, other.Outflow, other.Inflow, other.Account)
        {
        }

        #endregion Constructors
    }
}