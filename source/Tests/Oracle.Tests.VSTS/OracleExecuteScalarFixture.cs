// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport;
using Microsoft.Practices.EnterpriseLibrary.Data.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests
{
    /// <summary>
    /// Test the ExecuteScalar method on the Database class
    /// </summary>
    [TestClass]
    [TestCategory("Oracle")]
    public class OracleExecuteScalarFixture
    {
        static Database db;
        static ExecuteScalarFixture baseFixture;

        [TestInitialize]
        public void SetUp()
        {
            EnvironmentHelper.AssertOracleClientIsInstalled();
            DatabaseProviderFactory factory = new DatabaseProviderFactory(OracleTestConfigurationSource.CreateConfigurationSource());
            db = factory.Create("OracleTest");
            DbCommand command = db.GetSqlStringCommand("Select count(*) from region");

            baseFixture = new ExecuteScalarFixture(db, command);
        }

        [TestMethod]
        public void ExecuteScalarWithIDbCommand()
        {
            baseFixture.ExecuteScalarWithIDbCommand();
        }

        [TestMethod]
        public void ExecuteScalarWithIDbTransaction()
        {
            baseFixture.ExecuteScalarWithIDbTransaction();
        }

        [TestMethod]
        public void CanExecuteScalarDoAnInsertion()
        {
            baseFixture.CanExecuteScalarDoAnInsertion();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteScalarWithNullIDbCommand()
        {
            baseFixture.ExecuteScalarWithNullIDbCommand();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteScalarWithNullIDbTransaction()
        {
            baseFixture.ExecuteScalarWithNullIDbTransaction();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteScalarWithNullIDbCommandAndNullTransaction()
        {
            baseFixture.ExecuteScalarWithNullIDbCommandAndNullTransaction();
        }

        [TestMethod]
        public void ExecuteScalarWithCommandTextAndTypeInTransaction()
        {
            baseFixture.ExecuteScalarWithCommandTextAndTypeInTransaction();
        }
    }
}
