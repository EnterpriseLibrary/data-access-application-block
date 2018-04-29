// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using System.Linq;
using EnterpriseLibrary.Common.Configuration;
using EnterpriseLibrary.Data.Configuration;
using EnterpriseLibrary.Data.Oracle;
using EnterpriseLibrary.Data.Oracle.Configuration;
using EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnterpriseLibrary.Data.Tests.Configuration
{
    [TestClass]
    public class GivenConnectionStringForSqlServer
    {
        private DictionaryConfigurationSource configurationSource;
        private DatabaseSyntheticConfigSettings settings;

        [TestInitialize]
        public void Given()
        {
            configurationSource = new DictionaryConfigurationSource();
            var connectionStringsSection = new ConnectionStringsSection();
            connectionStringsSection.ConnectionStrings.Add(
                new ConnectionStringSettings
                    {
                        Name = "sql connection",
                        ConnectionString = "connection string",
                        ProviderName = "System.Data.SqlClient"
                    });
            configurationSource.Add("connectionStrings", connectionStringsSection);

            settings = new DatabaseSyntheticConfigSettings(this.configurationSource.GetSection);
        }

        [TestMethod]
        public void when_creating_database_then_creates_sql_database()
        {
            var database = this.settings.Databases.First().BuildDatabase();

            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
            Assert.AreEqual("connection string", database.ConnectionString);
        }
    }

    [TestClass]
    public class GivenConnectionForOdbcDatabase
    {
        private DictionaryConfigurationSource configurationSource;
        private DatabaseSyntheticConfigSettings settings;

        [TestInitialize]
        public void Given()
        {
            configurationSource = new DictionaryConfigurationSource();
            var connectionStringsSection = new ConnectionStringsSection();
            connectionStringsSection.ConnectionStrings.Add(
                new ConnectionStringSettings
                    {
                        Name = "odbc connection",
                        ConnectionString = "connection string",
                        ProviderName = "System.Data.Odbc"
                    });
            configurationSource.Add("connectionStrings", connectionStringsSection);

            settings = new DatabaseSyntheticConfigSettings(configurationSource.GetSection);
        }

        [TestMethod]
        public void when_creating_database_then_creates_generic_database()
        {
            var database = this.settings.Databases.First().BuildDatabase();

            Assert.IsInstanceOfType(database, typeof(GenericDatabase));
            Assert.AreEqual("connection string", database.ConnectionString);
        }
    }

    [TestClass]
    public class GivenConfigurationForAnOracleDatabase
    {
        private DictionaryConfigurationSource configurationSource;
        private DatabaseSyntheticConfigSettings settings;

        [TestInitialize]
        public void Given()
        {
            // Setup connection strings
            configurationSource = new DictionaryConfigurationSource();
            var connectionStringsSection = new ConnectionStringsSection();
            connectionStringsSection.ConnectionStrings.Add(
                new ConnectionStringSettings
                    {
                        Name = "myConnectionName",
                        ConnectionString = "myConnectionString",
                        ProviderName = "Oracle.ManagedDataAccess.Client"
                    }
                );

            configurationSource.Add("connectionStrings", connectionStringsSection);

            // Setup oracle configuration sections
            var oracleConnectionData = new OracleConnectionData { Name = "myConnectionName" };
            oracleConnectionData.Packages.Add(new OraclePackageData("foo", "bar"));

            var oracleSettings = new OracleConnectionSettings();
            oracleSettings.OracleConnectionsData.Add(oracleConnectionData);
            configurationSource.Add(OracleConnectionSettings.SectionName, oracleSettings);


            settings = new DatabaseSyntheticConfigSettings(configurationSource.GetSection);
        }

#pragma warning disable 612, 618
        [TestMethod]
        public void when_creating_database_then_creates_oracle_database_with_packages()
        {
            var database = this.settings.Databases.First().BuildDatabase();

            Assert.IsInstanceOfType(database, typeof(OracleDatabase));
            Assert.AreEqual("myConnectionString", database.ConnectionString);

            var command = database.GetStoredProcCommand("barTest");
            Assert.AreEqual("foo.barTest", command.CommandText);
        }
#pragma warning restore 612, 618
    }
}
