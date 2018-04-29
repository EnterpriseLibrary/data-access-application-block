// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using EnterpriseLibrary.Common.TestSupport.ContextBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnterpriseLibrary.Data.Tests
{
    public abstract class Given_GenericDatabaseInstance : ArrangeActAssert
    {
        protected Database Database { get; private set; }
        private const string northwind = @"server=(localdb)\v11.0;database=Northwind;Integrated Security=true";

        protected override void Arrange()
        {
            base.Arrange();

            Database = new GenericDatabase(northwind, SqlClientFactory.Instance);
        }
    }

    [TestClass]
    public class When_UsingGenericDatabase : Given_GenericDatabaseInstance
    {
        [TestMethod]
        public void Then_SupportsAsyncIsFalse()
        {
            Assert.IsFalse(Database.SupportsAsync);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Then_AsyncOperationThrows()
        {
            var command = Database.GetStoredProcCommand("Ten Most Popular Products");
            Database.BeginExecuteReader(command, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Then_AsyncAccessorThrows()
        {
            var accessor = Database.CreateSprocAccessor<object>("Ten Most Popular Products");
            accessor.BeginExecute(null, null);
        }
    }

}
