// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.GenericDatabaseODBC
{
    /// <summary>
    /// Tests ExecuteDataSet Method of the DataSet Object
    /// </summary>
    [TestClass]
    public class ExecuteDataSetFixture : EntLibFixtureBase
    {
        private string itemsXMLfile;
        private DataSet dsExpectedResult = new DataSet();

        public ExecuteDataSetFixture()
            : base(@"ConfigFiles.OlderTestsConfiguration.config")
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

        #region "DB Command"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetContainsCorrectItemsWhenUsingCommandQuery()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            int index;
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            int index;

            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");

            DbCommand dbCommandWrapper = db.GetStoredProcCommand("{ Call ItemPriceGetById(?,?)}");
            CreateCommandParameter(db, dbCommandWrapper, "@Id", DbType.Int32, ParameterDirection.Input, 1);
            //  dbCommandWrapper.AddInParameter("@ID", DbType.Int32, 1);
            CreateCommandParameter(db, dbCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Output, 50);
            //  dbCommandWrapper.AddOutParameter("@Price", DbType.Currency,50);

            using (DataSet dsActualResult = db.ExecuteDataSet(dbCommandWrapper))
            {
                int count = dsActualResult.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual(dsExpectedResult.Tables[0].Rows[index][0].ToString().Trim(), dsActualResult.Tables[0].Rows[index]["ItemDescription"].ToString().Trim());
                }
            }

            Assert.AreEqual(Convert.ToDouble(39), Convert.ToDouble(dbCommandWrapper.Parameters["@Price"].Value));
            Assert.AreEqual(ConnectionState.Closed, dbCommandWrapper.Connection.State);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetContainsCorrectItemsWhenUsingStoredProcedureWithToUpdate()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            string sqlCommand = "{ Call ItemQuantityUpdateById(?,?) }";
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
        [ExpectedException(typeof(OdbcException))]
        public void ExceptionIsThrownWhenStoredProcedureNotPresent()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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


            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 1");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(45, (int)dr["QtyInHand"]);
            }
            dr.Close();

        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetRecordNotUpdatedWhenTransRolledback()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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



            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();

        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataSetRecordUpdatedWhenCommitted()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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


            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 1");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(111), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
            OdbcCommand cmd1 = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr1 = db.ExecuteReader(cmd1);
            while (dr1.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr1["QtyInHand"].ToString()));
            }
            dr1.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotCommittedWhenErrorUsingExecuteDataSetCommand()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotCommittedWhenNoCommitUsingExecuteDataSetStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "{ Call ItemQuantityUpdateById(?,?) }";
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

            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsCommittedWhenUsingExecuteDataSetStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                string sqlCommand = "{ Call ItemQuantityUpdateById(?,?) }";
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
            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 1");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(88), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotCommittedWhenUsingExecuteDataSetStoredProcWithRollback()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "{ Call ItemQuantityUpdateById(?,?) }";
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
            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void AppropriateRecordsAreCommittedWhenUsingExecuteDataSetStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            StringBuilder readerData = new StringBuilder();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                string sqlCommand = "{ Call ItemQuantityUpdateById(?,?) }";
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
                sqlCommand = "{ Call ItemQuantityUpdateById(?,?) }";
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
            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 1");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {

                Assert.AreEqual(Convert.ToDouble(44), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();

            OdbcCommand cmd1 = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr1 = db.ExecuteReader(cmd1);
            while (dr1.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr1["QtyInHand"].ToString()));
            }
            dr1.Close();
        }

        #endregion

        #region "ExecuteDataSet(Commandtype)"

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreReturnedWhenUsingExecuteDataSetWithText()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");

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
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            //Expected Data
            DataSet dsExpectedData = new DataSet();
            dsExpectedData.ReadXml(itemsXMLfile);
            int count = 0;

            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");

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
        [ExpectedException(typeof(OdbcException))]
        public void ExceptionIsThrownWhenUsingExecuteDataSetWithNonExistentStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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


            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 1");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(175), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreNotUpdatedWhenTransactionNotCommittedUsingExecuteDataSetWithText()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreNotUpdatedWhenRollbackUsingExecuteDataSetWithText()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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
            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 2");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(Convert.ToDouble(95), Convert.ToDouble(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsUpdatedWhenCommittedUsingExecuteDataSetWithStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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

            OdbcCommand cmd = new OdbcCommand("Select QtyInHand from Items where ItemId = 1");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(50, Convert.ToInt32(dr["QtyInHand"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenNotCommittedUsingExecuteDataSetWithStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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


            OdbcCommand cmd = new OdbcCommand("Select QtyRequired from Items");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(100, Convert.ToInt32(dr["QtyRequired"].ToString()));
            }
            dr.Close();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotUpdatedWhenRollbackUsingExecuteDataSetWithStoredProc()
        {
            dsExpectedResult = new DataSet();
            dsExpectedResult.ReadXml(itemsXMLfile);
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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

            OdbcCommand cmd = new OdbcCommand("Select QtyRequired from Items");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(100, Convert.ToInt32(dr["QtyRequired"].ToString()));
            }
            dr.Close();
        }

        #endregion
    }
}

