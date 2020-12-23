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
