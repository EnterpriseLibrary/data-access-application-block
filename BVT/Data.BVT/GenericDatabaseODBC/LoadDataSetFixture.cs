// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.GenericDatabaseODBC
{
    /// <summary>
    /// Tests the LoadDataSet of the Database class
    /// </summary>
    [TestClass]
    public class LoadDataSetFixture : EntLibFixtureBase
    {
        string ItemsXMLfile;
        string CustomersOrdersXMLfile;
        string CustomersOrdersXMLfile0;
        string ItemsUpdateXml;
        string ItemLDSctspparamXML;

        public LoadDataSetFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        public override void Initialize()
        {
            base.Initialize();

            ItemsXMLfile = "Items.xml";
            CustomersOrdersXMLfile = "CustomersOrders.xml";
            CustomersOrdersXMLfile0 = "CustomersOrders0.xml";
            ItemsUpdateXml = "updateItems.xml";
            ItemLDSctspparamXML = "LDSctspparam.xml";
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }


        #region "Command,dataset,string table-sql,sp,multiple table"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingCommandAndTableName()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
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
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingStoredProcAndTableName()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            string ItemsTable = "Items";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            DataTable table = ItemDataSet.Tables[ItemsTable];
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
        public void MultipleTablesAreLoadedWhenLoadDataSetWithCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "Select ItemDescription from Items order by ItemID";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            //sqlCommand = "Select CustomerName from CustomersOrders order by CustomerID";
            db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable);
            sqlCommand = "Select CustomerName from CustomersOrders order by CustomerID";
            dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
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
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreNotSavedwhenLoadDataSetRollbackWithCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet ItemDataSet = new DataSet();
            string sqlCommand = "begin Insert into Items values(4,'infotech',77.0000,36,100) Select ItemDescription from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string ItemsTable = "Items";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, ItemDataSet, ItemsTable, transaction);
                transaction.Rollback();
                DataTable table = ItemDataSet.Tables[ItemsTable];
                connection.Close();
                int index;
                DataSet da = new DataSet();
                da.ReadXml(ItemsXMLfile);
                int count = da.Tables[0].Rows.Count;
                for (index = 0; index < count; index++)
                {
                    Assert.AreEqual(table.Rows[index][0].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreSavedwhenLoadDataSetCommitWithCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet CustomerDataSet = new DataSet();
            string sqlCommand = "begin Insert into CustomersOrders values(4,'Raj',1,100) Select CustomerName from CustomersOrders order by CustomerID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            string CustomerTable = "CustomersOrders";
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, CustomerDataSet, CustomerTable, transaction);
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

        #region "command,dataset,string[]"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingStoredProcCommandAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string spName = "ItemsGet";
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(spName);
            db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" });
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            // int count = da.Tables[0].Rows.Count;
            int index = 0;
            Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet.Tables[0].Rows.Count);
            for (index = 0; index < da.Tables[0].Rows.Count; index++)
            {
                Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet.Tables[0].Rows[index][1].ToString().Trim());
            }
        }

        #endregion

        #region "command,dataset,string[],transaction-rollback,commit"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreNotSavedWhenLoadDataSetRollbackWithCommandAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            // string sqlCommand = "begin update items set QtyRequired=111 where ItemId=1 Select * from Items order by ItemID  end";
            string sqlCommand = "begin insert into Items values(5,'hitech',79.0000,37,100) Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Rollback();
                connection.Close();
                DataSet da = new DataSet();
                da.ReadXml(ItemsXMLfile);
                DataSet itemDataSet1 = new DataSet();
                sqlCommand = "begin  Select * from Items order by ItemID end";
                DbCommand dbCommandWrapper1 = db.GetSqlStringCommand(sqlCommand);
                db.LoadDataSet(dbCommandWrapper1, itemDataSet1, new string[] { "Items" });
                int index = 0;
                Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet1.Tables[0].Rows.Count);
                for (index = 0; index < da.Tables[0].Rows.Count; index++)
                {
                    Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet.Tables[0].Rows[index][1].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreSavedWhenLoadDataSetCommitWithCommandAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            string sqlCommand = "begin Update Items set ItemDescription='Notinfotech' where ItemID=1 Select * from Items order by ItemID end";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(dbCommandWrapper, itemDataSet, new string[] { "Items" }, transaction);
                transaction.Commit();
                connection.Close();
                DataSet da = new DataSet();
                da.ReadXml(ItemsUpdateXml);
                int index = 0;
                Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet.Tables[0].Rows.Count);
                for (index = 0; index < da.Tables[0].Rows.Count; index++)
                {
                    Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet.Tables[0].Rows[index][1].ToString().Trim());
                }
            }
        }

        #endregion

        #region "commandtype,sql/sp,dataset,string[]"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingTextAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
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

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingStoredProcAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "ItemsGet", itemDataSet, new string[] { "Items" });
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int index = 0;
            Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet.Tables[0].Rows.Count);
            for (index = 0; index < da.Tables[0].Rows.Count; index++)
            {
                Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet.Tables[0].Rows[index][1].ToString().Trim());
            }
        }

        #endregion"

        #region "transaction,commandtype,sp/sql,dataset,string--commit,rollback"

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreSavedWhenLoadDataSetCommitWithTextAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Update Items set ItemDescription='Notinfotech' where ItemID=1 Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
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
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreNotSavedWhenLoadDataSetCommitWithTextAndTableNameArray()
        {
            Database db = DatabaseFactory.CreateDatabase("GenericSQLTestODBC");
            DataSet itemDataSet = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.LoadDataSet(transaction, CommandType.Text, "begin Update Items set ItemDescription='Notinfotech' where ItemID=1 Select * from Items order by ItemID end", itemDataSet, new string[] { "Items" });
                transaction.Rollback();
                connection.Close();
            }

            int index = 0;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;

            var cmd = new OdbcCommand("select ItemDescription from Items ");
            var dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[index][0].ToString().Trim());
                index++;
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreSavedWhenLoadDataSetCommitWithStoredProcAndTableNameArray()
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
            DataSet da = new DataSet();
            da.ReadXml(ItemLDSctspparamXML);
            int index = 0;
            Assert.AreEqual(da.Tables[0].Rows.Count, itemDataSet.Tables[0].Rows.Count);
            for (index = 0; index < da.Tables[0].Rows.Count; index++)
            {
                Assert.AreEqual(da.Tables[0].Rows[index][0].ToString().Trim(), itemDataSet.Tables[0].Rows[index][1].ToString().Trim());
            }

            var cmd = new OdbcCommand("Delete from Items where ItemID=4");
            db.ExecuteNonQuery(cmd);
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\CustomersOrders.xml")]
        [DeploymentItem(@"TestFiles\CustomersOrders0.xml")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        [DeploymentItem(@"TestFiles\updateItems.xml")]
        [DeploymentItem(@"TestFiles\LDSctspparam.xml")]
        public void RecordsAreNotSavedWhenLoadDataSetRollbackWithStoredProcAndTableNameArray()
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
            int i = 0;
            DataSet da = new DataSet();
            da.ReadXml(ItemsXMLfile);
            int count = da.Tables[0].Rows.Count;

            OdbcCommand cmd = new OdbcCommand("select ItemDescription from Items ");
            IDataReader dr = db.ExecuteReader(cmd);
            while (dr.Read())
            {
                Assert.AreEqual(dr["ItemDescription"].ToString().Trim(), da.Tables[0].Rows[i][0].ToString().Trim());
                i++;
            }
        }

        #endregion
    }
}

