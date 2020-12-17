// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Common.Configuration
{
    ///<summary>
    /// Provides extensions for common database providers.
    ///</summary>
    public static class DatabaseProviderExtensions
    {

        /// <summary>
        /// An Odbc database for use with the <see cref="System.Data.Odbc"/> namespace.
        /// </summary>
        /// <returns></returns>
        public static IOdbcDatabaseConfiguration AnOdbcDatabase(this IDatabaseConfigurationProviders context)
        {
            return new OdbcConfigurationExtension(context);
        }

        ///<summary>
        /// A database with the specified database provider name.
        ///</summary>
        /// <param name="context">Extension context for fluent-interface</param>
        /// <param name="providerName">The provider name to use for this database connection</param>
        ///<returns></returns>
        /// <seealso cref="DbProviderFactories"/>
        public static IDatabaseAnotherDatabaseConfiguration AnotherDatabaseType(this IDatabaseConfigurationProviders context, string providerName)
        {
            return new AnotherDatabaseConfigurationExtensions(context, providerName);
        }
    }
}
