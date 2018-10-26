// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.Data.SqlCe;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlCeDatabaseFixtures
{
    [TestClass]
    public class SqlCeConnectionStringFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConnectionStringIsEmpty()
        {
            SqlCeDatabase database = new SqlCeDatabase("");
        }
    }
}

