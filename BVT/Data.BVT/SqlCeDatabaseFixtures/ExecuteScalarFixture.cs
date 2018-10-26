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
    /// Tests the ExecuteScalar Method of the Database Class
    /// </summary>
    [TestClass]
    public class ExecuteScalarFixture : SqlCeDatabaseFixtureBase
    {
        private string itemsXMLfile;
        private DataSet dsExpectedResult;

        public ExecuteScalarFixture() :
            base("ConfigFiles.OlderTestsConfiguration.config")
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
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            string SqlCeCommand = "select ItemDescription from items where ItemID=1";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
            object actualResult = db.ExecuteScalar(dbCommandWrapper);

            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        #endregion

        #region "Command Type, Command Text"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void ItemIsReturnedWhenExecuteScalarWithCommandText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            string SqlCeCommand = "select ItemDescription from items where ItemID=1";
            DbCommand dbCommandWrapper = db.GetSqlStringCommand(SqlCeCommand);
            object actualResult = db.ExecuteScalar(CommandType.Text, SqlCeCommand);
            Assert.AreEqual(actualResult.ToString().Trim(), dsExpectedResult.Tables[0].Rows[0][0].ToString());
        }

        #endregion

        #region "Transaction, CommandType, Command Text"

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsInsertedWhenExecuteScalarUsingTransAndText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                object actualResult1 = db.ExecuteScalar(transaction, CommandType.Text, "Insert into CustomersOrders values(5,'orange',3,600)");
                object actualResult = db.ExecuteScalar(transaction, CommandType.Text, "Select count(*) from CustomersOrders where CustomerID='5'");
                transaction.Commit();
                Assert.AreEqual(Convert.ToInt32(actualResult.ToString().Trim()), 1);
                connection.Close();
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        [DeploymentItem(@"TestFiles\Items.xml")]
        public void RecordIsNotInsertedWhenExecuteScalarAndTransRollbackWithText()
        {
            Database db = DatabaseFactory.CreateDatabase("SqlCeTest");
            using (DbConnection connection = db.CreateConnection())
            {
                bool value = true;
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                db.ExecuteScalar(transaction, CommandType.Text, "Insert into CustomersOrders values(6,'Hutch',3,200) ");
                transaction.Rollback();
                connection.Close();
                SqlCeConnection conn = new SqlCeConnection();
                StringBuilder readerDataActual = new StringBuilder();
                conn.ConnectionString = connStr;
                conn.Open();
                SqlCeCommand cmd = new SqlCeCommand("Select CustomerName from CustomersOrders where CustomerID='6' ", conn);
                SqlCeDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    readerDataActual.Append(dr["CustomerName"]);
                    value = false;
                }
                conn.Close();
                Assert.IsTrue(value);
            }
        }

        #endregion
    }
}

