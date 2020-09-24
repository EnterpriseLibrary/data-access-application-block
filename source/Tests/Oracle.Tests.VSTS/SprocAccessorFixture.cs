// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.TestSupport.ContextBase;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests
{
#pragma warning disable 612, 618
    [TestClass]
    public class WhenSprocAccessorIsCreatedForOracleDatabase : ArrangeActAssert
    {
        OracleDatabase database;

        protected override void Arrange()
        {
            EnvironmentHelper.AssertOracleClientIsInstalled();
            String connectionString = ConfigurationManager.ConnectionStrings["OracleTest"].ConnectionString;
            database = new OracleDatabase(connectionString);
        }

        [TestMethod]
        public void ThenCanExecuteParameterizedSproc()
        {
            var accessor = database.CreateSprocAccessor<Customer>("GetCustomerById");
            var result = accessor.Execute("BLAUS", null).ToArray();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        private class Customer
        {
            public string ContactName { get; set; }
        }
    }
#pragma warning restore 612, 618
}
