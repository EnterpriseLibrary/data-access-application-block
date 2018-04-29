// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using EnterpriseLibrary.Common.Configuration;
using EnterpriseLibrary.Data.Configuration;
using System.Configuration;

namespace Data.SqlCe.Tests.VSTS
{
    public class TestConfigurationSource
    {
        public static DictionaryConfigurationSource CreateConfigurationSource()
        {
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();

            DatabaseSettings settings = new DatabaseSettings();
            settings.DefaultDatabase = "SqlCeTestConnection";

            ConnectionStringsSection section = new ConnectionStringsSection();
            section.ConnectionStrings.Add(new ConnectionStringSettings("SqlCeTestConnection", "Data Source='testdb.sdf'", "System.Data.SqlServerCe.4.0"));

            source.Add(DatabaseSettings.SectionName, settings);
            source.Add("connectionStrings", section);

            return source;
        }
    }
}
