// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlDatabaseFixtures
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
            CreateItemsXmlFile(itemsXMLfile);
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExcepExceptionIsThrownWhenIncorrectDatabaseNameUsed()
        {
            Database db = DatabaseFactory.CreateDatabase("InvalidData");
        }

        #region "DB Command"

        [TestMethod]
        public void DataSetContainsCorrectItemsWhenUsingCommandQuery()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithNoParameters()
        {
            int index;
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithParameters()
        {
            int index;
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            DbCommand dbCommandWrapper = db.GetStoredProcCommand("ItemGetByPrice", new object[] { 90 });
            using (DataSet ds = db.ExecuteDataSet(dbCommandWrapper))
            {
                int count = ds.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual("Excel 2003", ds.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                }
            }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithOutParameter()
        {
            int index;
            SqlDatabase db = (SqlDatabase)DatabaseFactory.CreateDatabase("DataSQLTest");

            DbCommand dbCommandWrapper = db.GetStoredProcCommand("ItemPriceGetById");
            db.AddInParameter(dbCommandWrapper, "ID", SqlDbType.Int, 1);
            db.AddOutParameter(dbCommandWrapper, "Price", SqlDbType.Money, 50);
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
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithToUpdate()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        [ExpectedException(typeof(SqlException))]
        public void ExceptionIsThrownWhenStoredProcedureNotPresent()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            string sqlCommand = "UpdateItem1";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);

            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper)) { }
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        #endregion

        #region "Db Command, Transaction"

        [TestMethod]
        public void DataSetRecordNotUpdatedWhenTransNotCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            DbConnection connection;
            using (connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand= 88 where ItemId = 2  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
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
            Assert.AreEqual(ConnectionState.Closed, connection.State);
        }

        [TestMethod]
        public void DataSetRecordNotUpdatedWhenTransCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            DbConnection connection;
            using (connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 45 where ItemId = 1  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
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
            Assert.AreEqual(ConnectionState.Closed, connection.State);
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
        public void DataSetRecordNotUpdatedWhenTransRolledback()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            DbConnection connection;
            using (connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "begin update Items set QtyInHand = 67 where ItemId = 2  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
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
            Assert.AreEqual(ConnectionState.Closed, connection.State);
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
        public void DataSetRecordUpdatedWhenCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            DbConnection connection;
            using (connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "begin update Items set QtyInHand = 111 where ItemId = 1  select * from Items end";
                DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);

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
            Assert.AreEqual(ConnectionState.Closed, connection.State);
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
        public void DataSetChangesRolledBackWhenError()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            DbTransaction transaction = null;
            DbConnection connection = null;
            try
            {
                using (connection = db.CreateConnection())
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    string sqlCommand = "begin update Items set QtyInHand = 45 where ItemId = 2  select * from Items where ItemId = 'Test' end";
                    DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
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
            Assert.AreEqual(ConnectionState.Closed, connection.State);
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
        public void DataSetChangesNotCommittedWhenUsingStoredProcedure()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 67);
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
        public void DataSetChangesCommittedWhenUsingStoredProcedure()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 1);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 88);
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
        public void DataSetChangesNotCommittedWhenUsingCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 2);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 65);
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
        public void DataSetChangesCommittedWhenUsingStoredProcedure2()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "ItemQuantityUpdateById";
                DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
                CreateCommandParameter(db, dbCommandWrapper, "@ItemId", DbType.Int16, ParameterDirection.Input, 1);
                CreateCommandParameter(db, dbCommandWrapper, "@Quantity", DbType.Int16, ParameterDirection.Input, 44);
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

        #region "Transaction, Stored Procedure, Parameters"

        [TestMethod]
        public void DataIsUpdatedWithinTransactionUsingStoredProcButRolledBackWhenNotCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, "ItemQuantityIncrease"))
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
                Assert.AreEqual(Convert.ToDouble(100), Convert.ToDouble(dr["QtyRequired"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void DataIsUpdatedWhenUsingStoredProcAndCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, "OrderUpdate")) { }
                transaction.Commit();
            }

            SqlConnection conn = new SqlConnection();

            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("Select QtyOrdered from CustomersOrders", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(150, Convert.ToInt32(dr["QtyOrdered"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void DataIsUpdatedWithinTransactionUsingStoredProcButRolledBackWhenRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, "ItemQuantityIncrease"))
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

        [TestMethod]
        public void DataIsUpdatedWithinTransactionUsingStoredProcWithParametersButRolledBackWhenNotCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, "ItemQuantityUpdateById", new object[] { "2", "88" }))
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
            SqlCommand cmd = new SqlCommand("Select QtyInHand from Items where ItemId = 2", conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(95, Convert.ToInt32(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void DataIsUpdatedWhenUsingStoredProcWithParametersAndCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, "ItemQuantityUpdateById", new object[] { "1", "53" }))
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
                Assert.AreEqual(53, Convert.ToInt32(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        [TestMethod]
        public void DataIsUpdatedWithinTransactionUsingStoredProcWithParametersButRolledBackWhenRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                using (DataSet dsActualResult = db.ExecuteDataSet(transaction, "ItemQuantityUpdateById", new object[] { "2", "99" }))
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
                Assert.AreEqual(95, Convert.ToInt32(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            conn.Close();
        }

        #endregion

        #region "ExecuteDataSet(Commandtype)"

        [TestMethod]
        public void RecordsAreReturnedWhenUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");

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
        public void RecordsAreReturnedWhenUsingExecuteDataSetWithStoredProc()
        {
            //Expected Data
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;

            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");

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
        [ExpectedException(typeof(SqlException))]
        public void ExceptionIsThrownWhenUsingExecuteDataSetWithNonExistentStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
            StringBuilder readerDataActual = new StringBuilder();
            string spName = "something";
            using (DataSet dsActualResult = db.ExecuteDataSet(CommandType.StoredProcedure, spName)) { }
        }

        #endregion

        #region "Transaction, Command Type, Command Name"

        [TestMethod]
        public void RecordsAreReturnedWhenUpdatedInTransactionUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void RecordsAreNotUpdatedWhenTransactionNotCommittedUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void RecordsAreNotUpdatedWhenRollbackUsingExecuteDataSetWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void RecordIsUpdatedWhenCommittedUsingExecuteDataSetWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void RecordIsNotUpdatedWhenNotCommittedUsingExecuteDataSetWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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
        public void RecordIsNotUpdatedWhenRollbackUsingExecuteDataSetWithStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("DataSQLTest");
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

        private void CreateItemsXmlFile(string FileName)
        {
            StreamWriter SW;
            SW = File.CreateText(Path.GetFullPath(FileName));
            SW.WriteLine("<Items>");
            SW.WriteLine("<ItemDescription>Digital Image Pro</ItemDescription>");
            SW.WriteLine("<ItemDescription>Excel 2003</ItemDescription>");
            SW.WriteLine("<ItemDescription>Infopath</ItemDescription>");
            SW.WriteLine("</Items>");
            SW.Close();
        }

        private void CreateProductsXmlFile(string FileName)
        {
            StreamWriter SW;
            SW = File.CreateText(Path.GetFullPath(FileName));
            SW.WriteLine("<Products>");
            SW.WriteLine("<ProductID>5</ProductID>");
            SW.WriteLine("<ProductName>Chef Anton's Gumbo Mix</ProductName>");
            SW.WriteLine("<UnitPrice>$21.35</UnitPrice>");
            SW.WriteLine("</Products>");
            SW.Close();
        }
    }
}

