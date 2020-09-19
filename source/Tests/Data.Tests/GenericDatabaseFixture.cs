// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    [TestClass]
    public class GenericDatabaseFixture
    {
        const string connectionString = "some connection string;";

        [TestInitialize]
        public void Setup()
        {
            NetCoreHelpers.RegisterDbProviderFactories();
        }

        [TestMethod]
        public void CanCreateGenericDatabaseFromSysConfiguration()
        {
            Database database =
                new DatabaseProviderFactory(new SystemConfigurationSource(false)).Create("OdbcDatabase");

            Assert.IsNotNull(database);
            Assert.AreEqual(typeof(GenericDatabase), database.GetType());
            Assert.AreEqual(typeof(OdbcFactory), database.DbProviderFactory.GetType());
            Assert.AreEqual(database.ConnectionStringWithoutCredentials, connectionString);
        }

        [TestMethod]
        public void CanDoExecuteDataReaderForGenericDatabaseBug1836()
        {
            Database db = new GenericDatabase($@"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};Dbq={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Northwind.mdb;Uid=sa;Pwd=sa;", OdbcFactory.Instance);
            
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();

                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "select * from [Order Details]")) { }
                    transaction.Commit();
                }
            }
        }

        [TestMethod]
        public void CanDoExecuteDataReaderForGenericDatabaseForSqlProviderBug1836()
        {
            Database db = new GenericDatabase(String.Format(@"server={0};database=Northwind;Integrated Security=true", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), SqlClientFactory.Instance);

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();

                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    using (IDataReader dataReader = db.ExecuteReader(transaction, CommandType.Text, "select * from [Order Details]")) { }
                    transaction.Commit();
                }
            }
        }

        [TestMethod]
        public void DSDBWrapperTransSqlNoCommitTestBug1857()
        {
            Database db = new GenericDatabase(String.Format(@"server={0};database=Northwind;Integrated Security=true", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), SqlClientFactory.Instance);

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    string sqlCommand = "select * from [Order Details]";
                    DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

                    DataSet dsActualResult = db.ExecuteDataSet(dbCommand, transaction);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void GenericDatabaseThrowsWhenAskedToDeriveParameters()
        {
            Database db = new GenericDatabase(String.Format(@"server={0};database=Northwind;Integrated Security=true", ConfigurationManager.AppSettings["SqlServerDatabaseInstance"]), SqlClientFactory.Instance);
            DbCommand storedProcedure = db.GetStoredProcCommand("CustOrdersOrders");
            db.DiscoverParameters(storedProcedure);
        }
    }
}
