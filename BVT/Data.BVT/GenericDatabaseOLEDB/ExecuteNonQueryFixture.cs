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
    /// Tests ExecuteNonQuery of the Database class
    /// </summary>
    [TestClass]
    public class ExecuteNonQueryFixture : EntLibFixtureBase
    {
        public ExecuteNonQueryFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #region "DB Command"

        [TestMethod]
        public void ResultIsReadWhenExecuteNonQueryUsingDBCmd()
        {
            bool isPresent = false;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "Insert into CustomersOrders values(13, 'John',3, 214)";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            db.ExecuteNonQuery(dbCommandWrapper);
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from CustomersOrders where CustomerName = 'John'", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(3, Convert.ToInt32(dr["ItemId"].ToString()));
                Assert.AreEqual(214, Convert.ToInt32(dr["QtyOrdered"].ToString()));
                isPresent = true;
            }

            dr.Close();
            Assert.AreEqual(true, isPresent);
        }

        [TestMethod]
        public void ResultIsReadWhenDBCmdWithSPNoParam()
        {
            bool isPresent = false;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "CustomerOrdersAdd";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            db.ExecuteNonQuery(dbCommandWrapper);
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from CustomersOrders where CustomerName = 'David'", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(3, Convert.ToInt32(dr["ItemId"].ToString()));
                Assert.AreEqual(555, Convert.ToInt32(dr["QtyOrdered"].ToString()));
                isPresent = true;
            }

            dr.Close();
            cmd = new SqlCommand("delete from CustomersOrders where CustomerName = 'David'", conn);
            dr = cmd.ExecuteReader();
            dr.Close();
            conn.Close();
            Assert.AreEqual(true, isPresent);
        }

        [TestMethod]
        public void ResultIsReadWhenDBCmdWithSPInParam()
        {
            bool isPresent = false;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "CustomerOrdersAddwithParm";

            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            CreateCommandParameter(db, dbCommandWrapper, "@CustomerID", DbType.Int16, ParameterDirection.Input, 10);
            CreateCommandParameter(db, dbCommandWrapper, "@CustomerName", DbType.String, ParameterDirection.Input, "Steve");
            CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 3);
            CreateCommandParameter(db, dbCommandWrapper, "@QtyOrdered", DbType.Int16, ParameterDirection.Input, 432);
            db.ExecuteNonQuery(dbCommandWrapper);
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from CustomersOrders where CustomerName = 'Steve'", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(3, Convert.ToInt32(dr["ItemId"].ToString()));
                Assert.AreEqual(432, Convert.ToInt32(dr["QtyOrdered"].ToString()));
                isPresent = true;
            }

            dr.Close();
            Assert.AreEqual(true, isPresent);
        }

        [TestMethod]
        public void ResultIsReadWhenDBCmdWithSPOutParam()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "CustomerOrdersAddwithOutParm";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            CreateCommandParameter(db, dbCommandWrapper, "@CustomerId", DbType.Int16, ParameterDirection.Input, 3);
            CreateCommandParameter(db, dbCommandWrapper, "@QtyOrdered", DbType.Int16, ParameterDirection.Output, 16);
            db.ExecuteNonQuery(dbCommandWrapper);
            Assert.AreEqual(234, Convert.ToInt32(dbCommandWrapper.Parameters["@QtyOrdered"].Value));
        }

        [TestMethod]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenNonQueryDBCmdNoSPPresent()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "UpdateItem1";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            db.ExecuteNonQuery(dbCommandWrapper);
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        #endregion

        #region "Db Command, Transaction"

        [TestMethod]
        public void OriginalValueIsReturnedWhenDBCmdTransactionWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand= 188 where ItemId = 2 end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
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
        public void UpdatedValueIsReturnedWhenDBCmdTransactionWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 145 where ItemId = 1 end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Commit();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(145), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void OriginalValueIsReturnedWhenDBCmdTransactionWithRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 167 where ItemId = 2 end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
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
        public void CorrectValuesAreReturnedWhenDBCmdTransactionWithCommitAndRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 211 where ItemId = 1  end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Commit();
                transaction = connection.BeginTransaction();
                sqlCommand = "begin update Items set QtyInHand = 255 where ItemId = 2 end";
                dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Rollback();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(211), Convert.ToDouble(dr["QtyInHand"].ToString()));
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
        public void OriginalValueIsReturnedWhenDBCmdTransactionError()
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
                    string sqlCommand = "begin update Items set QtyInHand = 45 where ItemId = 'Test' end";
                    DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                    db.ExecuteNonQuery(dbCommandWrapper, transaction);
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
        public void OriginalValueIsReturnedWhenDBCmdTransactionSPNoCommit()
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
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
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
        public void UpdatedValueIsReturnedWhenDBCmdTransactionSPCommit()
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
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 43);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Commit();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(43), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void OriginalValueIsReturnedWhenDBCmdTransactionSPRollback()
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
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
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
        public void CorrectValuesAreReturnedWhenDBCmdTransactionSPCommitRollback()
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
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 74);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Commit();
                transaction = connection.BeginTransaction();
                sqlCommand = "ItemQuantityUpdateById";
                dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 72);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Rollback();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(74), Convert.ToDouble(dr["QtyInHand"].ToString()));
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

        #region "CommandType, CommandText"

        [TestMethod]
        public void RecordIsInsertedWhenNonQueryWithText()
        {
            bool isPresent = false;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string sqlText = "Insert into CustomersOrders Values(14, 'Lee', 1, 233)";
            db.ExecuteNonQuery(CommandType.Text, sqlText);
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select CustomerName from CustomersOrders where CustomerId = 14", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual("Lee", dr["CustomerName"].ToString());
                isPresent = true;
            }
            dr.Close();
            conn.Close();
            Assert.AreEqual(true, isPresent);
        }

        [TestMethod]
        public void RecordIsInsertedWhenNonQueryWithStoredProcedure()
        {
            bool isPresent = false;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "CustomerOrdersAdd";
            db.ExecuteNonQuery(CommandType.StoredProcedure, spName);
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from CustomersOrders where CustomerName = 'David'", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(3, Convert.ToInt32(dr["ItemId"].ToString()));
                Assert.AreEqual(555, Convert.ToInt32(dr["QtyOrdered"].ToString()));
                isPresent = true;
            }
            dr.Close();
            cmd = new SqlCommand("delete from CustomersOrders where CustomerName = 'David'", conn);
            dr = cmd.ExecuteReader();
            dr.Close();
            conn.Close();
            Assert.AreEqual(true, isPresent);
        }

        [TestMethod]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenStoredProcedureNameDoesNotExist()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "something";
            db.ExecuteNonQuery(CommandType.StoredProcedure, spName);
        }

        #endregion

        #region "Transaction, Command Type, Command Name"

        [TestMethod]
        public void RecordIsUpdatedWhenNonQueryWithCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.Text, "begin update Items set QtyInHand = 175 where ItemId = 1 end ");
                transaction.Commit();
            }
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId=1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(175), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void RecordIsNotUpdatedWhenNonQueryWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.Text, "begin update Items set QtyInHand = 67 where ItemId = 2 end ");
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
        public void RecordIsNotUpdatedWhenNonQueryWithRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.Text, "begin update Items set QtyInHand = 18 where ItemId = 2 select * from Items end ");
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
        public void RecordIsUpdatedWhenNonQueryWithCommitUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "ItemQuantityUpdateById");
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
        public void RecordIsNotUpdatedWhenNonQueryTransWithNoCommitUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "ItemQuantityIncrease");
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
        public void RecordIsNotUpdatedWhenNonQueryTransWithRollbackUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "ItemQuantityIncrease");
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

