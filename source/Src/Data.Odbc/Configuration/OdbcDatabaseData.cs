using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;

namespace Data.Odbc.Configuration
{
    public class OdbcDatabaseData : DatabaseData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OdbcDatabaseData"/> class with the specified
        /// <paramref name="connectionStringSettings"/> and <paramref name="configurationSource"/> delegate.
        /// </summary>
        /// <param name="connectionStringSettings">The connection string configuration</param>
        /// <param name="configurationSource">A delegate to retrieve the configuration section</param>
        protected OdbcDatabaseData(ConnectionStringSettings connectionStringSettings, Func<string, ConfigurationSection> configurationSource)
            : base(connectionStringSettings, configurationSource)
        {
        }

        /// <inheritdoc/>
        public override Database BuildDatabase() => new OdbcDatabase(ConnectionString);
    }
}
