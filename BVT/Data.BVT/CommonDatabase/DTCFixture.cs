// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.CommonDatabase
{
    [TestClass]
    public class DTCFixture : EntLibFixtureBase
    {
        public DTCFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        public override void Initialize()
        {
            ServiceHelper.Start("MSDTC");
            System.Threading.Thread.Sleep(15000);
            ConnectionStringsSection section = (ConnectionStringsSection)base.ConfigurationSource.GetSection("connectionStrings");
            connStr = section.ConnectionStrings["DataSQLTest"].ConnectionString;

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Delete  from DTCTable", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            ServiceHelper.Stop("MSDTC");
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        [TestMethod]
        public void TransactionScopeTimesOutAndTransactionIsRolledBack()
        {
            ServiceHelper.Start("MSDTC");
            System.Threading.Thread.Sleep(15000);

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = connStr;
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                SqlCommand cmRowCont = null;

                int numberOfRows = 0;
                try
                {
                    SqlCommand cmd = new SqlCommand("insert into DTCTable Values(1,'Narayan')", conn, trans);
                    cmd.ExecuteNonQuery();
                    cmRowCont = new SqlCommand("select count(eno) from DTCTable", conn, trans);
                    numberOfRows = (int)cmRowCont.ExecuteScalar();
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        //generating an exception
                        Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
                        SqlConnection conn1 = new SqlConnection();
                        conn1.ConnectionString = connStr;
                        //primary key violation 
                        SqlCommand cmd1 = new SqlCommand("insert into DTCTable values(1,'Narayan')", conn1);
                        db.ExecuteNonQuery(cmd1);
                        ts.Complete();
                    }

                    trans.Commit();

                    Assert.Fail("Exception was not raised");
                }
                catch (Exception)
                {
                    trans.Rollback();
                    int numberOfRowsafter = (int)cmRowCont.ExecuteScalar();
                    Assert.AreEqual(numberOfRows - 1, numberOfRowsafter);
                }
            }
        }

        [TestMethod]
        public void TransactionSucceedsWithDTC()
        {
            ServiceHelper.Start("MSDTC");
            System.Threading.Thread.Sleep(15000);
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connStr;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("insert into DTCTable Values(1,'Narayan')", conn);
                    cmd.ExecuteNonQuery();
                }
                // using DAAB with transaction
                Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
                using (DbConnection connection = db.CreateConnection())
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();
                    db.ExecuteNonQuery(transaction, CommandType.Text, "insert into DTCTable Values(2,'Nandan')");
                    transaction.Commit();
                }
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connStr;
                    conn.Open();
                    SqlCommand cmRowCont = new SqlCommand("select count(eno) from DTCTable", conn);
                    int numberOfRows = (int)cmRowCont.ExecuteScalar();
                    Assert.AreEqual(numberOfRows, 2);
                }
                ts.Complete();
            }
            ServiceHelper.Stop("MSDTC");
        }

        [TestMethod]
        public void RowsAreInsertedWithoutTransactionWhenUsingDTC()
        {
            ServiceHelper.Start("MSDTC");
            System.Threading.Thread.Sleep(15000);
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connStr;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("insert into DTCTable Values(1,'Narayan')", conn);
                    cmd.ExecuteNonQuery();
                }
                // using DAAB with transaction
                Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
                using (SqlConnection conn1 = new SqlConnection())
                {
                    conn1.ConnectionString = connStr;
                    SqlCommand cmd = new SqlCommand("insert into DTCTable values(2,'Nandan')", conn1);
                    db.ExecuteNonQuery(cmd);
                }

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connStr;
                    conn.Open();
                    SqlCommand cmRowCont = new SqlCommand("select count(eno) from DTCTable", conn);
                    int numberOfRows = (int)cmRowCont.ExecuteScalar();
                    Assert.AreEqual(numberOfRows, 2);
                    conn.Close();
                }
                ts.Complete();
            }
            ServiceHelper.Stop("MSDTC");
        }

        [TestMethod]
        public void InnerTransactionFailsButOuterTransactionCommitsWhenSuppressingInner()
        {
            ServiceHelper.Start("MSDTC");
            System.Threading.Thread.Sleep(15000);
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connStr;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("insert into DTCTable Values(1,'Narayan')", conn);
                    cmd.ExecuteNonQuery();
                }
                try
                {
                    using (TransactionScope ts1 = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
                        SqlConnection conn1 = new SqlConnection();
                        conn1.ConnectionString = connStr;
                        SqlCommand cmd = new SqlCommand("insert into DTCTable values(1,'Nandan')", conn1);
                        db.ExecuteNonQuery(cmd);
                        ts1.Complete();
                    }
                }
                catch (Exception) { }

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connStr;
                    conn.Open();
                    SqlCommand cmRowCont = new SqlCommand("select count(eno) from DTCTable", conn);
                    int numberOfRows = (int)cmRowCont.ExecuteScalar();
                    Assert.AreEqual(numberOfRows, 1);
                    conn.Close();
                }
                ts.Complete();
            }
            ServiceHelper.Stop("MSDTC");
        }
    }
}

