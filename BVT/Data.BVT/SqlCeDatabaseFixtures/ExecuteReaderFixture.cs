// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlCeDatabaseFixtures
{
    /// <summary>
    /// Tests ExecuteReader of the DataBase Class
    /// </summary>
    [TestClass]
    public class ExecuteReaderFixture : SqlCeDatabaseFixtureBase
    {
        private string itemsXMLfile;

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
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReadUsingExecuteReaderQuery()
        {
            //Expected Data
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            string SqlCeCommand = "Select ItemDescription from Items order by ItemId";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper))
            {
                while (dataReader.Read())
                {
                    Assert.AreEqual(dsExpectedData.Tables[0].Rows[count][0].ToString(), dataReader["ItemDescription"].ToString().Trim());
                    count++;
                }
            }
        }

        #endregion

        #region "Db Command, Transaction"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsUpdatedWhenExecuteReaderTransWithCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand;
                DbCommand dbCommandWrapper;

                SqlCeCommand = " update Items set QtyInHand = 45 where ItemId = 1";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }

                SqlCeCommand = "select * from Items";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);

                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Commit();
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(45), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsNotUpdatedWhenExecuteReaderTransWithRollbackUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand;
                DbCommand dbCommandWrapper;

                SqlCeCommand = " update Items set QtyInHand = 67 where ItemId = 2";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }

                SqlCeCommand = "select * from Items";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
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
        public void CorrectValuesAreReturnedWhenExecuteReaderTransWithRollbackAndCommitUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string SqlCeCommand;
                DbCommand dbCommandWrapper;

                SqlCeCommand = " update Items set QtyInHand = 111 where ItemId = 1";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }

                SqlCeCommand = " select * from Items";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Commit();
                transaction = connection.BeginTransaction();

                SqlCeCommand = "update Items set QtyInHand = 55 where ItemId = 2";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                SqlCeCommand = "select * from Items";
                dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
                transaction.Rollback();
            }

            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(111), Convert.ToDouble(dr["QtyInHand"].ToString()));
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
        public void TransactionIsRolledbackWhenExecuteReaderWithException()
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
                    string SqlCeCommand;
                    DbCommand dbCommandWrapper;

                    SqlCeCommand = "update Items set QtyInHand = 45 where ItemId = 2";
                    dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
                    using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }

                    SqlCeCommand = "select * from Items where ItemId = 'Test'";
                    dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);

                    using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper, transaction)) { }
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

        #region "ExecuteReader(Commandtype)"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsReturnedWhenUsingCommandtypeText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
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

        #endregion

        #region "Transaction, Command Type, Command Name"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void ValueIsUpdatedWhenUsingTransactionWithCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "update Items set QtyInHand = 75 where ItemId = 1")) { }
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "select * from Items ")) { }
                transaction.Commit();
            }
            SqlCeConnection conn = new SqlCeConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("Select QtyInHand from Items where ItemId=1", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(75), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void ValueIsNotUpdatedWhenUsingTransactionWithNoCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "update Items set QtyInHand = 67 where ItemId = 2")) { }
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "select * from Items ")) { }
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
        public void ValueIsNotUpdatedWhenUsingTransactionWithRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "update Items set QtyInHand = 18 where ItemId = 2 ")) { }
                using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "select * from Items ")) { }
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

