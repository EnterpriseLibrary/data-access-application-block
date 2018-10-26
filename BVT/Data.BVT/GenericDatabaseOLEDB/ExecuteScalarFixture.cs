// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.GenericDatabaseOLEDB
{
    /// <summary>
    /// Tests the ExecuteScalar Method of the Database Class
    /// </summary>
    [TestClass]
    public class ExecuteScalarFixture : EntLibFixtureBase
    {
        private string itemsXMLfile;
        private DataSet dsExpectedResult = new DataSet();

        public ExecuteScalarFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        public override void Initialize()
        {
            base.Initialize();

            itemsXMLfile = "Items.xml";
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #region "DBCommand"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "select ItemDescription from items where ItemID=1";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            object actualResult = db.ExecuteScalar(dbCommandWrapper);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string spName = "ItemsDescriptionGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            object actualResult = db.ExecuteScalar(dbCommandWrapper);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ResultIsReturnedWhenExecuteScalarWithCommandParameters()
        {
            string spName = "CustomerOrdersAddwithParm";
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@CustomerId", DbType.Int32, ParameterDirection.Input, "19"));
            dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@CustomerName", DbType.String, ParameterDirection.Input, "google"));
            dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@ItemId", DbType.Int32, ParameterDirection.Input, 3));
            dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@QtyOrdered", DbType.Int32, ParameterDirection.Input, 300));
            object actualResult = db.ExecuteScalar(dbCommandWrapper);
            Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 300);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenExecuteReaderWithNonExistentStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "UpdateItem1";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            object actualResult = db.ExecuteScalar(dbCommandWrapper); { }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        #endregion

        #region "Command Type, Command Text"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithCommandText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "select ItemDescription from items where ItemID=1";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            object actualResult = db.ExecuteScalar(CommandType.Text, sqlCommand);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithCommandStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string spName = "ItemsDescriptionGet";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(spName);
            object actualResult = db.ExecuteScalar(CommandType.StoredProcedure, spName);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        #endregion

        #region "Command, Transaction"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ResultIsReturnedWhenExecuteScalarUsingTransWithCommandParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand dbCommandWrapper = db.GetStoredProcCommand("CustomerOrdersAddwithParm");
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@CustomerId", DbType.Int32, ParameterDirection.Input, "20"));
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@CustomerName", DbType.String, ParameterDirection.Input, "MSoft"));
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@ItemId", DbType.Int32, ParameterDirection.Input, "2"));
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@QtyOrdered", DbType.Int32, ParameterDirection.Input, "500"));
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                object actualResult = db.ExecuteScalar(dbCommandWrapper, transaction);
                transaction.Commit();
                Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 500);
                connection.Close();
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void OriginalValueIsReturnedWhenSPCommandRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                bool value = true;
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand dbCommandWrapper = db.GetStoredProcCommand("CustomerOrdersAddwithParm");
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@CustomerId", DbType.Int32, ParameterDirection.Input, "21"));
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@CustomerName", DbType.String, ParameterDirection.Input, "oracle"));
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@ItemId", DbType.Int32, ParameterDirection.Input, "2"));
                dbCommandWrapper.Parameters.Add(CreateCommandParameter(dbCommandWrapper, "@QtyOrdered", DbType.Int32, ParameterDirection.Input, "500"));
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
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

        #region "Transaction, CommandType, Command Text"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsInsertedWhenExecuteScalarUsingTransAndText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
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
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotInsertedWhenExecuteScalarAndTransRollbackWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
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
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsInsertedWhenExecuteScalarAndTransCommitedWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
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
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotInsertedWhenExecuteScalarAndTransRollbackWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
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

        # region Helper Method

        private DbParameter CreateCommandParameter(DbCommand comm,
                                           string Parametername,
                                           DbType dbType,
                                           ParameterDirection dir,
                                           object value)
        {
            DbParameter cmd = comm.CreateParameter();
            cmd.ParameterName = Parametername;
            cmd.Value = value;
            cmd.Direction = dir;
            cmd.DbType = dbType;
            return (cmd);
        }

        # endregion
    }
}

