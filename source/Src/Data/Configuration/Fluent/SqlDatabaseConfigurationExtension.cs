// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.SqlClient;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{

    /// <summary>
    /// Sql Server Database configuration options.
    /// </summary>
    public interface IDatabaseSqlDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    {
        /// <summary>
        /// Define a connection string using the <see cref="SqlConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        IDatabaseConfigurationProperties WithConnectionString(SqlConnectionStringBuilder builder);
    }

    internal class SqlDatabaseConfigurationExtension : DatabaseConfigurationExtension, IDatabaseSqlDatabaseConfiguration
    {

        public SqlDatabaseConfigurationExtension(IDatabaseConfigurationProviders context)
            : base(context)
        {
            base.ConnectionString.ProviderName = DbProviderMapping.DefaultSqlProviderName;
        }

        public IDatabaseConfigurationProperties WithConnectionString(SqlConnectionStringBuilder builder)
        {
            return base.WithConnectionString(builder);
        }
    }
}
