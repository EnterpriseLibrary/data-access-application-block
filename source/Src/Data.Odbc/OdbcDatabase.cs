/*
Copyright 2020 (c) Enterprise Library project

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Odbc.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Properties;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Odbc
{
    /// <summary>
    /// Represents an ODBC data provider wrapper.
    /// </summary>
    [ConfigurationElementType(typeof(OdbcDatabaseData))]
    public class OdbcDatabase : Database
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="OdbcDatabase"/> using the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        public OdbcDatabase(string connectionString) : base(connectionString, OdbcFactory.Instance)
        {
        }

        /// <summary>
        /// Retrieves parameter information from the stored procedure specified in the <see cref="DbCommand"/> and populates
        /// the Parameters collection of the specified <see cref="DbCommand"/> object.
        /// </summary>
        /// <param name="discoveryCommand">The <see cref="DbCommand"/> to do the discovery.</param>
        /// <exception cref="NotSupportedException">Always</exception>
        /// <remarks>While <see cref="OdbcCommandBuilder"/> supports deriving parameters, we don't know
        /// which specific database will be used, what are it's parameter discovery capabilities,
        /// and other differences. So we can't support deriving parameters automatically.</remarks>
        protected override void DeriveParameters(DbCommand discoveryCommand)
        {
            throw new NotSupportedException(Properties.Resources.ExceptionParameterDiscoveryNotSupportedOnOdbcDatabase);
            //To execute a procedure in ODBC you have to name it as "{ Call procName(?,?,?,?) }"
            //in the CommandText. But in this syntax, DeriveParameters doesn't work.
            //to derive parameters, you have to give just the procedure name in CommandText, but then
            //the ExecuteScalar (for example) doesn't work, complaining a parameter is missing.
        }

        /// <summary>
        /// Determines whether the database provider supports parameter discovery.
        /// </summary>
        /// <value>Returns <b>false</b></value>
        /// <remarks>Parameter discovery is not supported for ODBC drivers.</remarks>
        /// <seealso cref="DeriveParameters(DbCommand)"/>
        public override bool SupportsParemeterDiscovery => false;

        /// <inheritdoc/>
        protected override void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            ((OdbcDataAdapter)adapter).RowUpdated += OdbcDataAdapter_RowUpdated;
        }

        /// <summary>
        /// Listens for the RowUpdate event on a data adapter to support UpdateBehavior.Continue
        /// </summary>
        /// <param name="sender">The <see cref="OdbcDataAdapter"/> which raised the event</param>
        /// <param name="e">The event arguments.</param>
        private void OdbcDataAdapter_RowUpdated(object sender, OdbcRowUpdatedEventArgs e)
        {
            if (e.RecordsAffected == 0 && e.Errors != null)
            {
                e.Row.RowError = Resources.ExceptionMessageUpdateDataSetRowFailure;
                e.Status = UpdateStatus.SkipCurrentRow;
            }
        }
    }
}
