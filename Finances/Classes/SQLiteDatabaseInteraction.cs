using Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace Finances
{
    internal class SQLiteDatabaseInteraction
    {
        private readonly SQLiteConnection con = new SQLiteConnection { ConnectionString = "Data Source = Finances.sqlite;Version=3" };

        #region Load

        /// <summary>Loads all Accounts.</summary>
        /// <returns>Returns all Accounts</returns>
        internal async Task<List<Account>> LoadAccounts()
        {
            List<Account> AllAccounts = new List<Account>();
            DataSet ds = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM Accounts", con);
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    da.Fill(ds, "Accounts");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Account newAccount = new Account(ds.Tables[0].Rows[i]["Name"].ToString(),
                                new List<Transaction>());

                            AllAccounts.Add(newAccount);
                        }
                    }

                    ds = new DataSet();
                    da = new SQLiteDataAdapter("SELECT * FROM Transactions", con);
                    da.Fill(ds, "Transactions");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Account selectedAccount = AllAccounts.Find(account => account.Name == ds.Tables[0].Rows[i]["Account"].ToString());

                            Transaction newTransaction = new Transaction(
                                date: DateTimeHelper.Parse(ds.Tables[0].Rows[i]["Date"]),
                                payee: ds.Tables[0].Rows[i]["Payee"].ToString(),
                                majorCategory: ds.Tables[0].Rows[i]["MajorCategory"].ToString(),
                                minorCategory: ds.Tables[0].Rows[i]["MinorCategory"].ToString(),
                                memo: ds.Tables[0].Rows[i]["Memo"].ToString(),
                                outflow: DecimalHelper.Parse(ds.Tables[0].Rows[i]["Outflow"]),
                                inflow: DecimalHelper.Parse(ds.Tables[0].Rows[i]["Inflow"]),
                                account: selectedAccount.Name);
                            selectedAccount.AddTransaction(newTransaction);
                        }
                    }

                    AllAccounts = AllAccounts.OrderBy(account => account.Name).ToList();
                    if (AllAccounts.Count > 0)
                    {
                        foreach (Account account in AllAccounts)
                            account.Sort();
                    }
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Loading Accounts", NotificationButtons.OK).ShowDialog();
                }
                finally
                {
                    con.Close();
                }
            });

            return AllAccounts;
        }

        /// <summary>Loads all Categories.</summary>
        /// <returns>Returns all Categories</returns>
        internal async Task<List<Category>> LoadCategories()
        {
            List<Category> AllCategories = new List<Category>();
            DataSet ds = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM MajorCategories", con);
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    da.Fill(ds, "MajorCategories");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Category newCategory = new Category(ds.Tables[0].Rows[i]["Name"].ToString(), new List<string>());

                            AllCategories.Add(newCategory);
                        }
                    }
                    AllCategories = AllCategories.OrderBy(category => category.Name).ToList();

                    ds = new DataSet();
                    da = new SQLiteDataAdapter("SELECT * FROM MinorCategories", con);
                    da.Fill(ds, "MinorCategories");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Category selectedCategory = AllCategories.Find(category => category.Name == ds.Tables[0].Rows[i]["MajorCategory"].ToString());

                            selectedCategory.MinorCategories.Add(ds.Tables[0].Rows[i]["MinorCategory"].ToString());
                        }
                    }

                    foreach (Category category in AllCategories)
                        category.Sort();
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Loading Categories", NotificationButtons.OK).ShowDialog();
                }
                finally
                {
                    con.Close();
                }
            });

            return AllCategories;
        }

        #endregion Load

        #region Transaction Management

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <param name="account">Account the transaction will be added to</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> AddTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Transactions([Date],[Payee],[MajorCategory],[MinorCategory],[Memo],[Outflow],[Inflow],[Account])Values('" + transaction.DateToString.Replace("'", "''") + "','" + transaction.Payee.Replace("'", "''") + "','" + transaction.MajorCategory.Replace("'", "''") + "','" + transaction.MinorCategory.Replace("'", "''") + "','" + transaction.Memo.Replace("'", "''") + "','" + transaction.Outflow + "','" + transaction.Inflow + "','" + account.Name.Replace("'", "''") + "')";

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand
                    {
                        Connection = con,
                        CommandText = "UPDATE Accounts SET [Balance] = @balance WHERE [Name] = @name"
                    };
                    cmd.Parameters.AddWithValue("@balance", account.Balance);
                    cmd.Parameters.AddWithValue("@name", account.Name);
                    cmd.ExecuteNonQuery();

                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Adding New Transaction", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Modifies the selected Transaction in the database.</summary>
        /// <param name="newTransaction">Transaction to replace the current one in the database</param>
        /// <param name="oldTransaction">Current Transaction in the database</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction)
        {
            bool success = false;
            SQLiteCommand cmd = new SQLiteCommand
            {
                Connection = con,
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
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Modifying Transaction", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Deletes a transaction from the database.</summary>
        /// <param name="transaction">Transaction to be deleted</param>
        /// <param name="account">Account the transaction will be deleted from</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> DeleteTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Transactions WHERE [Date] = @date AND [Payee] = @payee AND [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory AND [Memo] = @memo AND [Outflow] = @outflow AND [Inflow] = @inflow AND [Account] = @account";
            cmd.Parameters.AddWithValue("@date", transaction.DateToString);
            cmd.Parameters.AddWithValue("@payee", transaction.Payee);
            cmd.Parameters.AddWithValue("@majorCategory", transaction.MajorCategory);
            cmd.Parameters.AddWithValue("@minorCategory", transaction.MinorCategory);
            cmd.Parameters.AddWithValue("@memo", transaction.Memo);
            cmd.Parameters.AddWithValue("@outflow", transaction.Outflow);
            cmd.Parameters.AddWithValue("@inflow", transaction.Inflow);
            cmd.Parameters.AddWithValue("@account", account.Name);

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Deleting Transaction", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        #endregion Transaction Management

        #region Account Manipulation

        /// <summary>Adds an account to the database.</summary>
        /// <param name="account">Account to be added</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> AddAccount(Account account)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Accounts([Name],[Balance])Values('" + account.Name.Replace("'", "''") + "','" + account.Balance + "')";
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Creating New Account", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Deletes an account from the database.</summary>
        /// <param name="account">Account to be deleted</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> DeleteAccount(Account account)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Accounts WHERE [Name] = @name";
            cmd.Parameters.AddWithValue("@name", account.Name);

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DELETE FROM Transactions WHERE [Account] = @name";
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Deleting Account", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Renames an account in the database.</summary>
        /// <param name="account">Account to be renamed</param>
        /// <param name="newAccountName">New account's name</param>
        /// <returns>Returns true if successful</returns>
        internal async Task<bool> RenameAccount(Account account, string newAccountName)
        {
            bool success = false;
            string oldAccountName = account.Name;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "UPDATE Accounts SET [Name] = @newAccountName WHERE [Name] = @oldAccountName";
            cmd.Parameters.AddWithValue("@newAccountName", newAccountName.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldAccountName", oldAccountName.Replace("'", "''"));

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "UPDATE Transactions SET [Account] = @newAccountName WHERE [Account] = @oldAccountName";
                    cmd.ExecuteNonQuery();

                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Renaming Account", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
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
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = isMajor
                ? "INSERT INTO MajorCategories([Name])Values('" + newName.Replace("'", "''") + "')"
                : "INSERT INTO MinorCategories([MajorCategory],[MinorCategory])Values('" +
                  selectedCategory.Name.Replace("'", "''") + "','" + newName.Replace("'", "''") + "')";

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Creating New Category", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Rename a category in the database.</summary>
        /// <param name="selectedCategory">Category to rename</param>
        /// <param name="newName">New name of the Category</param>
        /// <param name="oldName">Old name of the Category</param>
        /// <param name="isMajor">Is the category being renamed a Major Category?</param>
        /// <returns></returns>
        internal async Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();

            cmd.CommandText = isMajor ? "UPDATE MajorCategories SET [Name] = @newName WHERE [Name] = @oldName; UPDATE MinorCategories SET [MajorCategory] = @newName WHERE [MajorCategory] = @oldName" : "UPDATE MinorCategories SET[MinorCategory] = @newName WHERE[MinorCategory] = @oldName AND [MajorCategory] = @majorCategory";
            cmd.Parameters.AddWithValue("@newName", newName.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@oldName", oldName.Replace("'", "''"));
            cmd.Parameters.AddWithValue("@majorCategory", selectedCategory.Name.Replace("'", "''"));
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Renaming Category", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        internal async Task<bool> RemoveMajorCategory(Category selectedCategory)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText =
                "DELETE FROM MajorCategories WHERE [Name] = @name; DELETE FROM MinorCategories WHERE [MajorCategory] = @name; UPDATE Transactions SET [MajorCategory] = @newName AND [MinorCategory] = @newName WHERE [MinorCategory] = @name";
            cmd.Parameters.AddWithValue("@name", selectedCategory.Name);
            cmd.Parameters.AddWithValue("@newName", "");

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();

                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Deleting Account", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Removes a Major Category from the database, as well as removes it from all Transactions which utilize it.</summary>
        /// <param name="selectedCategory">Selected Major Category</param>
        /// <param name="minorCategory">Selected Minor Category to delete</param>
        /// <returns>Returns true if operation successful</returns>
        internal async Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory)
        {
            bool success = false;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText =
                "DELETE FROM MinorCategories WHERE [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory; UPDATE Transactions SET [MinorCategory] = @newMinorName WHERE [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory";
            cmd.Parameters.AddWithValue("@majorCategory", selectedCategory.Name);
            cmd.Parameters.AddWithValue("@minorCategory", minorCategory);
            cmd.Parameters.AddWithValue("@newMinorName", "");

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Deleting Account", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        #endregion Category Management
    }
}