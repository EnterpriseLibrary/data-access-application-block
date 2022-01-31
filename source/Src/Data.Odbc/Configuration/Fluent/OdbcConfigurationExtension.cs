// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Odbc;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Odbc.Configuration.Fluent
{


    internal class OdbcConfigurationExtension : DatabaseConfigurationExtension, IOdbcDatabaseConfiguration
    {
        public OdbcConfigurationExtension(IDatabaseConfigurationProviders context) : base(context)
        {
            base.ConnectionString.ProviderName = "System.Data.Odbc";
        }

        public IDatabaseConfigurationProperties WithConnectionString(OdbcConnectionStringBuilder builder)
        {
            return base.WithConnectionString(builder);
        }
    }
}
