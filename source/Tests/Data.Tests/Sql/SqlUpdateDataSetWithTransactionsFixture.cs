﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using EnterpriseLibrary.Data.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnterpriseLibrary.Data.Sql.Tests
{
    [TestClass]
    public class SqlUpdateDataSetWithTransactionsFixture : UpdateDataSetWithTransactionsFixture
    {
        [TestInitialize]
        public void Initialize()
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory(TestConfigurationSource.CreateConfigurationSource());
            db = factory.CreateDefault();

            try
            {
                DeleteStoredProcedures();
            }
            catch { }
            CreateStoredProcedures();
            base.SetUp();
        }

        [TestCleanup]
        public void Dispose()
        {
            base.TearDown();
            DeleteStoredProcedures();
        }

        [TestMethod]
        public void SqlModifyRowWithStoredProcedure()
        {
            base.ModifyRowWithStoredProcedure();
        }

        [TestMethod]
        public void SqlAttemptToInsertBadRowInsideOfATransaction()
        {
            base.AttemptToInsertBadRowInsideOfATransaction();
        }

        protected override void CreateDataAdapterCommands()
        {
            SqlDataSetHelper.CreateDataAdapterCommands(db, ref insertCommand, ref updateCommand, ref deleteCommand);
        }

        protected override void CreateStoredProcedures()
        {
            SqlDataSetHelper.CreateStoredProcedures(db);
        }

        protected override void DeleteStoredProcedures()
        {
            SqlDataSetHelper.DeleteStoredProcedures(db);
        }

        protected override void AddTestData()
        {
            SqlDataSetHelper.AddTestData(db);
        }
    }
}
