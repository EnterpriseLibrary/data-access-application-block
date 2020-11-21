// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.Fluent
{
    /// <summary>
    /// Oracle package configuration options.
    /// </summary>
    public interface IDatabaseOraclePackageConfiguration : IFluentInterface
    {
        /// <summary>
        /// Define the prefix for the Oracle package.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        IDatabaseConfigurationProperties AndPrefix(string prefix);
    }
}
