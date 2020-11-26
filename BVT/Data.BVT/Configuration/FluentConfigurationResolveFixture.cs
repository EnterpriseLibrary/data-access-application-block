// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql.Configuration.Fluent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class FluentConfigurationResolveFixture : FluentConfigurationFixtureBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void DatabaseIsResolvedWhenSetAsDefault()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault();

            DictionaryConfigurationSource source = new DictionaryConfigurationSource();
            builder.UpdateConfigurationWithReplace(source);
            base.ConfigurationSource = source;

            var database = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            Assert.IsNotNull(database);
            Assert.AreEqual(DefaultConnectionString, database.ConnectionString);
            Assert.IsInstanceOfType(database, typeof(GenericDatabase));
        }

        [TestMethod]
        public void SqlDatabaseIsResolvedWhenSetAsDefault()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                               .AsDefault()
                               .ThatIs
                                   .ASqlDatabase()
                                   .WithConnectionString(new SqlConnectionStringBuilder() { DataSource = DataSource, InitialCatalog = InitialCatalog, IntegratedSecurity = IntegratedSecurity });
            configurationStart.WithProviderNamed(DbProviderMapping.DefaultSqlProviderName)
                .MappedToDatabase<SqlDatabase>();
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();
            builder.UpdateConfigurationWithReplace(source);
            base.ConfigurationSource = source;

            var database = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            Assert.IsNotNull(database);

            var connectionBuilder = new SqlConnectionStringBuilder(database.ConnectionString);

            Assert.AreEqual(DataSource, connectionBuilder.DataSource);
            Assert.AreEqual(InitialCatalog, connectionBuilder.InitialCatalog);
            Assert.IsTrue(connectionBuilder.IntegratedSecurity);
            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
        }

        [TestMethod]
        public void GenericDatabaseIsResolvedWhenSetAsDefault()
        {
            var dbConnectionBuilder = new DbConnectionStringBuilder();

            dbConnectionBuilder[UidPropertyName] = Uid;
            dbConnectionBuilder[PwdPropertyName] = Pwd;
            dbConnectionBuilder[DataSourcePropertyName] = DataSource;

            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnotherDatabaseType(DbProviderMapping.DefaultSqlProviderName)
                                    .WithConnectionString(dbConnectionBuilder);
            configurationStart.WithProviderNamed(DbProviderMapping.DefaultSqlProviderName)
                .MappedToDatabase<SqlDatabase>();
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();
            builder.UpdateConfigurationWithReplace(source);
            base.ConfigurationSource = source;

            var database = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            Assert.AreEqual(GenericConnectionString, database.ConnectionString);
            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
        }

        [TestMethod]
        public void MappedDatabaseIsResolvedWhenSetAsDefault()
        {
            configurationStart.WithProviderNamed(DbProviderMapping.DefaultSqlProviderName)
                                .MappedToDatabase<SqlDatabase>()
                                .ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnotherDatabaseType(DbProviderMapping.DefaultSqlProviderName)
                                    .WithConnectionString(GenericConnectionString);
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();
            builder.UpdateConfigurationWithReplace(source);
            base.ConfigurationSource = source;
            //         ConfigureContainer();

            var database = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            Assert.IsNotNull(database);
            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
        }
    }
}

