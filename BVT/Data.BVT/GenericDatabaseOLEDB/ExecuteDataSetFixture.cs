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
    /// Tests ExecuteDataSet Method of the DataSet Object
    /// </summary>
    [TestClass]
    public class ExecuteDataSetFixture : EntLibFixtureBase
    {
        private String itemsXMLfile;
        private DataSet dsExpectedResult = new DataSet();

        public ExecuteDataSetFixture()
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
            dsExpectedResult.Clear();
            dsExpectedResult.Dispose();

            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #region "DB Command"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetContainsCorrectItemsWhenUsingCommandQuery()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            int index;
            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper))
            {
                int count = dsActualResult.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index][0].ToString().Trim());
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithNoParameters()
        {
            int index;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper))
            {
                int count = dsActualResult.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithOutParameter()
        {
            int index;

            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");

            DbCommand dbCommandWrapper = db.GetStoredProcCommand("ItemPriceGetById");
            CreateCommandParameter(db, dbCommandWrapper, "@ID", DbType.Int32, ParameterDirection.Input, 1);
            CreateCommandParameter(db, dbCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Output, 50);
            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper))
            {
                int count = dsActualResult.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                }
            }

            Assert.AreEqual(Convert.ToDouble(38.95), Convert.ToDouble(dbCommandWrapper.Parameters["@Price"].Value));
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithToUpdate()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "ItemQuantityUpdateById";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 1);
            CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 55);
            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper))
            {
                int count = dsActualResult.Tables[0].Rows.Count;
                for (int index = 0; index < count; index++)
                {
                    if (Convert.ToInt16(dsActualResult.Tables[0].Rows[index]["ItemId"]) == 1)
                    {
                        Assert.AreEqual(55, Convert.ToInt32(dsActualResult.Tables[0].Rows[index]["QtyInHand"]));
                    }
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenStoredProcedureNotPresent()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            string sqlCommand = "UpdateItem1";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper)) { }

            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        #endregion

        #region "Db Command, Transaction"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetRecordNotUpdatedWhenTransNotCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand= 88 where ItemId = 2  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Transaction = transaction;
                dbCommandWrapper.Connection = connection;
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void DataSetRecordNotUpdatedWhenTransCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 45 where ItemId = 1  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                dbCommandWrapper.Connection = connection;
                dbCommandWrapper.Transaction = transaction;
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;

                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
                transaction.Commit();
            }

            SqlConnection conn = new SqlConnection();
            
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 1", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(45, (int)dr["QtyInHand"]);
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetRecordNotUpdatedWhenTransRolledback()
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
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void DataSetRecordUpdatedWhenCommitted()
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

                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;

                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
                transaction.Commit();
                transaction = connection.BeginTransaction();
                sqlCommand = "begin update Items set QtyInHand = 55 where ItemId = 2 select * from Items end";
                dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void DataSetChangesRolledBackWhenError()
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
                    dbCommandWrapper.Transaction = transaction;
                    dbCommandWrapper.Connection = connection;
                    using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                    {
                        int count = dsActualResult.Tables[0].Rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Assert.AreEqual(dsExpectedResult.Tables[0].Rows[i][0].ToString().Trim(), dsActualResult.Tables[0].Rows[i]["ItemDescription"].ToString().Trim());
                        }
                    }
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
        public void DataSetChangesNotCommittedWhenUsingStoredProcedure()
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
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[i][0].ToString().Trim(), dsActualResult.Tables[0].Rows[i]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void DataSetChangesCommittedWhenUsingStoredProcedure()
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
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[i][0].ToString().Trim(), dsActualResult.Tables[0].Rows[i]["ItemDescription"].ToString().Trim());
                    }
                }

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
        public void DataSetChangesNotCommittedWhenUsingCommand()
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
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }

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
        public void DataSetChangesCommittedWhenUsingStoredProcedure2()
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
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;

                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
                transaction.Commit();
                transaction = connection.BeginTransaction();
                sqlCommand = "ItemQuantityUpdateById";
                dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 72);
                using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper, transaction))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[i][0].ToString().Trim(), dsActualResult.Tables[0].Rows[i]["ItemDescription"].ToString().Trim());
                    }
                }
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

        #region "ExecuteDataSet(Commandtype)"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReturnedWhenUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string sqlText = "select ItemDescription From Items ";
            using (DataSet dsActualResult = db.ExecuteDataSet(CommandType.Text, sqlText))
            {
                int count = dsActualResult.Tables[0].Rows.Count;
                for (int index = 0; index < count; index++)
                {
                    Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReturnedWhenUsingExecuteDataSetWithStoredProc()
        {
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "ItemsGet";
            using (DataSet dsActualResult = db.ExecuteDataSet(CommandType.StoredProcedure, spName))
            {
                count = dsActualResult.Tables[0].Rows.Count;
                for (int index = 0; index < count; index++)
                {
                    Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [ExpectedException(typeof(OleDbException))]
        public void ExceptionIsThrownWhenUsingExecuteDataSetWithNonExistentStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "something";
            using (DataSet dsActualResult = db.ExecuteDataSet(CommandType.StoredProcedure, spName)) { }
        }

        #endregion

        #region "Transaction, Command Type, Command Name"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReturnedWhenUpdatedInTransactionUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, CommandType.Text, "begin update Items set QtyInHand = 175 where ItemId = 1 select * from Items order by ItemId end "))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }

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
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreNotUpdatedWhenTransactionNotCommittedUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, CommandType.Text, "begin update Items set QtyInHand = 67 where ItemId = 2 select * from Items order by ItemId end "))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void RecordsAreNotUpdatedWhenRollbackUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, CommandType.Text, "begin update Items set QtyInHand = 18 where ItemId = 2 select * from Items end "))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[i][0].ToString().Trim(), dsActualResult.Tables[0].Rows[i]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void RecordIsUpdatedWhenCommittedUsingExecuteDataSetWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, CommandType.StoredProcedure, "ItemQuantityUpdateById"))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void RecordIsNotUpdatedWhenNotCommittedUsingExecuteDataSetWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, CommandType.StoredProcedure, "ItemQuantityIncrease"))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }
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
        public void RecordIsNotUpdatedWhenRollbackUsingExecuteDataSetWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, CommandType.StoredProcedure, "ItemQuantityIncrease"))
                {
                    int count = dsActualResult.Tables[0].Rows.Count;
                    for (int index = 0; index < count; index++)
                    {
                        Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                    }
                }

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

