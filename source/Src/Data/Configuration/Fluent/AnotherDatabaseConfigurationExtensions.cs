// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using EnterpriseLibrary.Common.Properties;
using EnterpriseLibrary.Common.Configuration;

namespace EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Configuration extensions for database types specified via the <see cref="DatabaseProviderExtensions.AnotherDatabaseType" />.
    /// </summary>
    public interface IDatabaseAnotherDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    {
        /// <summary />
        IDatabaseAnotherDatabaseConfiguration WithConnectionString(System.Data.Common.DbConnectionStringBuilder builder);
    }

    internal class AnotherDatabaseConfigurationExtensions : DatabaseConfigurationExtension, IDatabaseAnotherDatabaseConfiguration
    {
        public AnotherDatabaseConfigurationExtensions(IDatabaseConfigurationProviders context, string providerName) : base(context)
        {
            if (String.IsNullOrEmpty(providerName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "providerName");
            ConnectionString.ProviderName = providerName;
        }

        IDatabaseAnotherDatabaseConfiguration IDatabaseAnotherDatabaseConfiguration.WithConnectionString(DbConnectionStringBuilder builder)
        {
            base.WithConnectionString(builder);
            return this;
        }
    }
}
