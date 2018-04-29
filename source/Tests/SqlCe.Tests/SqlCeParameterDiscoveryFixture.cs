// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using EnterpriseLibrary.Data.SqlCe;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Data.SqlCe.Tests.VSTS
{
    [TestClass]
    public class SqlCeParameterDiscoveryFixture
    {
        [TestCleanup]
        public void TearDown()
        {
            SqlCeConnectionPool.CloseSharedConnections();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void CannotGetCommandForStoredProcedure()
        {
            TestConnectionString testConnection = new TestConnectionString();
            testConnection.CopyFile();
            SqlCeDatabase db = new SqlCeDatabase(testConnection.ConnectionString);
            db.GetStoredProcCommand("CustOrdersOrders");
        }
    }
}
