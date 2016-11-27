using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Filling DataSet", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });

            return success;
        }

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