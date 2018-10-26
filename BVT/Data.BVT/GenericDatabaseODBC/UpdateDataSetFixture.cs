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
    /// Tests the UpdatesetMethod of the Database class
    /// </summary>
    [TestClass]
    public class UpdateDatasetTestFixture : EntLibFixtureBase
    {
        private string ItemsXMLfile;
        private string ItemsUpdateXml;
        private string ItemLDSctspparamXML;

        public UpdateDatasetTestFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        public override void Initialize()
        {
            base.Initialize();

            ItemsXMLfile = "Items.xml";
            ItemsUpdateXml = "updateItems.xml";
            ItemLDSctspparamXML = "LDSctspparam.xml";
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #region "UpdateDatasetWrapperDatasetStringSqlTest-sql,transCommit,transRollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            // Establish our Insert, Delete, and Update commands
            // DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);

            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("DBConcurrencyException should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSql()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSql()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Digital Image Pro";
            //Delete a product
            table.Rows[3].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlAndTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //add a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                ItemDataSet.RejectChanges();
                connection.Close();
            }
            int index = 0;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);

            StringBuilder readerDataExpected = new StringBuilder();
            var cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }
        }

        #endregion

        #region "UpdateDatasetWrapperDatasetStringSpTest-sp,transcommit"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //add a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("DBConcurrencyException should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            // Submit the DataSet, capturing the number of rows that were affected
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(sqlCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Digital Image Pro";
            //Delete a product
            table.Rows[3].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        #endregion

        #region "UpdateDatasetWrapperDatasetStringTrans-sql,commit&rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlAndTransactionCommitTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "begin Insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlAndTransactionCommitTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "begin Insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlAndTransactionCommitContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "begin Insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // insertCommandWrapper.AddInParameter("@QtyRequired", DbType.Int32, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "begin Insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlAndTransactionRollback2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "begin Insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            // deleteCommandWrapper.AddInParameter("@ItemID", DbType.Int32, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        #endregion

        #region "UpdateDatasetWrapperDatasetStringTrans-sp,commit&rollback

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionCommitTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "AddrowSelect";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionCommitTransactionalBehaviouralt()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "AddrowSelect";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcandTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "AddrowSelect";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "AddrowSelect";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "AddrowSelect";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = ItemDataSet.Tables[ItemsTable];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(ItemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        #endregion

        #region "UpdateDatasetWrapperDataset-behavioural,commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionCommit2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }



        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndRollbackTransactionBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcRollbackTransactionAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                // insertCommandWrapper.AddInParameter("@QtyRequired", DbType.Int32, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetCmdStoredProcAndRollbackTransactionTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            var cmd = new OdbcCommand("select ItemDescription from Items ");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }
        }

        #endregion

        #region "UpdateDatasetWrapperDatasetCTransaction-behavioural,commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithSqlCmdBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            // insertCommandWrapper.AddInParameter("@ItemID", DbType.Int32, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithSqlCmdBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            //  updateCommandWrapper.AddInParameter("@ItemDescription", DbType.String, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithSqlCmdBehaviouralStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithSqlCmdBehaviouralContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithSqlCmdAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithSqlCmdAndTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin insert into Items values(4, 'Infotech', 88, 35, 100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            var cmd = new OdbcCommand("select ItemDescription from Items ");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }
            dr.Close();
            var delcmd = new OdbcCommand("Delete from Items where ItemID=4");
            db.ExecuteNonQuery(cmd);
        }

        #endregion

        #region "Updatedataset commandtype(sql),string,dataset,,object[]-behaviuor,commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdtypeSqlSqlBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "begin Insert into Items values(4,'infotech',88,35,100)Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "begin Insert into Items values(4,'infotech',88,35,100)Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlBehaviouralContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "begin Insert into Items values(4,'infotech',88,35,100)Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlBehaviouralStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100)Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlAndTransactionCommit2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "begin Insert into Items values(4,'infotech',88,35,100)Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlAndTransactionRollback3()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100)Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            var cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }

            var delcmd = new OdbcCommand("Delete from Items where ItemID=4");
            db.ExecuteNonQuery(delcmd);
        }

        #endregion

        #region "updatedataset-cmdtype(sp),string,dataset,behaviour,commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSpBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralTransactional2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralStandard2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcBehaviouralContinue2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionCommit3()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionRollback2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            var cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }

            var delcmd = new OdbcCommand("Delete from Items where ItemID=4");
            db.ExecuteNonQuery(delcmd);

        }

        #endregion

        #region "updatedataset-trans(commit),cmdtype(sql),string,dataset,-behaviuor,commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlCommitBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlCommitBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlCommitBehaviouralStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands            
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlCommitBehaviouralContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlCommitAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlCommitAndTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            var cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }


            OdbcCommand delcmd = new OdbcCommand("Delete from Items where ItemID=4");
            db.ExecuteNonQuery(delcmd);

        }

        #endregion

        #region "Updatedataset-trans(roll),cmdtype(sql),string,dataset,-behaviour,rollback,commit"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlRollbackBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlRollbackBehaviouralTransactional2()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlRollbackBehaviouralStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlRollbackBehaviouralContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlRollbackAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdSqlRollbackAndTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Insert into Items values(4,'Infotech',88,35,100) Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            OdbcCommand cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }

        }

        #endregion

        #region "Updatedataset-trans(comm),cmdtype(sp),string,dataset,-behaviour,rollback,commit"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcCommitBehaviourTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcCommitBehaviourTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
                Assert.Fail("Concurrency Exception should have been raised.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcCommitBehaviourStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcCommitBehaviourContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcCommitTransCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcCommitTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[3].Delete();
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            int index = 0;
            OdbcCommand cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }
            var delcmd = new OdbcCommand("Delete from Items where ItemID=4");
            db.ExecuteNonQuery(delcmd);

        }

        #endregion

        #region "Updatedataset-trans(roll),cmdtype(sp),string,dataset,-behaviour,rollback,commit"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSpRollbackBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
            CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, UpdateBehavior.Transactional);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcRollbackBehaviouralTransactional()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemID"] = 4;
            // Establish our Insert, Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Transactional);
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcRollbackBehaviouralStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //add a row
            table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            table.Rows.Add(new object[] { 4, "Hello", 8, 99, 100 });
            table.Rows.Add(new object[] { 5, "fifth", 88, 99, 100 });
            // Establish our Insert commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            try
            {
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, null, null, UpdateBehavior.Standard);
                Assert.Fail("Primary Key Violation should have been thrown.");
            }
            catch (Exception) { }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcRollbackBehaviouralContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            //Insert a row
            DataRow addedRow = table.Rows.Add(new object[] { 4, "Infotech", 88, 35, 100 });
            // Modify an existing product
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            // Establish our Insert, Delete, and Update commands
            DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
            CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
            CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
            DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
            CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
            CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
            // Submit the DataSet, capturing the number of rows that were affected
            int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, null, UpdateBehavior.Continue);
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreSavedWhenUpdateDataSetWithmdStoredProcRollbackTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Commit();
                connection.Close();
            }
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][1].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void ChangesAreNotSavedWhenUpdateDataSetWithUpdCmdSpRollbackTransRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.StoredProcedure, "AddrowSelect", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }
            DataTable table = itemDataSet.Tables["Items"];
            DataRow addedRow = table.Rows.Add(new object[] { 5, "Infotech", 88, 35, 100 });
            table.Rows[0]["ItemDescription"] = "Notinfotech";
            //Delete a product
            table.Rows[4].Delete();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                // Establish our Insert, Delete, and Update commands
                DbCommand insertCommandWrapper = db.GetStoredProcCommand("{ Call AddItem(?,?,?,?,?) }");
                CreateCommandParameter(db, insertCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@Price", DbType.Currency, ParameterDirection.Input, "Price", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyinHand", DbType.Int32, ParameterDirection.Input, "QtyinHand", DataRowVersion.Current);
                CreateCommandParameter(db, insertCommandWrapper, "@QtyRequired", DbType.Int32, ParameterDirection.Input, "QtyRequired", DataRowVersion.Current);
                DbCommand deleteCommandWrapper = db.GetStoredProcCommand("{ Call deleteitem(?) }");
                CreateCommandParameter(db, deleteCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                DbCommand updateCommandWrapper = db.GetStoredProcCommand("{ Call UpdateItem(?,?) }");
                CreateCommandParameter(db, updateCommandWrapper, "@ItemID", DbType.Int32, ParameterDirection.Input, "ItemID", DataRowVersion.Current);
                CreateCommandParameter(db, updateCommandWrapper, "@ItemDescription", DbType.String, ParameterDirection.Input, "ItemDescription", DataRowVersion.Current);
                // Submit the DataSet, capturing the number of rows that were affected
                int rowsAffected = db.UpdateDataSet(itemDataSet, "Items", insertCommandWrapper, updateCommandWrapper, deleteCommandWrapper, transaction);
                transaction.Rollback();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            int index = 0;

            OdbcCommand cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }
        }

        #endregion
    }
}

