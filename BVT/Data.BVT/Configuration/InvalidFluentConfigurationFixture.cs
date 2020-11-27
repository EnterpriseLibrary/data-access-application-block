// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.Fluent;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql.Configuration.Fluent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class InvalidFluentConfigurationFixture : FluentConfigurationFixtureBase
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringMappingsWithInvalidDatabase()
        {
            configurationStart.WithProviderNamed(DbProviderMapping.DefaultSqlProviderName)
                                .MappedToDatabase(typeof(object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringMappingsWithInvalidProviderName()
        {
            configurationStart.WithProviderNamed(null)
                                .MappedToDatabase(typeof(object));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringDefaultDatabaseWithInvalidDatabaseName()
        {
            configurationStart.ForDatabaseNamed(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringOleDbDatabaseWithInvalidConnectionString()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOleDbDatabase()
                                    .WithConnectionString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringOleDbDatabaseWithInvalidConnectionStringBuilder()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOleDbDatabase()
                                    .WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringOdbcDatabaseWithInvalidConnectionString()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOdbcDatabase()
                                    .WithConnectionString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringOdbcDatabaseWithInvalidConnectionStringBuilder()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOdbcDatabase()
                                    .WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringSqlDatabaseWithInvalidConnectionString()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .ASqlDatabase()
                                    .WithConnectionString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringSqlDatabaseWithInvalidConnectionStringBuilder()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .ASqlDatabase()
                                    .WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringSqlCeDatabaseWithInvalidConnectionString()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .ASqlCeDatabase()
                                    .WithConnectionString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringOracleDatabaseWithInvalidConnectionString()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOracleDatabase()
                                    .WithConnectionString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringOracleDatabaseWithInvalidPackage()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOracleDatabase()
                                    .WithPackageNamed(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringOracleDatabaseWithInvalidPrefix()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnOracleDatabase()
                                    .WithPackageNamed(PackageName)
                                    .AndPrefix(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConfiguringAnotherDatabaseWithInvalidConnectionString()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnotherDatabaseType(GenericProviderName)
                                    .WithConnectionString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringAnotherDatabaseWithInvalidConnectionStringBuilder()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .AsDefault()
                                .ThatIs
                                    .AnotherDatabaseType(GenericProviderName)
                                    .WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringAnotherDatabaseWithNullConnectionStringProvider()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .ThatIs
                                .AnotherDatabaseType(GenericProviderName).WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringOdbcDatabaseWithNullConnectionStringProvider()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .ThatIs
                                .AnOdbcDatabase().WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringOleDbDatabaseWithNullConnectionStringProvider()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .ThatIs
                                .AnOleDbDatabase().WithConnectionString(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenConfiguringSqlDatabaseWithNullConnectionStringProvider()
        {
            configurationStart.ForDatabaseNamed(DatabaseName)
                                .ThatIs
                                .ASqlDatabase().WithConnectionString(null);
        }
    }
}

