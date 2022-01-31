// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.Fluent
{
    ///<summary>
    /// Provides extensions for Oracle database provider.
    ///</summary>
    public static class DatabaseProviderExtensions
    {

        ///<summary>
        /// An Oracle database for use with the Oracle.ManagedDataAccess.Client namespace.
        ///</summary>
        ///<returns></returns>
        ///<seealso cref="Oracle.ManagedDataAccess.Client"/>
        public static IDatabaseOracleConfiguration AnOracleDatabase(this IDatabaseConfigurationProviders context)
        {
            return new OracleConfigurationExtension(context);
        }
    }
}
