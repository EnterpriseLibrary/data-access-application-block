// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.OleDb;
using Microsoft.Practices.EnterpriseLibrary.Data.OleDb.Configuration.Fluent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests.Configuration
{
    public abstract partial class Given_ConfigureDataEntryPoint
    {
        [TestClass]
        public class When_PassingNullConnectionStringBuilderToOleDbDatabase : Given_NamedDatabase
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Then_WithConnectionString_ThrowsArgumentNullException()
            {
                DatabaseConfiguration
                    .ThatIs.AnOleDbDatabase()
                        .WithConnectionString((OleDbConnectionStringBuilder)null);
            }
        }

        [TestClass]
        public class When_PassingNullConnectionStringToOleDbDatabase : Given_NamedDatabase
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Then_WithConnectionString_ThrowsArgumentException()
            {
                DatabaseConfiguration
                    .ThatIs.AnOleDbDatabase()
                        .WithConnectionString((string)null);
            }
        }

        [TestClass]
        public class When_ConfiguringForOleDb : Given_NamedDatabase
        {
            private OleDbConnectionStringBuilder builder;

            protected override void Arrange()
            {
                base.Arrange();
                builder = new OleDbConnectionStringBuilder()
                {
                    DataSource = "someSource",
                    FileName = "SomeFile"

                };
            }

            protected override void Act()
            {
                DatabaseConfiguration
                    .ThatIs.AnOleDbDatabase()
                        .WithConnectionString(builder);
            }

            [TestMethod]
            public void Then_ConnectionStringProviderIsOleDb()
            {
                Assert.AreEqual("System.Data.OleDb", GetConnectionStringSettings().ProviderName);
            }

            [TestMethod]
            public void Then_ConnectionStringMatchesBuilderString()
            {
                Assert.AreEqual(builder.ConnectionString, GetConnectionStringSettings().ConnectionString);
            }
        }
    }
}
