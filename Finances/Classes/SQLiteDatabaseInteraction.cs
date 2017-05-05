using Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Finances
{
    /// <summary>Represents database interaction covered by SQLite.</summary>
    public class SQLiteDatabaseInteraction : IDatabaseInteraction
    {
        // ReSharper disable once InconsistentNaming
        private const string _DATABASENAME = "Finances.sqlite";

        private readonly string _con = $"Data Source = {_DATABASENAME};Version=3";

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        /// <returns>Returns true once the database has been validated</returns>
        public void VerifyDatabaseIntegrity()
        {
            Functions.VerifyFileIntegrity(Assembly.GetExecutingAssembly().GetManifestResourceStream($"Finances.{_DATABASENAME}"),
                _DATABASENAME);
        }

        #region Load

        /// <summary>Loads all Accounts.</summary>
        /// <returns>Returns all Accounts</returns>
        public async Task<List<Account>> LoadAccounts()
        {
            List<Account> allAccounts = new List<Account>();
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM Accounts", _con);

            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Enum.TryParse(dr["Type"].ToString(), out AccountTypes currentType);
                    Account newAccount = new Account(dr["Name"].ToString(), currentType,
                        new List<Transaction>());

                    allAccounts.Add(newAccount);
                }
            }

            ds = await SQLite.FillDataSet("SELECT * FROM Transactions", _con);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Account selectedAccount = allAccounts.Find(account => account.Name == dr["Account"].ToString());

                    Transaction newTransaction = new Transaction(
                        date: DateTimeHelper.Parse(dr["Date"]),
                        payee: dr["Payee"].ToString(),
                        majorCategory: dr["MajorCategory"].ToString(),
                        minorCategory: dr["MinorCategory"].ToString(),
                        memo: dr["Memo"].ToString(),
                        outflow: DecimalHelper.Parse(dr["Outflow"]),
                        inflow: DecimalHelper.Parse(dr["Inflow"]),
                        account: selectedAccount.Name);
                    selectedAccount.AddTransaction(newTransaction);
                }
            }

            allAccounts = allAccounts.OrderBy(account => account.Name).ToList();
            if (allAccounts.Count > 0)
            {
                foreach (Account account in allAccounts)
                    account.Sort();
            }

            return allAccounts;
        }

        /// <summary>Loads all Account types.</summary>
        /// <returns>Returns all Account types</returns>
        public async Task<List<string>> LoadAccountTypes()
        {
            List<string> allAccountTypes = new List<string>();
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM AccountTypes", _con);

            if (ds.Tables[0].Rows.Count > 0)
            {
                allAccountTypes.AddRange(from DataRow dr in ds.Tables[0].Rows select dr["Name"].ToString());
            }

            return allAccountTypes;
        }

        /// <summary>Loads all Categories.</summary>
        /// <returns>Returns all Categories</returns>
        public async Task<List<Category>> LoadCategories()
        {
            List<Category> allCategories = new List<Category>();
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM MajorCategories", _con);

            if (ds.Tables[0].Rows.Count > 0)
            {
                allCategories.AddRange(from DataRow dr in ds.Tables[0].Rows select new Category(dr["Name"].ToString(), new List<string>()));
            }
            allCategories = allCategories.OrderBy(category => category.Name).ToList();

            ds = await SQLite.FillDataSet("SELECT * FROM MinorCategories", _con);

            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Category selectedCategory = allCategories.Find(category => category.Name == dr["MajorCategory"].ToString());

                    selectedCategory.MinorCategories.Add(dr["MinorCategory"].ToString());
                }
            }

            foreach (Category category in allCategories)
                category.Sort();

            return allCategories;
        }

        #endregion Load

        #region Transaction Management

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <param name="account">Account the transaction will be added to</param>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> AddTransaction(Transaction transaction, Account account)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText =
                    $"INSERT INTO Transactions([Date],[Payee],[MajorCategory],[MinorCategory],[Memo],[Outflow],[Inflow],[Account])Values('{transaction.DateToString.Replace("'", "''")}','{transaction.Payee.Replace("'", "''")}','{transaction.MajorCategory.Replace("'", "''")}','{transaction.MinorCategory.Replace("'", "''")}','{transaction.Memo.Replace("'", "''")}','{transaction.Outflow}','{transaction.Inflow}','{account.Name.Replace("'", "''")}');UPDATE Accounts SET [Balance] = @balance WHERE [Name] = @name"
            };
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@name", account.Name);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Modifies the selected Transaction in the database.</summary>
        /// <param name="newTransaction">Transaction to replace the current one in the database</param>
        /// <param name="oldTransaction">Current Transaction in the database</param>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "UPDATE Transactions SET [Date] = @date, [Payee] = @payee, [MajorCategory] = @majorCategory, [MinorCategory] = @minorCategory, [Memo] = @memo, [Outflow] = @outflow, [Inflow] = @inflow, [Account] = @account WHERE [Date] = @oldDate AND [Payee] = @oldPayee AND [MajorCategory] = @oldMajorCategory AND [MinorCategory] = @oldMinorCategory AND [Memo] = @oldMemo AND [Outflow] = @oldOutflow AND [Inflow] = @oldInflow AND [Account] = @oldAccount"
            };
            cmd.Parameters.AddWithValue("@date", newTransaction.DateToString.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@payee", newTransaction.Payee.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@majorCategory", newTransaction.MajorCategory.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@minorCategory", newTransaction.MinorCategory.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@memo", newTransaction.Memo.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@outflow", newTransaction.Outflow);
            cmd.Parameters.AddWithValue("@inflow", newTransaction.Inflow);
            cmd.Parameters.AddWithValue("@account", newTransaction.Account.Replace("'", "''"));

            cmd.Parameters.AddWithValue("@oldDate", oldTransaction.DateToString.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldPayee", oldTransaction.Payee.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldMajorCategory", oldTransaction.MajorCategory.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldMinorCategory", oldTransaction.MinorCategory.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldMemo", oldTransaction.Memo.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldOutflow", oldTransaction.Outflow);
            cmd.Parameters.AddWithValue("@oldInflow", oldTransaction.Inflow);
            cmd.Parameters.AddWithValue("@oldAccount", oldTransaction.Account.Replace("'", "''"));

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Deletes a transaction from the database.</summary>
        /// <param name="transaction">Transaction to be deleted</param>
        /// <param name="account">Account the transaction will be deleted from</param>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> DeleteTransaction(Transaction transaction, Account account)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText =
                    "DELETE FROM Transactions WHERE [Date] = @date AND [Payee] = @payee AND [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory AND [Memo] = @memo AND [Outflow] = @outflow AND [Inflow] = @inflow AND [Account] = @account"
            };
            cmd.Parameters.AddWithValue("@date", transaction.DateToString);
            cmd.Parameters.AddWithValue("@payee", transaction.Payee);
            cmd.Parameters.AddWithValue("@majorCategory", transaction.MajorCategory);
            cmd.Parameters.AddWithValue("@minorCategory", transaction.MinorCategory);
            cmd.Parameters.AddWithValue("@memo", transaction.Memo);
            cmd.Parameters.AddWithValue("@outflow", transaction.Outflow);
            cmd.Parameters.AddWithValue("@inflow", transaction.Inflow);
            cmd.Parameters.AddWithValue("@account", account.Name);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Transaction Management

        #region Account Manipulation

        /// <summary>Adds an account to the database.</summary>
        /// <param name="account">Account to be added</param>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> AddAccount(Account account)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText =
                    $"INSERT INTO Accounts([Name],[Type],[Balance])Values('{account.Name.Replace("'", "''")}','{account.AccountType}','{account.Balance}')"
            };

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Deletes an account from the database.</summary>
        /// <param name="account">Account to be deleted</param>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> DeleteAccount(Account account)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "DELETE FROM Accounts WHERE [Name] = @name;DELETE FROM Transactions WHERE [Account] = @name"
            };
            cmd.Parameters.AddWithValue("@name", account.Name);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Renames an account in the database.</summary>
        /// <param name="account">Account to be renamed</param>
        /// <param name="newAccountName">New account's name</param>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> RenameAccount(Account account, string newAccountName)
        {
            string oldAccountName = account.Name;
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "UPDATE Accounts SET [Name] = @newAccountName WHERE [Name] = @oldAccountName;UPDATE Transactions SET[Account] = @newAccountName WHERE[Account] = @oldAccountName"
            };
            cmd.Parameters.AddWithValue("@newAccountName", newAccountName.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldAccountName", oldAccountName.Replace("'", "''"));

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Account Manipulation

        #region Category Management

        /// <summary>Inserts a new Category into the database.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="newName">Name for new Category</param>
        /// <param name="isMajor">Is the category being added a Major Category?</param>
        /// <returns>Returns true if successful.</returns>
        public async Task<bool> AddCategory(Category selectedCategory, string newName, bool isMajor)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = isMajor
                    ? $"INSERT INTO MajorCategories([Name])Values('{newName.Replace("'", "''")}')"
                    : $"INSERT INTO MinorCategories([MajorCategory],[MinorCategory])Values('{selectedCategory.Name.Replace("'", "''")}','{newName.Replace("'", "''")}')"
            };

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Rename a category in the database.</summary>
        /// <param name="selectedCategory">Category to rename</param>
        /// <param name="newName">New name of the Category</param>
        /// <param name="oldName">Old name of the Category</param>
        /// <param name="isMajor">Is the category being renamed a Major Category?</param>
        /// <returns></returns>
        public async Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = isMajor
                    ? "UPDATE MajorCategories SET [Name] = @newName WHERE [Name] = @oldName; UPDATE MinorCategories SET [MajorCategory] = @newName WHERE [MajorCategory] = @oldName"
                    : "UPDATE MinorCategories SET[MinorCategory] = @newName WHERE[MinorCategory] = @oldName AND [MajorCategory] = @majorCategory"
            };
            cmd.Parameters.AddWithValue("@newName", newName.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldName", oldName.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@majorCategory", selectedCategory.Name.Replace("'", "''"));

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        public async Task<bool> RemoveMajorCategory(Category selectedCategory)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText =
                "DELETE FROM MajorCategories WHERE [Name] = @name; DELETE FROM MinorCategories WHERE [MajorCategory] = @name; UPDATE Transactions SET [MajorCategory] = @newName AND [MinorCategory] = @newName WHERE [MinorCategory] = @name"
            };
            cmd.Parameters.AddWithValue("@name", selectedCategory.Name);
            cmd.Parameters.AddWithValue("@newName", "");

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        public async Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText =
                "DELETE FROM MinorCategories WHERE [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory; UPDATE Transactions SET [MinorCategory] = @newMinorName WHERE [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory"
            };
            cmd.Parameters.AddWithValue("@majorCategory", selectedCategory.Name);
            cmd.Parameters.AddWithValue("@minorCategory", minorCategory);
            cmd.Parameters.AddWithValue("@newMinorName", "");

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Category Management
    }
}