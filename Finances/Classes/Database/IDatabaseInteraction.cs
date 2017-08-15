using Finances.Classes.Categories;
using Finances.Classes.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finances.Classes.Database
{
    /// <summary>Represents required interactions for implementations of databases.</summary>
    internal interface IDatabaseInteraction
    {
        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        /// <returns>Returns true once the database has been validated</returns>
        void VerifyDatabaseIntegrity();

        #region Load

        /// <summary>Loads all Accounts.</summary>
        /// <returns>Returns all Accounts</returns>
        Task<List<Account>> LoadAccounts();

        /// <summary>Loads all Account types.</summary>
        /// <returns>Returns all Account types</returns>
        Task<List<string>> LoadAccountTypes();

        /// <summary>Loads all Categories.</summary>
        /// <returns>Returns all Categories</returns>
        Task<List<Category>> LoadCategories();

        /// <summary>Loads all credit scores from the database.</summary>
        /// <returns>List of all credit scores</returns>
        Task<List<CreditScore>> LoadCreditScores();

        #endregion Load

        #region Account Manipulation

        /// <summary>Adds an account to the database.</summary>
        /// <param name="account">Account to be added</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> AddAccount(Account account);

        /// <summary>Deletes an account from the database.</summary>
        /// <param name="account">Account to be deleted</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> DeleteAccount(Account account);

        /// <summary>Renames an account in the database.</summary>
        /// <param name="account">Account to be renamed</param>
        /// <param name="newAccountName">New account's name</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> RenameAccount(Account account, string newAccountName);

        #endregion Account Manipulation

        #region Category Management

        /// <summary>Inserts a new Category into the database.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="newName">Name for new Category</param>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        /// <returns>Returns true if successful.</returns>
        Task<bool> AddCategory(Category selectedCategory, string newName, bool isMajor);

        /// <summary>Rename a category in the database.</summary>
        /// <param name="selectedCategory">Category to rename</param>
        /// <param name="newName">New name of the Category</param>
        /// <param name="oldName">Old name of the Category</param>
        /// <param name="isMajor">Is the category being renamed a Major Category?</param>
        /// <returns></returns>
        Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor);

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        Task<bool> RemoveMajorCategory(Category selectedCategory);

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory);

        #endregion Category Management

        #region Transaction Management

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <param name="account">Account the transaction will be added to</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> AddTransaction(Transaction transaction, Account account);

        /// <summary>Deletes a transaction from the database.</summary>
        /// <param name="transaction">Transaction to be deleted</param>
        /// <param name="account">Account the transaction will be deleted from</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> DeleteTransaction(Transaction transaction, Account account);

        /// <summary>Gets the next Transaction ID autoincrement value in the database for the Transactions table.</summary>
        /// <returns>Next Transactions ID value</returns>
        Task<int> GetNextTransactionsIndex();

        /// <summary>Modifies the selected Transaction in the database.</summary>
        /// <param name="newTransaction">Transaction to replace the current one in the database</param>
        /// <param name="oldTransaction">Current Transaction in the database</param>
        /// <returns>Returns true if successful</returns>
        Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction);

        #endregion Transaction Management
    }
}