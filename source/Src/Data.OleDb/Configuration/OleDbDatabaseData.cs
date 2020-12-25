/*
Copyright 2020 (c) Enterprise Library project

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.OleDb.Configuration
{
    public class OleDbDatabaseData : DatabaseData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbDatabaseData"/> class
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings</param>
        /// <param name="configurationSource">A delegate to retrieve the configuration section</param>
        public OleDbDatabaseData(ConnectionStringSettings connectionStringSettings, Func<string, ConfigurationSection> configurationSource)
            : base(connectionStringSettings, configurationSource)
        {
        }

        public override Database BuildDatabase() => new OleDbDatabase(ConnectionString);
    }
}
