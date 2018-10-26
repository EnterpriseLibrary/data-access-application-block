// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.BVT;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.OracleDatabaseFixtures
{
    [TestClass]
    public class ExecuteDataSetFixture : EntLibFixtureBase
    {
        static DataSet dsCustomers = null;
        static DataSet dsProducts = null;

        public ExecuteDataSetFixture()
            : base(@"ConfigFiles.OracleDatabaseFixture.config")
        {
        }

        #region Additional test attributes

        [TestInitialize()]
        public override void Initialize()
        {
            dsCustomers = new DataSet();
            dsProducts = new DataSet();
            string curPath = Environment.CurrentDirectory;
            string customerFile = Path.Combine(curPath, @"Customers.xml");
            string productFile = Path.Combine(curPath, @"Products.xml");
            dsCustomers.ReadXml(customerFile);
            dsProducts.ReadXml(productFile);

            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #endregion

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void DataSetContainsCorrectCustomersWhenUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string sqlText = "select * From Customers where customerId in ('BLAUS','BLONP','BOLID') order by CustomerId";

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, sqlText))
            {
                int rows = dsCustomers.Tables[0].Rows.Count;
                int columns = dsCustomers.Tables[0].Columns.Count;

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
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void DataSetContainsCorrectCustomersWhenUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "GetCustomersView";
            using (DataSet ds = db.ExecuteDataSet(CommandType.StoredProcedure, spName))
            {
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
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void DataSetContainsCorrectCustomersWhenUsingStoredProcCommandWithParameter()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetCustomerByID");
            db.AddInParameter(dbCommand, "vCustomerID", DbType.String, "BLAUS");

            using (DataSet ds = db.ExecuteDataSet(dbCommand))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[0][j].ToString().Trim(), ds.Tables[0].Rows[0][j].ToString().Trim());
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void DataSetContainsCorrectCustomersWhenUsingStoredProcCommandWithOutParameter()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetCustomerOut");
            db.AddInParameter(dbCommand, "vCustomerID", DbType.String, "BLONP");
            db.AddOutParameter(dbCommand, "vName", DbType.String, 50);
            using (DataSet ds = db.ExecuteDataSet(dbCommand))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[1][j].ToString().Trim(), ds.Tables[0].Rows[0][j].ToString().Trim());
                }
                Assert.AreEqual(dsCustomers.Tables[0].Rows[1]["CompanyName"].ToString(), (string)db.GetParameterValue(dbCommand, "vName"));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void DataSetContainsCorrectCustomersWhenUsingStoredProcTextWithParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            using (DataSet ds = db.ExecuteDataSet("GetCustomerByID", new object[] { "BOLID", 1 }))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                for (int j = 0; j < columns; j++)
                {
                    Assert.AreEqual(dsCustomers.Tables[0].Rows[2][j].ToString().Trim(), ds.Tables[0].Rows[0][j].ToString().Trim());
                }
            }
        }

        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        [TestMethod]
        public void RecordNotUpdatedWhenUsingStoredProcCommandWithParameterAndErrorOccurs()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet dsAddCountry = null;
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteDataSet(transaction, "UpdateCountryListAll", new object[] { "US", "United States of America", "" });
                    dsAddCountry = db.ExecuteDataSet(transaction, "AddCountryListAll", new object[] { "IN", "India", "" });
                    Assert.Fail("Exception should have been thrown.");
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    Assert.IsFalse("United States of America" == (string)db.ExecuteScalar(CommandType.Text, "select CountryName from Country where CountryCode='US'"));
                    Assert.IsNull(dsAddCountry);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordIsUpdatedWhenUsingStoredProcCommandWithParameterAndCommit()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    int initialCount = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select count(countrycode) from Country"));
                    db.ExecuteDataSet(transaction, "AddCountryListAll", new object[] { "TEMP", "Temporary", "" });
                    db.ExecuteDataSet(transaction, "DeleteCountryListAll", new object[] { "TEMP", "" });
                    transaction.Commit();
                    int finalCount = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select count(countrycode) from Country"));
                    Assert.IsTrue(finalCount == initialCount);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Assert.Fail("Transaction Rolled Back : " + e.Message);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void DataSetContainsCorrectCustomersWhenUsingCommandText()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            string sqlText = "select * From Customers where customerId in ('BLAUS','BLONP','BOLID') order by CustomerId";
            DbCommand dbCmdSql = db.GetSqlStringCommand(sqlText);
            using (DataSet ds = db.ExecuteDataSet(dbCmdSql))
            {
                int rows = dsCustomers.Tables[0].Rows.Count;
                int columns = dsCustomers.Tables[0].Columns.Count;

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
        }
    }
}

