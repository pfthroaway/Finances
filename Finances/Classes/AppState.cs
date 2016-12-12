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
            SQLiteDataAdapter da = new SQLiteDataAdapter();
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
                        for (int i = 0; i < AllAccounts.Count; i++)
                            AllAccounts[i].Sort();
                    }

                    AllTransactions = AllTransactions.OrderBy(transaction => transaction.Date).ToList();
                    if (AllTransactions.Count > 0)
                    {
                        int months = ((DateTime.Now.Year - AllTransactions[0].Date.Year) * 12) + DateTime.Now.Month - AllTransactions[0].Date.Month;
                        DateTime startMonth = new DateTime(AllTransactions[0].Date.Year, AllTransactions[0].Date.Month, 1);

                        int start = 0;
                        do
                        {
                            AllMonths.Add(new Month(startMonth.AddMonths(start), new List<Transaction>()));
                            start += 1;
                        }
                        while (start <= months);

                        for (int i = 0; i < AllTransactions.Count; i++)
                        {
                            AllMonths.Find(month => month.MonthStart <= AllTransactions[i].Date && AllTransactions[i].Date <= month.MonthEnd.Date).AddTransaction(AllTransactions[i]);
                        }

                        AllMonths = AllMonths.OrderByDescending(month => month.FormattedMonth).ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Filling DataSet", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });

            return success;
        }

        #region Transaction Manipulation

        /// <summary>Adds a transaction to an account and the database</summary>
        /// <param name="transaction">Transaction to be added</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> AddTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Transactions([Date],[Payee],[MajorCategory],[MinorCategory],[Memo],[Outflow],[Inflow],[Account])Values('" + transaction.DateToString.Replace("'", "''") + "','" + transaction.Payee.Replace("'", "''") + "','" + transaction.MajorCategory.Replace("'", "''") + "','" + transaction.MinorCategory.Replace("'", "''") + "','" + transaction.Memo.Replace("'", "''") + "','" + transaction.Outflow + "','" + transaction.Inflow + "','" + account.Name.Replace("'", "''") + "')";

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();

                    cmd = new SQLiteCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "UPDATE Accounts SET [Balance] = @balance WHERE [Name] = @name";
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
                    MessageBox.Show(ex.Message, "Error Adding New Transaction", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Modifies the selected Transaction in the database.</summary>
        /// <param name="newTransaction">Transaction to replace the current one in the database</param>
        /// <param name="oldTransaction">Current Transaction in the database</param>
        /// <param name="account">Account the transaction is associated with</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> ModifyTransaction(Transaction newTransaction, Transaction oldTransaction, Account account)
        {
            bool success = false;

            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;

            cmd.CommandText = "UPDATE Transactions SET [Date] = @date AND [Payee] = @payee AND [MajorCategory] = @majorCategory AND [MinorCategory] = @minorCategory AND [Memo] = @memo AND [Outflow] = @outflow AND [Inflow] = @inflow AND [Account] = @account WHERE [Date] = @oldDate AND [Payee] = @oldPayee AND [MajorCategory] = @oldMajorCategory AND [MinorCategory] = @oldMinorCategory AND [Memo] = @oldMemo AND [Outflow] = @oldOutflow AND [Inflow] = @oldInflow AND [Account] = @oldAccount";
            cmd.Parameters.AddWithValue("@date", newTransaction.DateToString);
            cmd.Parameters.AddWithValue("@payee", newTransaction.Payee);
            cmd.Parameters.AddWithValue("@majorCategory", newTransaction.MajorCategory);
            cmd.Parameters.AddWithValue("@minorCategory", newTransaction.MinorCategory);
            cmd.Parameters.AddWithValue("@memo", newTransaction.Memo);
            cmd.Parameters.AddWithValue("@outflow", newTransaction.Outflow);
            cmd.Parameters.AddWithValue("@inflow", newTransaction.Inflow);
            cmd.Parameters.AddWithValue("@account", account.Name);
            cmd.Parameters.AddWithValue("@oldDate", newTransaction.DateToString);
            cmd.Parameters.AddWithValue("@oldPayee", newTransaction.Payee);
            cmd.Parameters.AddWithValue("@oldMajorCategory", newTransaction.MajorCategory);
            cmd.Parameters.AddWithValue("@oldMinorCategory", newTransaction.MinorCategory);
            cmd.Parameters.AddWithValue("@oldMemo", newTransaction.Memo);
            cmd.Parameters.AddWithValue("@oldOutflow", newTransaction.Outflow);
            cmd.Parameters.AddWithValue("@oldInflow", newTransaction.Inflow);
            cmd.Parameters.AddWithValue("@oldAccount", account.Name);
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving Hero Bank", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });

            return success;
        }

        /// <summary>Deletes a transaction from the database.</summary>
        /// <param name="transaction">Transaction to be deleted</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> DeleteTransaction(Transaction transaction, Account account)
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;
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
                    MessageBox.Show(ex.Message, "Error Deleting Transaction", MessageBoxButton.OK);
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
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;
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
                    MessageBox.Show(ex.Message, "Error Creating New Account", MessageBoxButton.OK);
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
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;
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
                    MessageBox.Show(ex.Message, "Error Deleting Account", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });
            return success;
        }

        /// <summary>Renames an account in the database.</summary>
        /// <param name="account">Account to be renamed</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> RenameAccount(Account account, string newAccountName)
        {
            bool success = false;
            string oldAccountName = account.Name;
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;
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

                    for (int i = 0; i < AllTransactions.Count; i++)
                    {
                        if (AllTransactions[i].Account == oldAccountName)
                            AllTransactions[i].Account = newAccountName;
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Deleting Account", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });
            return success;
        }

        #endregion Account Manipulation

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