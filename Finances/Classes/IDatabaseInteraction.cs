using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finances
{
    /// <summary>Represents required interactions for implementations of databases.</summary>
    internal interface IDatabaseInteraction
    {
        bool VerifyDatabaseIntegrity();

        #region Load

        Task<List<Account>> LoadAccounts();

        Task<List<string>> LoadAccountTypes();

        Task<List<Category>> LoadCategories();

        #endregion Load

        #region Transaction Management

        Task<bool> AddTransaction(Transaction transaction, Account account);

        Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction);

        Task<bool> DeleteTransaction(Transaction transaction, Account account);

        #endregion Transaction Management

        #region Account Manipulation

        Task<bool> AddAccount(Account account);

        Task<bool> DeleteAccount(Account account);

        Task<bool> RenameAccount(Account account, string newAccountName);

        #endregion Account Manipulation

        #region Category Management

        Task<bool> AddCategory(Category selectedCategory, string newName, bool isMajor);

        Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor);

        Task<bool> RemoveMajorCategory(Category selectedCategory);

        Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory);

        #endregion Category Management
    }
}