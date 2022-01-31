using System;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Odbc.Configuration.Fluent
{
    ///<summary>
    /// Provides extensions for ODBC database provider.
    ///</summary>
    public static class DatabaseProviderExtensions
    {
        /// <summary>
        /// An ODBC database for use with the <see cref="System.Data.Odbc"/> namespace.
        /// </summary>
        /// <returns></returns>
        public static IOdbcDatabaseConfiguration AnOdbcDatabase(this IDatabaseConfigurationProviders context)
        {
            return new OdbcConfigurationExtension(context);
        }
    }
}
