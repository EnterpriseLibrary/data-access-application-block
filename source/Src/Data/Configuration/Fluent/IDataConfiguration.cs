// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Starting point for data configuration.
    /// </summary>
    /// <seealso cref="DataConfigurationSourceBuilderExtensions"/>
    public interface IDataConfiguration : IDatabaseConfiguration
    {
        /// <summary>
        /// Specify a custom provider name or alias to use.  This must
        /// map to the name of the invariant name specified by <see cref="DbProviderFactories"/>
        /// </summary>
        /// <remarks>If the provider is not mapped to a specific Enterprise Library <see cref="Database"/> class, then the <see cref="GenericDatabase"/> will be used.</remarks>
        /// <param name="providerName">The name of the database provider's invariant.</param>
        /// <returns></returns>
        IDatabaseProviderConfiguration WithProviderNamed(string providerName);
    }

    /// <summary>
    /// Defines the mapping options for providers.
    /// </summary>
    public interface IDatabaseProviderConfiguration : IDataConfiguration
    {
        /// <summary>
        /// The <see cref="Database"/> to map the provider to.
        /// </summary>
        /// <param name="databaseType">The <see cref="Database"/> type.</param>
        /// <returns></returns>
        /// <seealso cref="GenericDatabase"/>
        IDataConfiguration MappedToDatabase(Type databaseType);

        /// <summary>
        /// The <see cref="Database"/> to map the provider to.
        /// </summary>
        /// <typeparam name="T">Database type to map to</typeparam>
        /// <returns></returns>
        /// <seealso cref="GenericDatabase"/>
        IDataConfiguration MappedToDatabase<T>() where T : Database;
    }
}
