// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Practices.EnterpriseLibrary.Data.SqlCe;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlCeDatabaseFixtures
{
    /// <summary>
    /// Tests ExecuteNonQuery of the Database class
    /// </summary>
    [TestClass]
    public class ExecuteNonQueryFixture : SqlCeDatabaseFixtureBase
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
        [DeploymentItem(@"TestDb.sdf")]
        public void ResultIsReadWhenExecuteNonQueryUsingDBCmd()
        {
            bool isPresent = false;
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            string SqlCeCommand = "Insert into CustomersOrders values(13, 'John',3, 214)";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
            db.ExecuteNonQuery(dbCommandWrapper);
            SqlCeConnection conn = new SqlCeConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("select * from CustomersOrders where CustomerName = 'John'", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(3, Convert.ToInt32(dr["ItemId"].ToString()));
                Assert.AreEqual(214, Convert.ToInt32(dr["QtyOrdered"].ToString()));
                isPresent = true;
            }

            dr.Close();
            Assert.AreEqual(true, isPresent);
        }

        #endregion

        #region "Db Command, Transaction"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void OriginalValueIsReturnedWhenDBCmdTransactionWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand = "update Items set QtyInHand= 188 where ItemId = 2";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
            }
            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("select * from Items where ItemId = 2", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void UpdatedValueIsReturnedWhenDBCmdTransactionWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand = "update Items set QtyInHand = 145 where ItemId = 1";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Commit();
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(145), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void OriginalValueIsReturnedWhenDBCmdTransactionWithRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand = "update Items set QtyInHand = 167 where ItemId = 2";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Rollback();
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void CorrectValuesAreReturnedWhenDBCmdTransactionWithCommitAndRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand = "update Items set QtyInHand = 211 where ItemId = 1";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Commit();
                transaction = connection.BeginTransaction();
                SqlCeCommand = "update Items set QtyInHand = 255 where ItemId = 2";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                db.ExecuteNonQuery(dbCommandWrapper, transaction);
                transaction.Rollback();
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(211), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 2", conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void OriginalValueIsReturnedWhenDBCmdTransactionError()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            DbTransaction transaction = null;
            try
            {
                using (DbConnection connection = db.CreateConnection())
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    string SqlCeCommand = "update Items set QtyInHand = 45 where ItemId = 'Test'";
                    DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);

                    db.ExecuteNonQuery(dbCommandWrapper, transaction);

                    transaction.Commit();
                }
            }
            catch { }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
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
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsInsertedWhenNonQueryWithText()
        {
            string connectionString;
            string filename = "TestDb.sdf";

            bool isPresent = false;

            filename = System.IO.Path.Combine(Environment.CurrentDirectory, filename);
            connectionString = "Data Source='{0}'";
            connectionString = String.Format(connectionString, filename);
            Database db = new SqlCeDatabase(connectionString);

            StringBuilder readerDataActual = new StringBuilder();
            string sqlText = "Insert into CustomersOrders Values(14, 'Lee', 1, 233)";
            db.ExecuteNonQuery(CommandType.Text, sqlText);
            SqlCeConnection conn = new SqlCeConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select CustomerName from CustomersOrders where CustomerId = 14", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual("Lee", dr["CustomerName"].ToString());
                isPresent = true;
            }
            dr.Close();
            conn.Close();
            Assert.AreEqual(true, isPresent);
        }

        #endregion

        #region "Transaction, Command Type, Command Name"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsUpdatedWhenNonQueryWithCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.Text, "update Items set QtyInHand = 175 where ItemId = 1");
                transaction.Commit();
            }
            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId=1", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(175), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsNotUpdatedWhenNonQueryWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.Text, "update Items set QtyInHand = 67 where ItemId = 2");
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId=2", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsNotUpdatedWhenNonQueryWithRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteNonQuery(transaction, CommandType.Text, "update Items set QtyInHand = 18 where ItemId = 2");
                db.ExecuteNonQuery(transaction, CommandType.Text, "select * from Items");
                transaction.Rollback();
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId=2", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        #endregion
    }
}

