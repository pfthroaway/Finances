using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Finances
{
    internal static class AppState
    {
        private const string _DBPROVIDERANDSOURCE = "Data Source = Finances.sqlite;Version=3";

        internal static List<Account> AllAccounts = new List<Account>();
        internal static List<Category> AllCategories = new List<Category>();

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
                            Account newAccount = new Account(ds.Tables[0].Rows[i]["Name"].ToString(), DecimalHelper.Parse(ds.Tables[0].Rows[i]["Balance"]), new List<Transaction>());

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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Filling DataSet", MessageBoxButton.OK);
                }
                finally { con.Close(); }
            });

            return success;
        }
    }
}