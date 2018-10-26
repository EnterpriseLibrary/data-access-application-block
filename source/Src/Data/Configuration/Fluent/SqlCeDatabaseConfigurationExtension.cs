// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{

    /// <summary>
    /// SqlCe database configuration options.
    /// </summary>
    public interface IDatabaseSqlCeDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    { }


    internal class SqlCeDatabaseConfigurationExtension : DatabaseConfigurationExtension, IDatabaseSqlCeDatabaseConfiguration
    {
        public SqlCeDatabaseConfigurationExtension(IDatabaseConfigurationProviders context)
            : base(context)
        {
            base.ConnectionString.ProviderName = "System.Data.SqlServerCe";
        }
    }
}
