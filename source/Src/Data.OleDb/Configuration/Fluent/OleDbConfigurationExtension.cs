// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.OleDb;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.OleDb.Configuration.Fluent
{

    internal class OleDbConfigurationExtension : DatabaseConfigurationExtension, IOleDbDatabaseConfiguration
    {
        public OleDbConfigurationExtension(IDatabaseConfigurationProviders context)
            : base(context)
        {
            ConnectionString.ProviderName = "System.Data.OleDb";
        }

        public IDatabaseConfigurationProperties WithConnectionString(OleDbConnectionStringBuilder builder)
        {
            return base.WithConnectionString(builder);
        }
    }
}
