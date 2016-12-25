using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Finances
{
    /// <summary>Represents the current state of the application.</summary>
    internal static class AppState
    {
        private const string _DBPROVIDERANDSOURCE = "Data Source = Finances.sqlite;Version=3";

        internal static List<Account> AllAccounts = new List<Account>();
        internal static List<Category> AllCategories = new List<Category>();
        internal static List<Transaction> AllTransactions = new List<Transaction>();
        internal static List<Month> AllMonths = new List<Month>();

        /// <summary>Loads all information from the database.</summary>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> LoadAll()
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection();
            SQLiteDataAdapter da;
            DataSet ds = new DataSet();
            con.ConnectionString = _DBPROVIDERANDSOURCE;

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    string sql = "SELECT * FROM Accounts";
                    da = new SQLiteDataAdapter(sql, con);
                    da.Fill(ds, "Accounts");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Account newAccount = new Account(ds.Tables[0].Rows[i]["Name"].ToString(), new List<Transaction>());

                            AllAccounts.Add(newAccount);
                        }
                    }

                    sql = "SELECT * FROM MajorCategories";
                    ds = new DataSet();
                    da = new SQLiteDataAdapter(sql, con);
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

                    sql = "SELECT * FROM MinorCategories";
                    ds = new DataSet();
                    da = new SQLiteDataAdapter(sql, con);
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

                    sql = "SELECT * FROM Transactions";
                    ds = new DataSet();
                    da = new SQLiteDataAdapter(sql, con);
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
                            AllTransactions.Add(newTransaction);
                        }
                    }

                    AllAccounts = AllAccounts.OrderBy(account => account.Name).ToList();
                    if (AllAccounts.Count > 0)
                    {
                        foreach (Account account in AllAccounts)
                            account.Sort();
                    }

                    AllTransactions = AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
                    LoadMonths();
                    success = true;
                }
                catch (Exception ex)
                {
                    new Notification(ex.Message, "Error Filling DataSet", NotificationButtons.OK).ShowDialog();
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Loads all the Months from AllTransactions.</summary>
        internal static void LoadMonths()
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

        #region Transaction Manipulation

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <param name="account">Account the transaction will be added to</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> AddTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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
        internal static async Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction)
        {
            bool success = false;

            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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
                    AllTransactions[AllTransactions.IndexOf(oldTransaction)] = newTransaction;
                    AllTransactions = AllTransactions.OrderByDescending(transaction => transaction.Date).ToList();
                    LoadMonths();
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
        internal static async Task<bool> DeleteTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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

        #endregion Transaction Manipulation

        #region Account Manipulation

        /// <summary>Adds an account to the database.</summary>
        /// <param name="account">Account to be added</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> AddAccount(Account account)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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
        internal static async Task<bool> DeleteAccount(Account account)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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

                    AllAccounts.Remove(account);
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
        internal static async Task<bool> RenameAccount(Account account, string newAccountName)
        {
            bool success = false;
            string oldAccountName = account.Name;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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

                    account.Name = newAccountName;

                    foreach (Transaction transaction in AllTransactions)
                    {
                        if (transaction.Account == oldAccountName)
                            transaction.Account = newAccountName;
                    }
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
        internal static async Task<bool> AddCategory(Category selectedCategory, string newName, bool isMajor)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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

                        if (isMajor)
                        {
                            AllCategories.Add(new Category(
                                name: newName,
                                minorCategories: new List<string>()));
                        }
                        else
                            selectedCategory.MinorCategories.Add(newName);

                        AllCategories = AllCategories.OrderBy(category => category.Name).ToList();
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
        internal static async Task<bool> RenameCategory(Category selectedCategory, string newName, string oldName, bool isMajor)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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

                    if (isMajor)
                    {
                        selectedCategory = AllCategories.Find(category => category.Name == selectedCategory.Name);
                        selectedCategory.Name = newName;
                        AllTransactions.Select(transaction => transaction.MajorCategory == oldName ? newName : oldName).ToList();
                    }
                    else
                    {
                        selectedCategory = AllCategories.Find(category => category.Name == selectedCategory.Name);
                        selectedCategory.MinorCategories.Select(category => category == oldName ? newName : category).ToList();
                        AllTransactions.Select(transaction => transaction.MinorCategory == oldName ? newName : oldName).ToList();
                    }

                    AllCategories = AllCategories.OrderBy(category => category.Name).ToList();
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
        internal static async Task<bool> RemoveMajorCategory(Category selectedCategory)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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
        internal static async Task<bool> RemoveMinorCategory(Category selectedCategory, string minorCategory)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
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

                    foreach (Transaction transaction in AllTransactions)
                    {
                        if (transaction.MajorCategory == selectedCategory.Name && transaction.MinorCategory == minorCategory)
                            transaction.MinorCategory = "";
                    }

                    selectedCategory = AllCategories.Find(category => category.Name == selectedCategory.Name);
                    selectedCategory.MinorCategories.Remove(minorCategory);
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

        /// <summary>Turns several Keyboard.Keys into a list of Keys which can be tested using List.Any.</summary>
        /// <param name="keys">Array of Keys</param>
        /// <returns>List of Keyboard.IsKeyDown states</returns>
        internal static List<bool> GetListOfKeys(params Key[] keys)
        {
            List<bool> allKeys = new List<bool>();
            foreach (Key key in keys)
                allKeys.Add(Keyboard.IsKeyDown(key));
            return allKeys;
        }
    }
}