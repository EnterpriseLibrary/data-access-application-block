// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlDatabaseFixtures
{
    /// <summary>
    /// Tests the ExecuteScalar Method of the Database Class
    /// </summary>
    [TestClass]
    public class ExecuteScalarFixture : EntLibFixtureBase
    {
        private String itemsXMLfile;
        private DataSet dsExpectedResult = new DataSet();

        public ExecuteScalarFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        [DeploymentItem(@"Testfiles\items.xml")]
        public override void Initialize()
        {
            base.Initialize();

            itemsXMLfile = "Items.xml";
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #region "DB Command"

        [TestMethod]
        [DeploymentItem(@"Testfiles\items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithText()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string sqlCommand = "select ItemDescription from items where ItemID=1";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            object actualResult = db.ExecuteScalar(dbCommandWrapper);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string spName = "ItemsDescriptionGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            object actualResult = db.ExecuteScalar(dbCommandWrapper);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingDbCommandWithStoredProcedureAndParameters()
        {
            string spName = "CustomerOrdersAddwithParm";
            SqlDatabase db = (SqlDatabase)DatabaseFactory.CreateDatabase("DataSQLTest");
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.AddInParameter(dbCommandWrapper, "@CustomerId", SqlDbType.Int, "19");
            db.AddInParameter(dbCommandWrapper, "@CustomerName", SqlDbType.VarChar, "google");
            db.AddInParameter(dbCommandWrapper, "@ItemId", SqlDbType.Int, 3);
            db.AddInParameter(dbCommandWrapper, "@QtyOrdered", SqlDbType.Int, 300);
            object actualResult = db.ExecuteScalar(dbCommandWrapper);
            Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 300);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ExceptionIsThrownWhenStoredProcedureNameDoesNotExist()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string sqlCommand = "UpdateItem1";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            object actualResult = db.ExecuteScalar(dbCommandWrapper); { }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        #endregion

        #region "Command Type, Command Text"

        [TestMethod]
        [DeploymentItem(@"Testfiles\items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithCommandText()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string sqlCommand = "select ItemDescription from items where ItemID=1";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            object actualResult = db.ExecuteScalar(CommandType.Text, sqlCommand);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithCommandStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);

            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string spName = "ItemsDescriptionGet";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(spName);
            object actualResult = db.ExecuteScalar(CommandType.StoredProcedure, spName);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        #endregion

        #region "Command, Transaction"

        [TestMethod]
        public void ResultIsReturnedWhenExecuteScalarUsingTransWithCommandParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand dbCommandWrapper = db.GetStoredProcCommand("CustomerOrdersAddwithParm");
                CreateCommandParameter(db, dbCommandWrapper, "@CustomerId", DbType.Int32, ParameterDirection.Input, "20");
                CreateCommandParameter(db, dbCommandWrapper, "@CustomerName", DbType.String, ParameterDirection.Input, "MSoft");
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int32, ParameterDirection.Input, "2");
                CreateCommandParameter(db, dbCommandWrapper, "@QtyOrdered", DbType.Int32, ParameterDirection.Input, "500");
                object actualResult = db.ExecuteScalar(dbCommandWrapper, transaction);
                transaction.Commit();
                Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 500);
                connection.Close();
            }
        }

        [TestMethod]
        public void OriginalValueIsReturnedWhenSPCommandRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                bool value = true;
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand dbCommandWrapper = db.GetStoredProcCommand("CustomerOrdersAddwithParm");
                CreateCommandParameter(db, dbCommandWrapper, "@CustomerId", DbType.Int32, ParameterDirection.Input, "21");
                CreateCommandParameter(db, dbCommandWrapper, "@CustomerName", DbType.String, ParameterDirection.Input, "oracle");
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int32, ParameterDirection.Input, "2");
                CreateCommandParameter(db, dbCommandWrapper, "@QtyOrdered", DbType.Int32, ParameterDirection.Input, "500");
                db.ExecuteScalar(dbCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
                SqlConnection conn = new SqlConnection();
                StringBuilder readerDataExpected = new StringBuilder();
                conn.ConnectionString = connStr;
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select QtyOrdered from CustomersOrders where customerName='oracle'", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    readerDataExpected.Append(dr["QtyOrdered"]);
                    value = false;
                }
                conn.Close();
                Assert.IsTrue(value);
            }
        }

        #endregion

        #region "SP, Parm"

        [TestMethod]
        public void ValueIsReturnedWhenUsingStoredProcWithParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string spName = "CustomerOrdersAddwithParm";
            object actualResult = db.ExecuteScalar(spName, new object[] { 22, "apple", 1, 800 });
            Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 800);
        }

        #endregion

        #region "Transaction, SP, Parm"

        [TestMethod]
        public void ValueIsReturnedWhenUsingStoredProcedureWithParametersWithTransaction()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string spName = "CustomerOrdersAddwithParm";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                object actualResult = db.ExecuteScalar(transaction, spName, new object[] { 23, "airtel", 1, 900 });
                transaction.Commit();
                Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 900);
                connection.Close();
            }
        }

        [TestMethod]
        public void RecordIsNotInsertedWhenUsingStoredProcWithParametersAndTransactionRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string spName = "CustomerOrdersAddwithParm";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteScalar(transaction, spName, new object[] { 24, "aircel", 1, 1000 });
                transaction.Rollback();
                connection.Close();
                SqlConnection conn = new SqlConnection();
                StringBuilder readerDataExpected = new StringBuilder();
                conn.ConnectionString = connStr;
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select QtyOrdered from CustomersOrders where customerName='aircel'", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    readerDataExpected.Append(dr["QtyOrdered"]);
                }
                conn.Close();
                Assert.AreEqual(readerDataExpected.ToString(), "");
            }
        }

        #endregion

        #region "Transaction, CommandType, Command Text"

        [TestMethod]
        public void RecordIsInsertedWhenExecuteScalarUsingTransAndText()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                object actualResult = db.ExecuteScalar(transaction, CommandType.Text, "begin Insert into CustomersOrders values(5,'orange',3,600) Select count(*) from CustomersOrders where CustomerID='5' end");
                transaction.Commit();
                Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 1);
                connection.Close();
            }
        }

        [TestMethod]
        public void RecordIsNotInsertedWhenExecuteScalarAndTransRollbackWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                bool value = true;
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteScalar(transaction, CommandType.Text, "Insert into CustomersOrders values(6,'Hutch',3,200) ");
                transaction.Rollback();
                connection.Close();
                SqlConnection conn = new SqlConnection();
                StringBuilder readerDataActual = new StringBuilder();
                conn.ConnectionString = connStr;
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select CustomerName from CustomersOrders where CustomerID='6' ", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    readerDataActual.Append(dr["CustomerName"]);
                    value = false;
                }
                conn.Close();
                Assert.IsTrue(value);
            }
        }

        [TestMethod]
        public void RecordIsInsertedWhenExecuteScalarAndTransCommitedWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                string spName = "CustomerOrdersAdd";
                DbTransaction transaction = connection.BeginTransaction();
                object actualResult = db.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
                transaction.Commit();
                Assert.AreEqual(actualResult.ToString().Trim(), "David");
                connection.Close();
            }
        }

        [TestMethod]
        public void RecordIsNotInsertedWhenExecuteScalarAndTransRollbackWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                bool value = true;
                string spName = "OrderUpdate";
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
                transaction.Rollback();
                connection.Close();
                SqlConnection conn = new SqlConnection();
                StringBuilder readerDataActual = new StringBuilder();
                conn.ConnectionString = connStr;
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select CustomerName from CustomersOrders where QtyOrdered=150", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    readerDataActual.Append(dr["QtyOrdered"]);
                    value = false;
                }
                conn.Close();
                Assert.IsTrue(value);
            }
        }

        #endregion
    }
}

