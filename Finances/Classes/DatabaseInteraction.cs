using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finances
{
    internal class DatabaseInteraction
    {
        private readonly SQLiteDatabaseInteraction databaseInteractor = new SQLiteDatabaseInteraction();

        #region Load

        /// <summary>Loads all Accounts.</summary>
        /// <returns>Returns all Accounts</returns>
        internal async Task<List<Account>> LoadAccounts()
        {
            return await databaseInteractor.LoadAccounts();
        }

        /// <summary>Loads all Categories.</summary>
        /// <returns>Returns all Categories</returns>
        internal async Task<List<Category>> LoadCategories()
        {
            return await databaseInteractor.LoadCategories();
        }

        #endregion Load

        #region Transaction Management

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <param name="account">Account the transaction will be added to</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> AddTransaction(Transaction transaction, Account account)
        {
            return await databaseInteractor.AddTransaction(transaction, account);
        }

        /// <summary>Modifies the selected Transaction in the database.</summary>
        /// <param name="newTransaction">Transaction to replace the current one in the database</param>
        /// <param name="oldTransaction">Current Transaction in the database</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction)
        {
            return await databaseInteractor.ModifyTransaction(newTransaction, oldTransaction);
        }

        /// <summary>Deletes a transaction from the database.</summary>
        /// <param name="transaction">Transaction to be deleted</param>
        /// <param name="account">Account the transaction will be deleted from</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> DeleteTransaction(Transaction transaction, Account account)
        {
            return await databaseInteractor.DeleteTransaction(transaction, account);
        }

        #endregion Transaction Management

        #region Account Manipulation

        /// <summary>Adds an account to the database.</summary>
        /// <param name="account">Account to be added</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> AddAccount(Account account)
        {
            return await databaseInteractor.AddAccount(account);
        }

        /// <summary>Deletes an account from the database.</summary>
        /// <param name="account">Account to be deleted</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> DeleteAccount(Account account)
        {
            return await databaseInteractor.DeleteAccount(account);
        }

        /// <summary>Renames an account in the database.</summary>
        /// <param name="account">Account to be renamed</param>
        /// <param name="newAccountName">New account's name</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> RenameAccount(Account account, string newAccountName)
        {
            return await databaseInteractor.RenameAccount(account, newAccountName);
        }

        #endregion Account Manipulation

        #region Category Management

        /// <summary>Inserts a new Category into the database.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="newName">Name for new Category</param>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        /// <returns>Returns true if successful.</returns>
        internal async Task<bool> AddCategory(Category selectedCategory, string newName, bool isMajor)
        {
            return await databaseInteractor.AddCategory(selectedCategory, newName, isMajor);
        }

        /// <summary>Rename a category in the database.</summary>
        /// <param name="selectedCategory">Category to rename</param>
        /// <param name="newName">New name of the Category</param>
        /// <param name="oldName">Old name of the Category</param>
        /// <param name="isMajor">Is the category being renamed a Major Category?</param>
        /// <returns></returns>
        internal async Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor)
        {
            return await databaseInteractor.RenameCategory(selectedCategory, newName, oldName, isMajor);
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        internal async Task<bool> RemoveMajorCategory(Category selectedCategory)
        {
            return await databaseInteractor.RemoveMajorCategory(selectedCategory);
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        internal async Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory)
        {
            return await databaseInteractor.RemoveMinorCategory(selectedCategory, minorCategory);
        }

        #endregion Category Management
    }
}