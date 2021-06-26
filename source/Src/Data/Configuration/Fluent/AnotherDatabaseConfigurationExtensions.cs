// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Common.Properties;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Configuration extensions for database types specified via the <see cref="DatabaseProviderExtensions.AnotherDatabaseType" />.
    /// </summary>
    public interface IDatabaseAnotherDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    {
        /// <summary>
        /// Connection string to use for this data source.
        /// </summary>
        /// <param name="builder">The connection string builder for the database provider</param>
        /// <returns>This instance</returns>
        IDatabaseAnotherDatabaseConfiguration WithConnectionString(DbConnectionStringBuilder builder);
    }

    internal class AnotherDatabaseConfigurationExtensions : DatabaseConfigurationExtension, IDatabaseAnotherDatabaseConfiguration
    {
        public AnotherDatabaseConfigurationExtensions(IDatabaseConfigurationProviders context, string providerName) : base(context)
        {
            if (String.IsNullOrEmpty(providerName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, nameof(providerName));
            ConnectionString.ProviderName = providerName;
        }

        IDatabaseAnotherDatabaseConfiguration IDatabaseAnotherDatabaseConfiguration.WithConnectionString(DbConnectionStringBuilder builder)
        {
            base.WithConnectionString(builder);
            return this;
        }
    }
}
