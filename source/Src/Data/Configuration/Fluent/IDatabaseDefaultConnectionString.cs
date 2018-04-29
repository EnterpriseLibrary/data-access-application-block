// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using System.Data.Common;
using EnterpriseLibrary.Common;

namespace EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Defines default connection string settings for fluent-style interface.
    /// </summary>
    public interface IDatabaseDefaultConnectionString : IFluentInterface
    {
        /// <summary>
        /// Connection string to use for this data source.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        /// <seealso cref="ConnectionStringSettings"/>
        /// <seealso cref="DbConnectionStringBuilder"/>
        IDatabaseConfigurationProperties WithConnectionString(string connectionString);
    }
}
