// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    [TestClass]
    public class DatabaseFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructDatabaseWithNullConnectionStringThrows()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            new TestDatabase(null, factory);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructDatabaseWithNullDbProviderFactoryThrows()
        {
            new TestDatabase("foo", null);
        }

        class TestDatabase : Database
        {
            public TestDatabase(string connectionString,
                                DbProviderFactory dbProviderFactory)
                : base(connectionString, dbProviderFactory) {}

            //protected override char ParameterToken
            //{
            //    get { return 'a'; }
            //}

            protected override void DeriveParameters(DbCommand discoveryCommand) {}
        }
    }
}
