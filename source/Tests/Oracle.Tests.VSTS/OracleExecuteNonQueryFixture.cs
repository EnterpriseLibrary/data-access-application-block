// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport;
using Microsoft.Practices.EnterpriseLibrary.Data.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests
{
    [TestClass]
    
    public class OracleExecuteNonQueryFixture
    {
        ExecuteNonQueryFixture baseFixture;
        Database db;
        const string insertString = "insert into Region values (77, 'Elbonia')";
        const string countQuery = "select count(*) from Region";

        [TestInitialize]
        public void SetUp()
        {
            EnvironmentHelper.AssertOracleClientIsInstalled();
            DatabaseProviderFactory factory = new DatabaseProviderFactory(OracleTestConfigurationSource.CreateConfigurationSource());
            db = factory.Create("OracleTest");

            DbCommand insertionCommand = db.GetSqlStringCommand(insertString);
            DbCommand countCommand = db.GetSqlStringCommand(countQuery);

            baseFixture = new ExecuteNonQueryFixture(db, insertString, countQuery, insertionCommand, countCommand);
        }

        [TestMethod]
        public void CanExecuteNonQueryWithCommandTextWithDefinedTypeAndTransaction()
        {
            baseFixture.CanExecuteNonQueryWithCommandTextWithDefinedTypeAndTransaction();
        }

        [TestMethod]
        public void CanExecuteNonQueryWithDbCommand()
        {
            baseFixture.CanExecuteNonQueryWithDbCommand();
        }

        [TestMethod]
        public void CanExecuteNonQueryThroughTransaction()
        {
            baseFixture.CanExecuteNonQueryThroughTransaction();
        }

        [TestMethod]
        public void TransactionActuallyRollsBack()
        {
            baseFixture.TransactionActuallyRollsBack();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNonQueryWithNullDbTransaction()
        {
            baseFixture.ExecuteNonQueryWithNullDbTransaction();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteNonQueryWithNullDbCommandAndTransaction()
        {
            baseFixture.ExecuteNonQueryWithNullDbCommandAndTransaction();
        }
    }
}
