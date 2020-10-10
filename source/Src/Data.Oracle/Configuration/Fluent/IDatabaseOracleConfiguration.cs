// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Oracle.ManagedDataAccess.Client;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.Fluent
{
    /// <summary>
    /// Oracle configuration options
    /// </summary>
    public interface IDatabaseOracleConfiguration : IDatabaseConfigurationProperties
    {
        /// <summary>
        /// Define an Oracle connection with a connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IDatabaseOracleConfiguration WithConnectionString(string connectionString);

        /// <summary>
        /// Define an Oracle connection with the <see cref="OracleConnectionStringBuilder"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
#pragma warning disable 612, 618
        IDatabaseOracleConfiguration WithConnectionString(OracleConnectionStringBuilder builder);
#pragma warning restore 612, 618

        /// <summary>
        /// Define an Oracle package with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDatabaseOraclePackageConfiguration WithPackageNamed(string name);

    }
}
