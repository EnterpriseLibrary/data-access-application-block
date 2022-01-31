// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Odbc;
using Microsoft.Practices.EnterpriseLibrary.Data.OleDb;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests.Configuration
{
    [TestClass]
    public class DatabaseFactoryFixture
    {
        public const string OracleTestStoredProcedureInPackageWithTranslation = "TESTPACKAGETOTRANSLATEGETCUSTOMERDETAILS";
        public const string OracleTestTranslatedStoredProcedureInPackageWithTranslation = "TESTPACKAGE.TESTPACKAGETOTRANSLATEGETCUSTOMERDETAILS";
        public const string OracleTestStoredProcedureInPackageWithoutTranslation = "TESTPACKAGETOKEEPGETCUSTOMERDETAILS";

        private DatabaseProviderFactory factory;

        [TestInitialize]
        public void Initialize()
        {
            NetCoreHelpers.RegisterDbProviderFactories();
            this.factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);
        }

        [TestMethod]
        public void CanCreateSqlDatabase()
        {
            Database database = this.factory.Create("Service_Dflt");
            Assert.IsNotNull(database);
            Assert.AreSame(typeof(SqlDatabase), database.GetType(), $"{nameof(SqlDatabase)} is different than {database.GetType().Name}");
        }

        [TestMethod]
        public void CanCreateGenericDatabase()
        {
            Database database = this.factory.Create("mapping2");
            Assert.IsNotNull(database);
            Assert.AreSame(typeof(GenericDatabase), database.GetType());

            // provider factories aren't exposed
            DbCommand command = database.GetStoredProcCommand("ignore");
            Assert.AreSame(typeof(SqlCommand), command.GetType());
        }

        [TestMethod]
        public void CanCreateOleDbDatabase()
        {
            Database database = factory.Create("OleDbDatabase");
            Assert.IsNotNull(database);
            Assert.AreSame(typeof(OleDbDatabase), database.GetType());
        }

        [TestMethod]
        public void CanCreateOdbcDatabase()
        {
            Database database = factory.Create("OdbcDatabase");
            Assert.IsNotNull(database);
            Assert.AreSame(typeof(OdbcDatabase), database.GetType());
        }
    }
}
