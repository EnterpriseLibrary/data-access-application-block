// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Sql.Tests
{
    [TestClass]
    public class SqlStoredProcedureCreatingFixture : StoredProcedureCreationBase
    {
        [TestInitialize]
        public void SetUp()
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory(TestConfigurationSource.CreateConfigurationSource());
            db = factory.CreateDefault();

            CompleteSetup(db);
        }

        [TestCleanup]
        public void TearDown()
        {
            Cleanup();
        }

        protected override void CreateStoredProcedure()
        {
            string storedProcedureCreation = "CREATE procedure [TestProc] " +
                                             "(@vCount int output, @vCustomerId varchar(15)) AS " +
                                             "set @vCount = (select count(*) from Orders where CustomerId = @vCustomerId)";

            DbCommand command = db.GetSqlStringCommand(storedProcedureCreation);
            db.ExecuteNonQuery(command);
        }

        protected override void DeleteStoredProcedure()
        {
            string storedProcedureDeletion = "Drop procedure TestProc";
            DbCommand command = db.GetSqlStringCommand(storedProcedureDeletion);
            db.ExecuteNonQuery(command);
        }

        [TestMethod]
        public void CanGetOutputValueFromStoredProcedure()
        {
            baseFixture.CanGetOutputValueFromStoredProcedure();
        }

        [TestMethod]
        public void CanGetOutputValueFromStoredProcedureWithCachedParameters()
        {
            baseFixture.CanGetOutputValueFromStoredProcedureWithCachedParameters();
        }

        [TestMethod(), ExpectedException(typeof(InvalidOperationException))]
        public void ArgumentExceptionWhenThereAreTooFewParameters()
        {
            baseFixture.ArgumentExceptionWhenThereAreTooFewParameters();
        }

        [TestMethod(), ExpectedException(typeof(InvalidOperationException))]
        public void ArgumentExceptionWhenThereAreTooManyParameters()
        {
            baseFixture.ArgumentExceptionWhenThereAreTooFewParameters();
        }

        [TestMethod(), ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionThrownWhenReadingParametersFromCacheWithTooFewParameterValues()
        {
            baseFixture.ExceptionThrownWhenReadingParametersFromCacheWithTooFewParameterValues();
        }
    }
}
