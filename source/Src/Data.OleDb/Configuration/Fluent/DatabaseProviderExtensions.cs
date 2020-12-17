// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.OleDb.Configuration.Fluent
{
    public static class DatabaseProviderExtensions
    {
        /// <summary>
        /// An OleDb database for use with the <see cref="System.Data.OleDb"/> namespace.
        /// </summary>
        /// <returns></returns>
        public static IOleDbDatabaseConfiguration AnOleDbDatabase(this IDatabaseConfigurationProviders context)
        {
            return new OleDbConfigurationExtension(context);
        }
    }
}
