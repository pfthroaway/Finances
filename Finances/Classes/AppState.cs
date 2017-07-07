using Extensions;
using Extensions.Enums;
using Finances.Classes.Categories;
using Finances.Classes.Data;
using Finances.Classes.Database;
using Finances.Classes.Sorting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Finances.Classes
{
    /// <summary>Represents the current state of the application.</summary>
    internal static class AppState
    {
        private static readonly SQLiteDatabaseInteraction DatabaseInteraction = new SQLiteDatabaseInteraction();

        internal static List<Account> AllAccounts = new List<Account>();
        internal static List<string> AllAccountTypes = new List<string>();
        internal static List<Category> AllCategories = new List<Category>();
        internal static List<Transaction> AllTransactions = new List<Transaction>();
        internal static List<Month> AllMonths = new List<Month>();
        internal static List<Year> AllYears = new List<Year>();

        /// <summary>Loads all information from the database.</summary>
        /// <returns>Returns true if successful</returns>
        internal static async Task LoadAll()
        {
            DatabaseInteraction.VerifyDatabaseIntegrity();
            AllAccounts = await DatabaseInteraction.LoadAccounts();
            AllAccountTypes = await DatabaseInteraction.LoadAccountTypes();
            AllCategories = await DatabaseInteraction.LoadCategories();

            foreach (Account account in AllAccounts)
                foreach (Transaction trans in account.AllTransactions)
                    AllTransactions.Add(trans);

            AllAccountTypes.Sort();
            AllTransactions = AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
            LoadMonths();
            LoadYears();
        }

        /// <summary>Loads all the Months from AllTransactions.</summary>
        private static void LoadMonths()
        {
            AllMonths.Clear();

            if (AllTransactions.Count > 0)
            {
                int months = ((DateTime.Now.Year - AllTransactions[AllTransactions.Count - 1].Date.Year) * 12) + DateTime.Now.Month - AllTransactions[AllTransactions.Count - 1].Date.Month;
                DateTime startMonth = new DateTime(AllTransactions[AllTransactions.Count - 1].Date.Year, AllTransactions[AllTransactions.Count - 1].Date.Month, 1);

                int start = 0;
                do
                {
                    AllMonths.Add(new Month(startMonth.AddMonths(start), new List<Transaction>()));
                    start += 1;
                }
                while (start <= months);

                foreach (Transaction transaction in AllTransactions)
                {
                    AllMonths.Find(month => month.MonthStart <= transaction.Date && transaction.Date <= month.MonthEnd.Date).AddTransaction(transaction);
                }

                AllMonths = AllMonths.OrderByDescending(month => month.FormattedMonth).ToList();
            }
        }

        /// <summary>Loads all the Months from AllTransactions.</summary>
        internal static void LoadYears()
        {
            AllYears.Clear();

            if (AllTransactions.Count > 0)
            {
                int years = ((DateTime.Now.Year - AllTransactions[AllTransactions.Count - 1].Date.Year));
                DateTime startYear = new DateTime(AllTransactions[AllTransactions.Count - 1].Date.Year, 1, 1);

                int start = 0;
                do
                {
                    AllYears.Add(new Year(startYear.AddYears(start), new List<Transaction>()));
                    start += 1;
                }
                while (start <= years);

                foreach (Transaction transaction in AllTransactions)
                {
                    AllYears.Find(year => year.YearStart <= transaction.Date && transaction.Date <= year.YearEnd.Date).AddTransaction(transaction);
                }

                AllYears = AllYears.OrderByDescending(year => year.FormattedYear).ToList();
            }
        }

        #region Transaction Manipulation

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <param name="account">Account the transaction will be added to</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> AddTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            if (await DatabaseInteraction.AddTransaction(transaction, account))
            {
                if (AllMonths.Any(month => month.MonthStart <= transaction.Date && transaction.Date <= month.MonthEnd.Date))
                    AllMonths.Find(month => month.MonthStart <= transaction.Date && transaction.Date <= month.MonthEnd.Date).AddTransaction(transaction);
                else
                {
                    Month newMonth = new Month(new DateTime(transaction.Date.Year, transaction.Date.Month, 1), new List<Transaction>());
                    newMonth.AddTransaction(transaction);
                    AllMonths.Add(newMonth);
                }

                AllMonths = AllMonths.OrderByDescending(month => month.FormattedMonth).ToList();
                success = true;
            }

            return success;
        }

        /// <summary>Modifies the selected Transaction in the database.</summary>
        /// <param name="newTransaction">Transaction to replace the current one in the database</param>
        /// <param name="oldTransaction">Current Transaction in the database</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction)
        {
            bool success = false;
            if (await DatabaseInteraction.ModifyTransaction(newTransaction, oldTransaction))
            {
                AllTransactions[AllTransactions.IndexOf(oldTransaction)] = newTransaction;
                AllTransactions = AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
                LoadMonths();
                LoadYears();
                success = true;
            }

            return success;
        }

        /// <summary>Deletes a transaction from the database.</summary>
        /// <param name="transaction">Transaction to be deleted</param>
        /// <param name="account">Account the transaction will be deleted from</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> DeleteTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            if (await DatabaseInteraction.DeleteTransaction(transaction, account))
            {
                AllTransactions.Remove(transaction);
                success = true;
            }

            return success;
        }

        #endregion Transaction Manipulation

        #region Account Manipulation

        /// <summary>Adds an account to the database.</summary>
        /// <param name="newAccount">Account to be added</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> AddAccount(Account newAccount)
        {
            bool success = false;
            if (await DatabaseInteraction.AddAccount(newAccount))
            {
                AllAccounts.Add(newAccount);
                AllAccounts = AllAccounts.OrderBy(account => account.Name).ToList();
                AllTransactions.Add(newAccount.AllTransactions[0]);
                AllTransactions = AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
                success = true;
            }

            return success;
        }

        /// <summary>Deletes an account from the database.</summary>
        /// <param name="account">Account to be deleted</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> DeleteAccount(Account account)
        {
            bool success = false;
            if (await DatabaseInteraction.DeleteAccount(account))
            {
                foreach (Transaction transaction in account.AllTransactions)
                    AllTransactions.Remove(transaction);

                AllAccounts.Remove(account);
                success = true;
            }

            return success;
        }

        /// <summary>Renames an account in the database.</summary>
        /// <param name="account">Account to be renamed</param>
        /// <param name="newAccountName">New account's name</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> RenameAccount(Account account, string newAccountName)
        {
            bool success = false;
            string oldAccountName = account.Name;
            if (await DatabaseInteraction.RenameAccount(account, newAccountName))
            {
                account.Name = newAccountName;

                foreach (Transaction transaction in AllTransactions)
                {
                    if (transaction.Account == oldAccountName)
                        transaction.Account = newAccountName;
                }
                success = true;
            }

            return success;
        }

        #endregion Account Manipulation

        #region Category Management

        /// <summary>Inserts a new Category into the database.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="newName">Name for new Category</param>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        /// <returns>Returns true if successful.</returns>
        internal static async Task<bool> AddCategory(Category selectedCategory, string newName, bool isMajor)
        {
            bool success = false;
            if (await DatabaseInteraction.AddCategory(selectedCategory, newName, isMajor))
            {
                if (isMajor)
                {
                    AllCategories.Add(new Category(
                        name: newName,
                        minorCategories: new List<MinorCategory>()));
                }
                else
                    selectedCategory.MinorCategories.Add(new MinorCategory(newName));

                AllCategories = AllCategories.OrderBy(category => category.Name).ToList();
                success = true;
            }

            return success;
        }

        /// <summary>Rename a category in the database.</summary>
        /// <param name="selectedCategory">Category to rename</param>
        /// <param name="newName">New name of the Category</param>
        /// <param name="oldName">Old name of the Category</param>
        /// <param name="isMajor">Is the category being renamed a Major Category?</param>
        /// <returns></returns>
        internal static async Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor)
        {
            bool success = false;
            if (await DatabaseInteraction.RenameCategory(selectedCategory, newName, oldName, isMajor))
            {
                if (isMajor)
                {
                    selectedCategory = AllCategories.Find(category => category.Name == selectedCategory.Name);
                    selectedCategory.Name = newName;
                    AllTransactions.Select(transaction => transaction.MajorCategory == oldName ? newName : oldName).ToList();
                }
                else
                {
                    selectedCategory = AllCategories.Find(category => category.Name == selectedCategory.Name);
                    MinorCategory minor = selectedCategory.MinorCategories.Find(category => category.Name == oldName);
                    minor.Name = newName;
                    AllTransactions.Select(transaction => transaction.MinorCategory == oldName ? newName : oldName).ToList();
                }

                AllCategories = AllCategories.OrderBy(category => category.Name).ToList();
                success = true;
            }

            return success;
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        internal static async Task<bool> RemoveMajorCategory(Category selectedCategory)
        {
            bool success = false;
            if (await DatabaseInteraction.RemoveMajorCategory(selectedCategory))
            {
                foreach (Transaction transaction in AllTransactions)
                {
                    if (transaction.MajorCategory == selectedCategory.Name)
                    {
                        transaction.MajorCategory = "";
                        transaction.MinorCategory = "";
                    }
                }

                AllCategories.Remove(AllCategories.Find(category => category.Name == selectedCategory.Name));
                success = true;
            }

            return success;
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        internal static async Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory)
        {
            bool success = false;
            if (await DatabaseInteraction.RemoveMinorCategory(selectedCategory, minorCategory))
            {
                foreach (Transaction transaction in AllTransactions)
                {
                    if (transaction.MajorCategory == selectedCategory.Name && transaction.MinorCategory == minorCategory)
                        transaction.MinorCategory = "";
                }

                selectedCategory = AllCategories.Find(category => category.Name == selectedCategory.Name);
                selectedCategory.MinorCategories.Remove(new MinorCategory(minorCategory));
                success = true;
            }

            return success;
        }

        #endregion Category Management

        #region Notification Management

        /// <summary>Displays a new Notification in a thread-safe way.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        internal static void DisplayNotification(string message, string title)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                new Notification(message, title, NotificationButtons.OK).ShowDialog();
            });
        }

        /// <summary>Displays a new Notification in a thread-safe way.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        /// <param name="window">Window being referenced</param>
        internal static void DisplayNotification(string message, string title, Window window)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                new Notification(message, title, NotificationButtons.OK, window).ShowDialog();
            });
        }

        /// <summary>Displays a new Notification in a thread-safe way and retrieves a boolean result upon its closing.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        /// <param name="window">Window being referenced</param>
        /// <returns>Returns value of clicked button on Notification.</returns>
        internal static bool YesNoNotification(string message, string title, Window window)
        {
            bool result = false;
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (new Notification(message, title, NotificationButtons.YesNo, window).ShowDialog() == true)
                    result = true;
            });
            return result;
        }

        #endregion Notification Management
    }
}