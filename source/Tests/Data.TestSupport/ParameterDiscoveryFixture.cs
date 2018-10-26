// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.TestSupport
{
    public class ParameterDiscoveryFixture
    {
        DbCommand storedProcedure;

        public ParameterDiscoveryFixture(DbCommand storedProcedure)
        {
            this.storedProcedure = storedProcedure;
        }

        public void CanCreateStoredProcedureCommand()
        {
            Assert.AreEqual(storedProcedure.CommandType, CommandType.StoredProcedure);
        }

        public class TestCache : ParameterCache
        {
            public bool CacheUsed = false;

            protected override void AddParametersFromCache(DbCommand command,
                                                           Database database)
            {
                CacheUsed = true;
                base.AddParametersFromCache(command, database);
            }
        }
    }
}
