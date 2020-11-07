// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.Fluent;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql.Configuration.Fluent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class ValidFluentConfigurationFixture : FluentConfigurationFixtureBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void DatabaseTypeIsSetWhenConfiguringMappings()
        {
            configurationStart.WithProviderNamed(DbProviderMapping.DefaultSqlProviderName)
                                .MappedToDatabase<GenericDatabase>();

            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(1, settings.ProviderMappings.Count);

            var dbProviderMapping = settings.ProviderMappings.Get(DbProviderMapping.DefaultSqlProviderName);

            Assert.IsNotNull(dbProviderMapping);

            Assert.AreEqual(typeof(GenericDatabase), dbProviderMapping.DatabaseType);
        }

        [TestMethod]
        public void DatabaseConnectionIsReadFromConfigFileWhenSetAsDefault()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault();

            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(DatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[DatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[DatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[DatabaseName].ProviderName;

            Assert.AreEqual<string>(DefaultConnectionString, connectionString);
            Assert.AreEqual(DbProviderMapping.DefaultSqlProviderName, providerName);
        }

        [TestMethod]
        public void GenericDatabaseIsReadFromConfigFileWhenSetAsDefault()
        {
            var dbConnectionBuilder = new DbConnectionStringBuilder();

            dbConnectionBuilder[UidPropertyName] = Uid;
            dbConnectionBuilder[PwdPropertyName] = Pwd;
            dbConnectionBuilder[DataSourcePropertyName] = DataSource;

            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnotherDatabaseType(GenericProviderName)
                                    .WithConnectionString(dbConnectionBuilder);


            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(DatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[DatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[DatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[DatabaseName].ProviderName;

            Assert.AreEqual(GenericConnectionString, connectionString);
            Assert.AreEqual(GenericProviderName, providerName);
        }

        [TestMethod]
        public void SqlDatabaseIsReadFromConfigFileWhenSetAsDefault()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .ASqlDatabase()
                                    .WithConnectionString(new SqlConnectionStringBuilder() { DataSource = DataSource, InitialCatalog = InitialCatalog, IntegratedSecurity = IntegratedSecurity });

            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(DatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[DatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[DatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[DatabaseName].ProviderName;

            var connectionBuilder = new SqlConnectionStringBuilder(connectionString);

            Assert.AreEqual(DataSource, connectionBuilder.DataSource);
            Assert.AreEqual(InitialCatalog, connectionBuilder.InitialCatalog);
            Assert.IsTrue(connectionBuilder.IntegratedSecurity);

            Assert.AreEqual(DbProviderMapping.DefaultSqlProviderName, providerName);
        }

        [TestMethod]
        public void SqlCeDatabaseIsReadFromConfigFileWhenSetAsDefault()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .ASqlCeDatabase()
                                    .WithConnectionString(SqlCeConnectionString);

            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(DatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[DatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[DatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[DatabaseName].ProviderName;

            Assert.AreEqual(SqlCeConnectionString, connectionString);

            Assert.AreEqual(SqlCeProviderName, providerName);
        }

        [TestMethod]
        public void OdbcDatabaseIsReadFromConfigFileWhenSetAsDefault()
        {
            var odbcBuilder = new OdbcConnectionStringBuilder()
            {
                Driver = Driver
            };

            odbcBuilder[DbqPropertyName] = Dbq;
            odbcBuilder[UidPropertyName] = Uid;
            odbcBuilder[PwdPropertyName] = Pwd;

            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOdbcDatabase()
                                    .WithConnectionString(odbcBuilder);

            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(DatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[DatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[DatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[DatabaseName].ProviderName;

            var connectionBuilder = new OdbcConnectionStringBuilder(connectionString);

            Assert.AreEqual(Driver, connectionBuilder.Driver);
            Assert.AreEqual(Dbq, connectionBuilder[DbqPropertyName]);
            Assert.AreEqual(Uid, connectionBuilder[UidPropertyName]);
            Assert.AreEqual(Pwd, connectionBuilder[PwdPropertyName]);
            Assert.AreEqual(OdbcProviderName, providerName);
        }

        [TestMethod]
        public void OleDbDatabaseIsReadFromConfigFileWhenSetAsDefault()
        {
            var oleDbBuilder = new OleDbConnectionStringBuilder()
            {
                FileName = OleDbFileName,
                Provider = OleDbProvider,
                DataSource = OleDbDataSource,
                PersistSecurityInfo = OleDbPersistSecurityInfo,
                OleDbServices = OleDbServicesValue
            };

            oleDbBuilder[UidPropertyName] = Uid;
            oleDbBuilder[PwdPropertyName] = Pwd;

            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOleDbDatabase()
                                    .WithConnectionString(oleDbBuilder);

            var settings = GetSettings<DatabaseSettings>();

            Assert.AreEqual(DatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[DatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[DatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[DatabaseName].ProviderName;

            var connectionBuilder = new OleDbConnectionStringBuilder(connectionString);

            Assert.AreEqual(OleDbFileName, connectionBuilder.FileName);
            Assert.AreEqual(OleDbProvider, connectionBuilder.Provider);
            Assert.AreEqual(OleDbDataSource, connectionBuilder.DataSource);
            Assert.AreEqual(OleDbPersistSecurityInfo, connectionBuilder.PersistSecurityInfo);
            Assert.AreEqual(OleDbServicesValue, connectionBuilder.OleDbServices);
            Assert.AreEqual(Uid, connectionBuilder[UidPropertyName]);
            Assert.AreEqual(Pwd, connectionBuilder[PwdPropertyName]);
            Assert.AreEqual(OleDbProviderName, providerName);
        }

        [TestMethod]
        public void OracleDatabaseIsReadFromConfigFileWhenSetAsDefault()
        {
            var oracleBuilder = new OracleConnectionStringBuilder()
            {
                DataSource = OracleDataSource,
                UserID = OracleUserID,
                Password = OraclePassword
            };

            configurationStart.ForDatabaseNamed(OracleDatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOracleDatabase()
                                    .WithConnectionString(oracleBuilder)
                                    .WithPackageNamed(PackageName)
                                    .AndPrefix(Prefix);

            var settings = GetSettings<DatabaseSettings>();

            var oracleSettings = GetSettings<OracleConnectionSettings>();

            Assert.IsNotNull(oracleSettings);

            var package = oracleSettings.OracleConnectionsData.Get(OracleDatabaseName).Packages.Get(PackageName);

            Assert.IsNotNull(package);

            Assert.AreEqual(Prefix, package.Prefix);

            Assert.AreEqual(OracleDatabaseName, settings.DefaultDatabase);

            var connectionStringSettings = GetConnectionStringSettings();

            var connectionStringElement = connectionStringSettings.ConnectionStrings[OracleDatabaseName];

            Assert.IsNotNull(connectionStringElement);

            var connectionString = connectionStringSettings.ConnectionStrings[OracleDatabaseName].ConnectionString;
            var providerName = connectionStringSettings.ConnectionStrings[OracleDatabaseName].ProviderName;

            var connectionBuilder = new OracleConnectionStringBuilder(connectionString);

            Assert.AreEqual(OracleDataSource, connectionBuilder.DataSource);
            Assert.AreEqual(OracleUserID, connectionBuilder.UserID);
            Assert.AreEqual(OraclePassword, connectionBuilder.Password);
            Assert.AreEqual(DbProviderMapping.DefaultOracleProviderName, providerName);
        }
    }
}

