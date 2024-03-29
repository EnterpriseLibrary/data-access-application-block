// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace Microsoft.Practices.EnterpriseLibrary.Data.TestSupport
{
    public static class TestConfigurationSource
    {
        public const string NorthwindDummyUser = "entlib";
        public const string NorthwindDummyPassword = "hdf7&834k(*KA";

        public static DictionaryConfigurationSource CreateConfigurationSource()
        {
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();

            DatabaseSettings settings = new DatabaseSettings();
            settings.DefaultDatabase = "Service_Dflt";
            DbProviderMapping mapping = new DbProviderMapping(DbProviderMapping.DefaultSqlProviderName, typeof(SqlDatabase));
            settings.ProviderMappings.Add(mapping);

            OracleConnectionSettings oracleConnectionSettings = new OracleConnectionSettings();
            OracleConnectionData data = new OracleConnectionData();
            data.Name = "OracleTest";
            data.Packages.Add(new OraclePackageData("TESTPACKAGE", "TESTPACKAGETOTRANSLATE"));
            oracleConnectionSettings.OracleConnectionsData.Add(data);

            source.Add(DatabaseSettings.SectionName, settings);
            source.Add(OracleConnectionSettings.SectionName, oracleConnectionSettings);

            return source;
        }
    }
}
