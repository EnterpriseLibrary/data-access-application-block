// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Odbc;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Odbc.Configuration.Fluent
{
    /// <summary>
    /// Odbc database configuration options.
    /// </summary>
    public interface IOdbcDatabaseConfiguration : IDatabaseDefaultConnectionString, IDatabaseConfigurationProperties
    {
        /// <summary>
        /// Define a connection string with the <see cref="OdbcConnectionStringBuilder"/>.
        /// </summary>
        /// <param name="builder">The ODBC connection string builder</param>
        /// <returns>A Database configuration properties object</returns>
        IDatabaseConfigurationProperties WithConnectionString(OdbcConnectionStringBuilder builder);
    }
}
