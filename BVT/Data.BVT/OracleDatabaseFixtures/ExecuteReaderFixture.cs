// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using EnterpriseLibrary.Data;
using EnterpriseLibrary.Data.BVT;
using EnterpriseLibrary.Data.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace EnterpriseLibrary.Data.BVT.OracleDatabaseFixtures
{
    [TestClass]
    public class ExecuteReaderFixture : EntLibFixtureBase
    {
        static DataSet dsCustomers = null;

        public ExecuteReaderFixture()
            : base(@"ConfigFiles.OracleDatabaseFixture.config")
        {
        }

        #region Additional test attributes

        [TestInitialize()]
        public override void Initialize()
        {
            dsCustomers = new DataSet();
            string curPath = Environment.CurrentDirectory;
            string customerFile = Path.Combine(curPath, "Customers.xml");
            dsCustomers.ReadXml(customerFile);
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource), false);
        }

        [TestCleanup()]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        #endregion

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreReturnedWhenUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string sqlText = "select * From Customers where customerId in ('BLAUS','BLONP','BOLID') order by CustomerId";

            using (IDataReader reader = db.ExecuteReader(CommandType.Text, sqlText))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                int i = 0;
                while (reader.Read())
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), reader[j].ToString().Trim());
                    }
                    i++;
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreReturnedWhenUsingStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "GetCustomersView";
            using (IDataReader reader = db.ExecuteReader(CommandType.StoredProcedure, spName))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                int i = 0;
                while (reader.Read())
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Assert.AreEqual(dsCustomers.Tables[0].Rows[i][j].ToString().Trim(), reader[j].ToString().Trim());
                    }
                    i++;
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreReturnedWhenUsingCommandText()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string sqlCommand = "select * from Customers where customerId ='BLAUS'";
            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

            using (IDataReader reader = db.ExecuteReader(dbCommand))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                while (reader.Read())
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Assert.AreEqual(dsCustomers.Tables[0].Rows[0][j].ToString().Trim(), reader[j].ToString().Trim());
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreReturnedWhenUsingStoredProcCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetCustomerByID");
            db.AddInParameter(dbCommand, "vCustomerID", DbType.String, "BLAUS");

            using (IDataReader reader = db.ExecuteReader(dbCommand))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                while (reader.Read())
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Assert.AreEqual(dsCustomers.Tables[0].Rows[0][j].ToString().Trim(), reader[j].ToString().Trim());
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreReturnedWhenUsingStoredProcCommandAndOutParameter()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetCustomerOut");
            db.AddInParameter(dbCommand, "vCustomerID", DbType.String, "BLONP");
            db.AddOutParameter(dbCommand, "vName", DbType.String, 50);
            using (IDataReader reader = db.ExecuteReader(dbCommand))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                while (reader.Read())
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Assert.AreEqual(dsCustomers.Tables[0].Rows[1][j].ToString().Trim(), reader[j].ToString().Trim());
                    }
                }
                Assert.AreEqual(dsCustomers.Tables[0].Rows[1]["CompanyName"].ToString(), (string)db.GetParameterValue(dbCommand, "vName"));
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreReturnedWhenUsingStoredProcTextWithParameter()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            using (IDataReader reader = db.ExecuteReader("GetCustomerByID", new object[] { "BOLID", 1 }))
            {
                int columns = dsCustomers.Tables[0].Columns.Count;
                while (reader.Read())
                {
                    for (int j = 0; j < columns; j++)
                    {
                        Assert.AreEqual(dsCustomers.Tables[0].Rows[2][j].ToString().Trim(), reader[j].ToString().Trim());
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreNotSavedWhenStoredProcWithParameterAndTransactionIsRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            IDataReader dbReaderAddCountry = null;
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (db.ExecuteReader(transaction, "UpdateCountryListAll", new object[] { "US", "United States of America", "" })) { }
                    dbReaderAddCountry = db.ExecuteReader(transaction, "AddCountryListAll", new object[] { "IN", "India", "" });
                    transaction.Commit();
                    Assert.Fail("Exception should have been thrown");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    Assert.IsFalse("United States of America" == (string)db.ExecuteScalar(CommandType.Text, "select CountryName from Country where CountryCode='US'"));
                    Assert.IsNull(dbReaderAddCountry);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void RecordsAreSavedWhenStoredProcWithParameterAndTransactionIsCommitted()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    int initialCount = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select count(countrycode) from Country"));
                    using (db.ExecuteReader(transaction, "AddCountryListAll", new object[] { "TEMP", "Temporary", "" })) { }
                    using (db.ExecuteReader(transaction, "DeleteCountryListAll", new object[] { "TEMP", "" })) { }
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
        /// <summary>
        /// Bug 18074
        /// </summary>
        [DeploymentItem(@"Testfiles\Customers.xml")]
        [DeploymentItem(@"Testfiles\Products.xml")]
        public void ValuesAreReadWhenUsingInnerReader()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            const string sqlCommand = "select * from Customers where customerId ='BLAUS'";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(sqlCommand);
            //IDataReader dataReader = db.ExecuteReader(CommandType.Text, sqlCommand);
            using (IDataReader dataReader = db.ExecuteReader(dbCommandWrapper))
            {
                using (OracleDataReader orareader = ((OracleDataReaderWrapper)dataReader).InnerReader)
                {
                    Assert.IsNotNull(orareader);
                    int n = orareader.GetOrdinal("customerId");
                    while (dataReader.Read())
                    {
                        string str1 = orareader.GetOracleValue(n).ToString();
                        string str = ((string)dataReader[0]).Trim();
                        Assert.AreNotSame(str1, str);
                    }
                }
            }
        }
    }
}

