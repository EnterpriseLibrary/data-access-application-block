// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.TestSupport;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport
{
    public static class OracleTestConfigurationSource
    {
        public const string OracleConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.10.235)(PORT=1522)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=DEPOSIT_CLONE_SITDB.localdomain)));user id=CBDB_DEPOSIT_ADMIN;password=Password1";
        public const string OracleConnectionStringName = "OracleTest";
        public const string OracleProviderName = "Oracle.ManagedDataAccess.Client";

        public static DictionaryConfigurationSource CreateConfigurationSource()
        {
            DictionaryConfigurationSource configSource = TestConfigurationSource.CreateConfigurationSource();

            var connectionString = new ConnectionStringSettings(
                OracleConnectionStringName,
                OracleConnectionString,
                OracleProviderName);

            var connectionStrings = new ConnectionStringsSection();
            connectionStrings.ConnectionStrings.Add(connectionString);

            configSource.Add("connectionStrings", connectionStrings);
            return configSource;
        }
    }
}
