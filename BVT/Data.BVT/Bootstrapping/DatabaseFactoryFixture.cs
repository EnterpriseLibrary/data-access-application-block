// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Bootstrapping
{
    [TestClass]
    public class DatabaseFactoryFixture : EntLibFixtureBase
    {
        public DatabaseFactoryFixture()
            : base(@"ConfigFiles.DatabaseFactoryFixture.config")
        {
        }

        [TestCleanup]
        public override void Cleanup()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenDatabaseFactoryIsNotInitialized()
        {
            DatabaseFactory.CreateDatabase();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenDatabaseFactoryIsInitializedTwice()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource), false);

            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource));
        }

        [TestMethod]
        public void NoExceptionIsThrownWhenDatabaseFactoryIsInitializedTwiceAndNoThrowOnError()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource), false);
            var db = DatabaseFactory.CreateDatabase("DefaultSql123");
            Assert.IsTrue(db.ConnectionString.Contains("database=Northwind123"));

            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);
            var db1 = DatabaseFactory.CreateDatabase("DataAccessQuickStart");
            Assert.IsTrue(db1.ConnectionString.Contains("EntLibQuickStarts"));
            DatabaseFactory.ClearDatabaseProviderFactory();
        }

        [TestMethod]
        public void ConnectionStringsAreSetWhenDatabaseFactoryIsClearedAndSetAgain()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource), false);
            var db = DatabaseFactory.CreateDatabase("DefaultSql123");
            Assert.IsTrue(db.ConnectionString.Contains("database=Northwind123"));
            DatabaseFactory.ClearDatabaseProviderFactory();
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);

            var db1 = DatabaseFactory.CreateDatabase("DataAccessQuickStart");
            Assert.IsTrue(db1.ConnectionString.Contains("EntLibQuickStarts"));
            DatabaseFactory.ClearDatabaseProviderFactory();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenNamedConnectionStringNotInConfig()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource), false);
            var db = DatabaseFactory.CreateDatabase("DoesnotExist");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenMappingDoesNotExist()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(base.ConfigurationSource), false);
            var db = DatabaseFactory.CreateDatabase("InvalidMapping");
        }
    }
}

