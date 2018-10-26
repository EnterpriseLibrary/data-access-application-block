// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    [TestClass]
    public class GenericDatabaseAccessorFixture : EntLibFixtureBase
    {
        private GenericDatabase genericDatabase = null;

        public GenericDatabaseAccessorFixture()
            : base("ConfigFiles.GenericDatabase.config")
        {
        }

        [TestInitialize]
        public void Setup()
        {
            WriteEmbeddedFileToDisk(Assembly.GetExecutingAssembly(), "Database.mdb");

            genericDatabase = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault() as GenericDatabase;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenExecutingASprocAccessor()
        {
            genericDatabase.ExecuteSprocAccessor<PersonForGenericDB>("Test", 1);
        }

        [TestMethod]
        public void ValueIsReturnedWhenExecutingAStringAccessor()
        {
            var result = genericDatabase.ExecuteSqlStringAccessor<PersonForGenericDB>("SELECT PersonID, FirstName, Age FROM Person",
                        MapBuilder<PersonForGenericDB>.MapAllProperties()
                        .Map(c => c.ID).ToColumn("PersonID")
                        .Map(c => c.Name).ToColumn("FirstName")
                        .Build());

            var person = result.First();

            Assert.AreEqual(1, person.ID);
        }
    }
}

