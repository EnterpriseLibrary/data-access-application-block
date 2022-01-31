using System;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.SqlCe.Configuration.Fluent
{
    /// <summary>
    /// Extension methods for <see cref="IDatabaseConfigurationProviders"/>
    /// </summary>
    public static class DatabaseProviderExtensions
    {

        /// <summary>
        /// A Sql CE database for use with the System.Data.SqlServerCe namespace.
        /// </summary>
        /// <param name="context">Configuration context</param>
        /// <returns>A new <see cref="SqlCeDatabaseConfigurationExtension"/> with the specified <paramref name="context"/>.</returns>
        public static IDatabaseSqlCeDatabaseConfiguration ASqlCeDatabase(this IDatabaseConfigurationProviders context)
        {
            return new SqlCeDatabaseConfigurationExtension(context);
        }
    }
}
