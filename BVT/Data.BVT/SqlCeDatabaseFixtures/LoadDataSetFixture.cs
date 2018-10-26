// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlCeDatabaseFixtures
{
    /// <summary>
    /// Tests the LoadDataSet of the Database class
    /// </summary>
    [TestClass]
    public class LoadDataSetFixture : SqlCeDatabaseFixtureBase
    {
        private const string ItemsXMLfile = "Items.xml";
        private const string CustomersOrdersXMLfile = "CustomersOrders.xml";
        private const string CustomersOrdersXMLfile0 = "CustomersOrders0.xml";
        private const string ItemsUpdateXml = "updateItems.xml";

        public LoadDataSetFixture() : base("ConfigFiles.OlderTestsConfiguration.config") { }

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

        #region "Command,dataset,string table-sql,sp,multiple table"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingCommandAndTableName()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet ItemDataSet = new DataSet();
            string SqlCeCommand = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            int index;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            for (index = 0; index < count; index++)
            {
                Assert.AreEqual(table.Rows[index][0].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void AllDataIsLoadedWhenLoadDataSetUsingCommandAndTableName()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet ItemDataSet = new DataSet();
            string SqlCeCommand = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);

            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
            int count1 = table.Rows.Count;

            object actualResult = db.ExecuteScalar(CommandType.Text, "Select count(*) from Items");
            int count2 = Convert.ToInt32(actualResult.ToString().Trim());

            Assert.AreEqual(count2, count1);
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void MultipleTablesAreLoadedWhenLoadDataSetWithCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet ItemDataSet = new DataSet();
            string SqlCeCommand = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);

            string ItemsTable = "Items";
            //SqlCeCommand = "Select CustomerName from CustomersOrders order by CustomerID";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            SqlCeCommand = "Select CustomerName from CustomersOrders order by CustomerID";
            dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);

            string customertable = "CustomersOrders";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, customertable);
            DataTable table1 = ItemDataSet.Tables[ItemsTable];
            DataTable table2 = ItemDataSet.Tables[customertable];
            int index;
            DataSet da1 = new DataSet();
            DataSet da2 = new DataSet();
            da1.ReadXml(ItemsXMLfile);
            da2.ReadXml(CustomersOrdersXMLfile0);
            int count1 = da1.Tables[0].Rows.Count;
            int count2 = da2.Tables[0].Rows.Count;
            int countT1 = table1.Rows.Count;
            int countT2 = table2.Rows.Count;
            Assert.AreEqual(count1, countT1);
            Assert.AreEqual(count2, countT2);

            for (index = 0; index < count1; index++)
            {
                Assert.AreEqual(table1.Rows[index][0].ToString().Trim(), da1.Tables[0].Rows[index][0].ToString().Trim());
            }
            for (index = 0; index < count2; index++)
            {
                Assert.AreEqual(table2.Rows[index][0].ToString().Trim(), da2.Tables[0].Rows[index][0].ToString().Trim());
            }
        }

        #endregion

        #region "command,dataset,string table,transaction-commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        public void RecordsAreNotSavedwhenLoadDataSetRollbackWithCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet ItemDataSet1 = new DataSet();
            DataSet ItemDataSet2 = new DataSet();
            //string SqlCeCommand = "begin Insert into Items values(4,'infotech',77.0000,36,100) Select ItemDescription from Items order by ItemID end";
            string SqlCeCommand1 = "Insert into Items values(4,'infotech',77.0000,36,100)";
            string SqlCeCommand2 = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper1 = db.GetSqlStringCommand(SqlCeCommand1);
            DbCommand dbCommandWrapper2 = db.GetSqlStringCommand(SqlCeCommand2);
            string ItemsTable1 = "Junk";
            string ItemsTable2 = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper1, ItemDataSet1, ItemsTable1, transaction);
                db.LoadDataSet(dbCommandWrapper2, ItemDataSet2, ItemsTable2, transaction);
                transaction.Rollback();
                DataTable table2 = ItemDataSet2.Tables[ItemsTable2];
                connection.Close();
                int index;
                DataSet da = new DataSet();
                da.ReadXml(ItemsXMLfile);
                int count = da.Tables[0].Rows.Count;
                int count1 = table2.Rows.Count;
                for (index = 0; index < count; index++)
                {
                    string expectedString = da.Tables[0].Rows[index][0].ToString().Trim();
                    string actualString = table2.Rows[index][0].ToString().Trim();
                    Assert.AreEqual(expectedString, actualString);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        public void RecordsAreSavedwhenLoadDataSetCommitWithCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet CustomerDataSet = new DataSet();
            string SqlCeCommand1 = "Insert into CustomersOrders values(4,'Raj',1,100)";
            string SqlCeCommand2 = "Select CustomerName from CustomersOrders order by CustomerID";
            DbCommand dbCommandWrapper1 = db.GetSqlStringCommand(SqlCeCommand1);
            DbCommand dbCommandWrapper2 = db.GetSqlStringCommand(SqlCeCommand2);
            string CustomerTable = "CustomersOrders";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper1, CustomerDataSet, CustomerTable, transaction);
                db.LoadDataSet(dbCommandWrapper2, CustomerDataSet, CustomerTable, transaction);
                transaction.Commit();
                DataTable table = CustomerDataSet.Tables[CustomerTable];
                connection.Close();
                int index;
                DataSet da = new DataSet();
                da.ReadXml(CustomersOrdersXMLfile);
                int count = da.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual(table.Rows[index][0].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                }
            }
        }

        #endregion

        #region "commandwrapper,dataset,string[],transaction-rollback,commit"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        public void RecordsAreNotSavedWhenLoadDataSetRollbackWithCommandAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet itemDataSet1 = new DataSet();
            DataSet itemDataSet2 = new DataSet();
            // string SqlCeCommand = "begin update items set QtyRequired=111 where ItemId=1 Select * from Items order by ItemID  end";
            string SqlCeCommand1 = "insert into Items values(5,'hitech',79.0000,37,100)";
            string SqlCeCommand2 = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper1 = db.GetSqlStringCommand(SqlCeCommand1);
            DbCommand dbCommandWrapper2 = db.GetSqlStringCommand(SqlCeCommand2);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper1, itemDataSet1, new string[] { "Items" }, transaction);
                db.LoadDataSet(dbCommandWrapper2, itemDataSet2, new string[] { "Items" }, transaction);
                transaction.Rollback();
                connection.Close();
                DataSet da = new DataSet();
                da.ReadXml(ItemsXMLfile);
                DataSet itemDataSet3 = new DataSet();
                string SqlCeCommand3 = "Select * from Items order by ItemID";
                DbCommand dbCommandWrapper3 = db.GetSqlStringCommand(SqlCeCommand3);
                db.LoadDataSet(dbCommandWrapper3, itemDataSet3, new string[] { "Items" });
                int index = 0;
                Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet3.Tables[0].Rows.Count);
                for (index = 0; index < da.Tables[0].Rows.Count; index++)
                {
                    Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet3.Tables[0].Rows[index][1].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        public void RecordsAreSavedWhenLoadDataSetCommitWithCommandAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet itemDataSet1 = new DataSet();
            DataSet itemDataSet2 = new DataSet();
            string SqlCeCommand1 = "Update Items set ItemDescription='Notinfotech' where ItemID=1";
            string SqlCeCommand2 = "Select * from Items order by ItemID";
            DbCommand dbCommandWrapper1 = db.GetSqlStringCommand(SqlCeCommand1);
            DbCommand dbCommandWrapper2 = db.GetSqlStringCommand(SqlCeCommand2);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper1, itemDataSet1, new string[] { "Items" }, transaction);
                db.LoadDataSet(dbCommandWrapper2, itemDataSet2, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
                DataSet da = new DataSet();
                da.ReadXml(ItemsUpdateXml);
                int index = 0;
                Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet2.Tables[0].Rows.Count);
                for (index = 0; index < da.Tables[0].Rows.Count; index++)
                {
                    Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet2.Tables[0].Rows[index][1].ToString().Trim());
                }
            }
        }

        #endregion

        #region "commandtype,sql/sp,dataset,string[]"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingTextAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.Text, "Select ItemDescription from Items order by ItemID", itemDataSet, new string[] { "Items" });
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int i = 0;
            Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet.Tables[0].Rows.Count);
            for (i = 0; i < da.Tables[0].Rows.Count; i++)
            {
                Assert.AreEqual(da.Tables[0].Rows[i][0].ToString().Trim(), itemDataSet.Tables[0].Rows[i][0].ToString().Trim());
            }
        }

        #endregion"

        #region "transaction,commandtype,sp/sql,dataset,string--commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        public void RecordsAreSavedWhenLoadDataSetCommitWithTextAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "Update Items set ItemDescription='Notinfotech' where ItemID=1", itemDataSet, new string[] { "Items" });
                db.LoadDataSet(transaction, CommandType.Text, "Select * from Items order by ItemID", itemDataSet, new string[] { "Items" });
                transaction.Commit();
                connection.Close();
            }
            DataSet da = new DataSet();
            da.ReadXml(ItemsUpdateXml);
            int index = 0;
            Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet.Tables[0].Rows.Count);
            for (index = 0; index < da.Tables[0].Rows.Count; index++)
            {
                Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet.Tables[0].Rows[index][1].ToString().Trim());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\updateitems.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordsAreNotSavedWhenLoadDataSetCommitWithTextAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "Update Items set ItemDescription='Notinfotech' where ItemID=1", itemDataSet, new string[] { "Items" });
                db.LoadDataSet(transaction, CommandType.Text, "Select * from Items order by ItemID", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }

            int index = 0;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;
            SqlCeConnection conn = new SqlCeConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCeCommand cmd = new SqlCeCommand("select ItemDescription from Items ", conn);
            SqlCeDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }
            conn.Close();
        }
        #endregion
    }
}

