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
    /// Tests ExecuteReader of the DataBase Class
    /// </summary>
    [TestClass]
    public class ExecuteReaderFixture : EntLibFixtureBase
    {
        private String itemsXMLfile;

        public ExecuteReaderFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
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
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReadUsingExecuteReaderQuery()
        {
            //Expected Data
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "Select ItemDescription from Items order by ItemId";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper))
            {
                while (dataReader.Read())
                {
                    Assert.AreEqual(dsExpectedData.Tables[0].Rows[count][0].ToString(), dataReader["ItemDescription"].ToString().Trim());
                    count++;
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReadUsingExecuteReaderSPNoParam()
        {
            //Expected Data
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper))
            {
                while (dataReader.Read())
                {
                    // Get the value of the 'Name' column in the DataReader
                    Assert.AreEqual(dataReader["ItemDescription"].ToString().Trim(), dsExpectedData.Tables[0].Rows[count][0]);
                    count++;
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReadUsingExecuteReaderDbCommandForSPInParam()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "ItemGetByPrice";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            CreateCommandParameter(db, dbCommandWrapper, "@Price", DbType.Int16, ParameterDirection.Input, 90);
            StringBuilder readerDataActual = new StringBuilder();
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper))
            {
                while (dataReader.Read())
                {
                    readerDataActual.Append(dataReader["ItemDescription"].ToString().Trim());
                }
            }
            Assert.AreEqual("Excel 2003", readerDataActual.ToString());
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReadUsingExecuteReaderDbCommandForSPOutParam()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "ItemPriceGetbyId";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            CreateCommandParameter(db, dbCommandWrapper, "@Id", DbType.Int16, ParameterDirection.Input, 1);
            CreateCommandParameter(db, dbCommandWrapper, "@Price", DbType.Double, ParameterDirection.Output, 32);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper)) { }
            Assert.AreEqual(Convert.ToDouble(38.95), Convert.ToDouble(dbCommandWrapper.Parameters["@Price"].Value));
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreUpdatedAndReadWhenExecuteReaderWithSPInParam()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "ItemQuantityUpdateById";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 1);
            CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 55);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper))
            {
                while (dataReader.Read())
                {
                    if (Convert.ToInt16(dataReader["ItemId"]) == 1)
                    {
                        Assert.AreEqual(55, Convert.ToInt32(dataReader["QtyInHand"]));
                    }
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenExecuteReaderWithNonExistentStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "UpdateItem1";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper)) { }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        #endregion

        #region "Db Command, Transaction"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransWithNoCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "begin update Items set QtyInHand= 88 where ItemId = 2  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);

                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from Items where ItemId = 2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsUpdatedWhenExecuteReaderTransWithCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 45 where ItemId = 1  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Commit();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(45), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransWithRollbackUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "begin update Items set QtyInHand = 67 where ItemId = 2  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Rollback();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void CorrectValuesAreReturnedWhenExecuteReaderTransWithRollbackAndCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 111 where ItemId = 1  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Commit();
                transaction = connection.BeginTransaction();
                sqlCommand = "begin update Items set QtyInHand = 55 where ItemId = 2 select * from Items end";
                dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Rollback();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(111), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void TransactionIsRolledbackWhenExecuteReaderWithException()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            DbTransaction transaction = null;
            try
            {
                using (DbConnection connection = db.CreateConnection())
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    string sqlCommand = "begin update Items set QtyInHand = 45 where ItemId = 2  select * from Items where ItemId = 'Test' end";
                    DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                    using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                    transaction.Commit();
                }
            }
            catch { }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransactionWithNoCommitUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 67);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsUpdatedWhenExecuteReaderTransactionWithCommitUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 1);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 88);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Commit();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(88), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransactionWithRollbackUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 65);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Rollback();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void CorrectValuesAreReturnedWhenExecuteReaderTransactionWithRollbackAndCommitUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 1);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 44);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Commit();
                transaction = connection.BeginTransaction();
                sqlCommand = "ItemQuantityUpdateById";
                dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 72);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Rollback();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(44), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        #endregion

        #region "ExecuteReader(Commandtype)"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsReturnedWhenUsingCommandtypeText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string sqlText = "select ItemDescription From Items where ItemId = 1 ";
            using (IDataReader dataReader = db.ExecuteReader(CommandType.Text, sqlText))
            {
                while (dataReader.Read())
                {
                    readerDataActual.Append(dataReader["ItemDescription"].ToString().Trim());
                }
            }
            Assert.AreEqual("Digital Image Pro", readerDataActual.ToString());

        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReturnedWhenExecuteReaderUsingStoredProc()
        {
            //Expected Data
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "ItemsGet";
            using (IDataReader dataReader = db.ExecuteReader(CommandType.StoredProcedure, spName))
            {
                while (dataReader.Read())
                {
                    Assert.AreEqual(dsExpectedData.Tables[0].Rows[count][0].ToString(), dataReader["ItemDescription"].ToString().Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenExecuteReaderCalledWithNonExistentStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "something";
            using (IDataReader dataReader = db.ExecuteReader(CommandType.StoredProcedure, spName))
            {
                while (dataReader.Read())
                {
                    readerDataActual.Append(dataReader["Name"]);
                    readerDataActual.Append(Environment.NewLine);
                }
            }
        }

        #endregion

        #region "Transaction, Command Type, Command Name"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsUpdatedWhenExecuteReaderTransactionWithCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "begin update Items set QtyInHand = 75 where ItemId = 1 select * from Items end ")) { }
                transaction.Commit();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId=1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(75), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransactionWithNoCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "begin update Items set QtyInHand = 67 where ItemId = 2 select * from Items end ")) { }
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId=2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransactionWithRollbackUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "begin update Items set QtyInHand = 18 where ItemId = 2 select * from Items end ")) { }
                transaction.Rollback();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId=2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsUpdatedWhenExecuteReaderTransactionWithCommitUsingStoredProc1()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.StoredProcedure, "ItemQuantityUpdateById")) { }
                transaction.Commit();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where itemid=1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(50, Convert.ToInt32(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransactionWithNoCommitUsingStoredProc3()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.StoredProcedure, "ItemQuantityIncrease")) { }
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyRequired from Items", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(100, Convert.ToInt32(dr["QtyRequired"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransactionWithRollbackUsingStoredProc2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.StoredProcedure, "ItemQuantityIncrease")) { }
                transaction.Rollback();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyRequired from Items", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(100, Convert.ToInt32(dr["QtyRequired"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        #endregion
    }
}

