// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Configuration;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.TestSupport
{
    public class TestConfigurationSource
    {
        public const string NorthwindDummyUser = "entlib";
        public const string NorthwindDummyPassword = "hdf7&834k(*KA";

        public static DictionaryConfigurationSource CreateConfigurationSource()
        {
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();

            DatabaseSettings settings = new DatabaseSettings();
            settings.DefaultDatabase = "Service_Dflt";

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
