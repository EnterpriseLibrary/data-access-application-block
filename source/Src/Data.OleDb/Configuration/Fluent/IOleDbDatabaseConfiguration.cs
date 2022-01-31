// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.OleDb;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.OleDb.Configuration.Fluent
{
    /// <summary>
    /// OleDb database configuration options.
    /// </summary>
    public interface IOleDbDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    {
        /// <summary>
        /// Define an OleDb connection with the <see cref="OleDbConnectionStringBuilder"/>
        /// </summary>
        /// <param name="builder">The connection string builder</param>
        /// <returns></returns>
        IDatabaseConfigurationProperties WithConnectionString(OleDbConnectionStringBuilder builder);
    }
}
