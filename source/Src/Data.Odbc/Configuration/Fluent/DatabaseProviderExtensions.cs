using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Odbc.Configuration.Fluent
{
    public static class DatabaseProviderExtensions
    {
        /// <summary>
        /// An ODBC database for use with the <see cref="System.Data.Odbc"/> namespace.
        /// </summary>
        /// <returns></returns>
        public static IOdbcDatabaseConfiguration AnOdbcDatabase(this IDatabaseConfigurationProviders context)
        {
            return new OdbcConfigurationExtension(context);
        }
    }
}
