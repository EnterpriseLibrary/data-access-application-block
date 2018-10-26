// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.OleDb;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// OleDb database configuration options.
    /// </summary>
    public interface IOleDbDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    {
        /// <summary>
        /// Define an OleDb connection with the <see cref="OleDbConnectionStringBuilder"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        IDatabaseConfigurationProperties WithConnectionString(OleDbConnectionStringBuilder builder);
    }

    internal class OleDbConfigurationExtension : DatabaseConfigurationExtension, IOleDbDatabaseConfiguration
    {
        public OleDbConfigurationExtension(IDatabaseConfigurationProviders context)
            : base(context)
        {
            base.ConnectionString.ProviderName = "System.Data.OleDb";
        }

        public IDatabaseConfigurationProperties WithConnectionString(OleDbConnectionStringBuilder builder)
        {
            return base.WithConnectionString(builder);
        }
    }
}
