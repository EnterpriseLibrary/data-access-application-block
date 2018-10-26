// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.DatabaseFixtures
{
    [TestClass]
    public class ConnectionStringsValidConfigurationFixture : EntLibFixtureBase
    {
        public ConnectionStringsValidConfigurationFixture()
            : base("ConfigFiles.ConnectionStringsOnly.config")
        {
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenDefaultDatabaseIsResolved()
        {
            new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();
        }

        [TestMethod]
        public void CorrectDatabaseTypeIsReturnedWhenNamedDatabaseIsResolved()
        {
            var database = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSql");

            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
        }
    }
}

