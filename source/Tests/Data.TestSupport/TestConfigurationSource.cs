// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
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
            var source = new DictionaryConfigurationSource();

            
            var connectionStringSettings = new ConnectionStringSettings(
                "Service_Dflt", 
                "Data Source=localhost;Initial Catalog=TestDb;Integrated Security=True;", 
                "System.Data.SqlClient" 
            );

            var connectionStringsSection = new ConnectionStringsSection();
            connectionStringsSection.ConnectionStrings.Add(connectionStringSettings);

            source.Add("connectionStrings", connectionStringsSection);

            
            var settings = new DatabaseSettings
            {
                DefaultDatabase = "Service_Dflt"
            };
            settings.ProviderMappings.Add(
                new DbProviderMapping(DbProviderMapping.DefaultSqlProviderName, typeof(SqlDatabase))
            );

            
            var oracleConnectionSettings = new OracleConnectionSettings();
            var data = new OracleConnectionData
            {
                Name = "OracleTest"
            };
            data.Packages.Add(new OraclePackageData("TESTPACKAGE", "TESTPACKAGETOTRANSLATE"));
            oracleConnectionSettings.OracleConnectionsData.Add(data);

            
            source.Add(DatabaseSettings.SectionName, settings);
            source.Add(OracleConnectionSettings.SectionName, oracleConnectionSettings);
            source.Add("connectionStrings", connectionStringsSection); // Important: register this

            return source;
        }
    }
}
