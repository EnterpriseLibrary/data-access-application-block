// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.BVT;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.OracleDatabaseFixtures
{
    [TestClass]
    public class LoadDataSetFixture : EntLibFixtureBase
    {
        DataSet dsCustomers = null;
        DataSet dsProducts = null;

        public LoadDataSetFixture()
            : base(@"ConfigFiles.OracleDatabaseFixture.config")
        {
        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            dsCustomers = new DataSet();
            dsProducts = new DataSet();
            string curPath = Environment.CurrentDirectory;
            string customerFile = Path.Combine(curPath, "Customers.xml");
            string productFile = Path.Combine(curPath, "ProductsOra.xml");
            dsCustomers.ReadXml(customerFile);
            dsProducts.ReadXml(productFile);
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);
        }

        [TestCleanup()]
        public override void Cleanup()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            db.ExecuteNonQuery(CommandType.Text, "delete from Country where CountryCode in ('NZ','ZIM')");
            dsProducts.Dispose();
            dsCustomers.Dispose();
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingTextAndTableName()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();
            db.LoadDataSet(CommandType.Text, "select * from Customers where CustomerId in ('BLAUS','BLONP','BOLID') order by CustomerId", ds, new string[] { "Customers" });
            int rows = ds.Tables[0].Rows.Count;
            int columns = ds.Tables[0].Columns.Count;
            Assert.IsTrue(rows == dsCustomers.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsCustomers.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables[0].Rows[i][j].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingTextAndMultipleTables()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();
            db.LoadDataSet(CommandType.Text, "select * from Customers where CustomerId in ('BLAUS','BLONP','BOLID') order by CustomerId", ds, new string[] { "Customers" });
            db.LoadDataSet(CommandType.Text, " select * from Products where ProductId in (1,2,3,4,5) order by productid", ds, new string[] { "Products" });
            int rows = ds.Tables["Customers"].Rows.Count;
            int columns = ds.Tables["Customers"].Columns.Count;
            Assert.IsTrue(rows == dsCustomers.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsCustomers.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Customers"].Rows[i][j].ToString().Trim());
                }
            }
            rows = ds.Tables["Products"].Rows.Count;
            columns = ds.Tables["Products"].Columns.Count;
            Assert.IsTrue(rows == dsProducts.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsProducts.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsProducts.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Products"].Rows[i][j].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingStoredProcAndMultipleTables()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();
            db.LoadDataSet(CommandType.StoredProcedure, "GetCustomersView", ds, new string[] { "Customers" });
            db.LoadDataSet(CommandType.StoredProcedure, "GetProductsView", ds, new string[] { "Products" });
            int rows = ds.Tables["Customers"].Rows.Count;
            int columns = ds.Tables["Customers"].Columns.Count;
            Assert.IsTrue(rows == dsCustomers.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsCustomers.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Customers"].Rows[i][j].ToString().Trim());
                }
            }
            rows = ds.Tables["Products"].Rows.Count;
            columns = ds.Tables["Products"].Columns.Count;
            Assert.IsTrue(rows == dsProducts.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsProducts.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsProducts.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Products"].Rows[i][j].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingStoredProcAndMultipleTableOverload()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();
            db.LoadDataSet("GetCustomersAndProductsView", ds, new string[] { "Customers", "Products" }, new object[] { 1, 1 });
            int rows = ds.Tables["Customers"].Rows.Count;
            int columns = ds.Tables["Customers"].Columns.Count;
            Assert.IsTrue(rows == dsCustomers.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsCustomers.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Customers"].Rows[i][j].ToString().Trim());
                }
            }
            rows = ds.Tables["Products"].Rows.Count;
            columns = ds.Tables["Products"].Columns.Count;
            Assert.IsTrue(rows == dsProducts.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsProducts.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsProducts.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Products"].Rows[i][j].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void DataIsLoadedWhenLoadDataSetUsingStoredProcCommandAndMultipleTables()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();
            OracleCommand dbCommand = (OracleCommand)db.GetStoredProcCommand("GetCustomersAndProductsView");
            db.AddParameter(dbCommand, "cur_OUT", OracleDbType.RefCursor, 100, ParameterDirection.Output, true, 1, 1, null, DataRowVersion.Default, null);
            db.AddParameter(dbCommand, "cur_Products", OracleDbType.RefCursor, 100, ParameterDirection.Output, true, 1, 1, null, DataRowVersion.Default, null);
            db.LoadDataSet(dbCommand, ds, new string[] { "Customers", "Products" });
            int rows = ds.Tables["Customers"].Rows.Count;
            int columns = ds.Tables["Customers"].Columns.Count;
            Assert.IsTrue(rows == dsCustomers.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsCustomers.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Customers"].Rows[i][j].ToString().Trim());
                }
            }
            rows = ds.Tables["Products"].Rows.Count;
            columns = ds.Tables["Products"].Columns.Count;
            Assert.IsTrue(rows == dsProducts.Tables[0].Rows.Count);
            Assert.IsTrue(columns == dsProducts.Tables[0].Columns.Count);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsProducts.Tables[0].Rows[i][j].ToString().Trim(), ds.Tables["Products"].Rows[i][j].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void RecordsAreSavedwhenLoadDataSetStoredProcTextAndCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet dsCountry = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.LoadDataSet(transaction, "AddCountryListAll", dsCountry, new string[] { "Country" }, new object[] { "NZ", "NewZealand", "" });
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Assert.Fail("The transaction is rolled back : " + e.Message);
                }
                dsCountry.Tables[0].PrimaryKey = new DataColumn[] { dsCountry.Tables[0].Columns["CountryCode"] };
                DataRow result = dsCountry.Tables[0].Rows.Find("NZ");
                Assert.IsNotNull(result);
                Assert.AreEqual("NewZealand", result["CountryName"].ToString());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void RecordsAreNotSavedwhenLoadDataSetStoredProcTextAndRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet dsCountry = new DataSet();
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.LoadDataSet(transaction, "DeleteCountryListAll", dsCountry, new string[] { "Country" }, new object[] { "US", "" });
                    db.LoadDataSet(transaction, "AddCountryListAll", dsCountry, new string[] { "Country" }, new object[] { "IN", "India", "" });
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
                string countryName = (string)db.ExecuteScalar(CommandType.Text, "select countryName from Country where CountryCode='US'");
                Assert.AreEqual(countryName, "UnitedStates");
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void RecordsAreSavedwhenLoadDataSetStoredProcCommandAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet dsCountry = new DataSet();
            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountryListAll");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "ZIM");
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "Zimbabwe");
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountryListAll");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "ZIM");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.LoadDataSet(dbAddCountry, dsCountry, "Country", transaction);
                    dsCountry.Clear();
                    db.LoadDataSet(dbDeleteCountry, dsCountry, "Country", transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Assert.Fail("The transaction is rolled back : " + e.Message);
                }
                dsCountry.Tables[0].PrimaryKey = new DataColumn[] { dsCountry.Tables[0].Columns["CountryCode"] };
                DataRow result = dsCountry.Tables[0].Rows.Find("ZIM");
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestFiles\Customers.xml")]
        [DeploymentItem(@"TestFiles\ProductsOra.xml")]
        public void RecordsAreNotSavedwhenLoadDataSetStoredProcCommandAndTransactionRollback()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            DataSet dsCountry = new DataSet();
            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountryListAll");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "SCO");
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "Scotland");
            DbCommand dbAddCountry1 = db.GetStoredProcCommand("AddCountryListAll");
            db.AddInParameter(dbAddCountry1, "vCountryCode", DbType.String, "SCO");
            db.AddInParameter(dbAddCountry1, "vCountryName", DbType.String, "Scotland");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.LoadDataSet(dbAddCountry, dsCountry, "Country", transaction);
                    db.LoadDataSet(dbAddCountry1, dsCountry, "Country", transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
            string countryName = (string)db.ExecuteScalar(CommandType.Text, "select countryName from Country where CountryCode='SCO'");
            Assert.IsNull(countryName);
        }
    }
}

