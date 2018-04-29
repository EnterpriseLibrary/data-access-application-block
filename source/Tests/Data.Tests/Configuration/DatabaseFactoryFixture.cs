// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.Odbc;
using EnterpriseLibrary.Common.Configuration;
using EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnterpriseLibrary.Data.Tests.Configuration
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
            this.factory = new DatabaseProviderFactory(new SystemConfigurationSource(false).GetSection);
        }

        [TestMethod]
        public void CanCreateSqlDatabase()
        {
            Database database = this.factory.Create("Service_Dflt");
            Assert.IsNotNull(database);
            Assert.AreSame(typeof(SqlDatabase), database.GetType());
        }

        [TestMethod]
        public void CanCreateGenericDatabase()
        {
            Database database = this.factory.Create("OdbcDatabase");
            Assert.IsNotNull(database);
            Assert.AreSame(typeof(GenericDatabase), database.GetType());

            // provider factories aren't exposed
            DbCommand command = database.GetStoredProcCommand("ignore");
            Assert.AreSame(typeof(OdbcCommand), command.GetType());
        }
    }
}
