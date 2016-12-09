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

                    sql = "SELECT * FROM Transactions";
                    ds = new DataSet();
                    da = new SQLiteDataAdapter(sql, con);
                    da.Fill(ds, "Transactions");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Account selectedAccount = AllAccounts.Find(account => account.Name == ds.Tables[0].Rows[i]["Account"].ToString());

                            selectedAccount.AddTransaction(new Transaction(DateTimeHelper.Parse(ds.Tables[0].Rows[i]["Date"]), ds.Tables[0].Rows[i]["Payee"].ToString(), ds.Tables[0].Rows[i]["MajorCategory"].ToString(), ds.Tables[0].Rows[i]["MinorCategory"].ToString(), ds.Tables[0].Rows[i]["Memo"].ToString(), DecimalHelper.Parse(ds.Tables[0].Rows[i]["Outflow"]), DecimalHelper.Parse(ds.Tables[0].Rows[i]["Inflow"])));
                        }
                    }
                    AllAccounts = AllAccounts.OrderBy(account => account.Name).ToList();
                    for (int i = 0; i < AllAccounts.Count; i++)
                        AllAccounts[i].Sort();
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
            cmd.CommandText = "INSERT INTO Transactions([Date],[Payee],[MajorCategory],[MinorCategory],[Memo],[Outflow],[Inflow],[Account])Values('" + transaction.DateToString + "','" + transaction.Payee + "','" + transaction.MajorCategory + "','" + transaction.MinorCategory + "','" + transaction.Memo + "','" + transaction.Outflow + "','" + transaction.Inflow + "','" + account.Name + "')";

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
            cmd.CommandText = "INSERT INTO Accounts([Name],[Balance])Values('" + account.Name + "','" + account.Balance + "')";
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

<<<<<<< HEAD
        /// <summary>
        /// Rename a category in the database.
        /// </summary>
        /// <param name="category">Category to rename</param>
        /// <param name="newName">New name of the Category</param>
        /// <returns></returns>
        internal static async Task<bool> RenameCategory(String oldName, String newName)
=======
        /// <summary>Deletes an account from the database.</summary>
        /// <param name="account">Account to be deleted</param>
        /// <returns>Returns true if successful</returns>
        internal static async Task<bool> DeleteAccount(Account account)
>>>>>>> refs/remotes/origin/master
        {
            bool success = false;
            SQLiteConnection con = new SQLiteConnection();
            con.ConnectionString = _DBPROVIDERANDSOURCE;
            SQLiteCommand cmd = con.CreateCommand();
<<<<<<< HEAD
            cmd.CommandText = "UPDATE MajorCategories SET [Name] = @new WHERE [Name] = @old ; UPDATE MinorCategories SET [MajorCategory] = @new WHERE [MajorCategory] = @old; UPDATE MinorCategories SET [MinorCategory] = @new WHERE [MinorCategory] = @old";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@new", newName);
            cmd.Parameters.AddWithValue("@old", oldName);
=======
            cmd.CommandText = "DELETE FROM Accounts WHERE [Name] = @name";
            cmd.Parameters.AddWithValue("@name", account.Name);

>>>>>>> refs/remotes/origin/master
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.ExecuteNonQuery();
<<<<<<< HEAD
=======

                    cmd.CommandText = "DELETE FROM Transactions WHERE [Account] = @name";
                    cmd.ExecuteNonQuery();

                    AllAccounts.Remove(account);
>>>>>>> refs/remotes/origin/master
                    success = true;
                }
                catch (Exception ex)
                {
<<<<<<< HEAD
                    MessageBox.Show(ex.Message, "Error Renaming Category.", MessageBoxButton.OK);
=======
                    MessageBox.Show(ex.Message, "Error Deleting Account", MessageBoxButton.OK);
>>>>>>> refs/remotes/origin/master
                }
                finally { con.Close(); }
            });
            return success;
        }

<<<<<<< HEAD
=======
        #endregion Account Manipulation

>>>>>>> refs/remotes/origin/master
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