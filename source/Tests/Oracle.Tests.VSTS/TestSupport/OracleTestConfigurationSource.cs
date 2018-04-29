// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using EnterpriseLibrary.Common.Configuration;
using EnterpriseLibrary.Data.TestSupport;

namespace EnterpriseLibrary.Data.Oracle.Tests.TestSupport
{
    public static class OracleTestConfigurationSource
    {
        public const string OracleConnectionString = "server=entlib;user id=testuser;password=testuser";
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
