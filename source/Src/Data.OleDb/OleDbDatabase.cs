﻿/*
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
using System.Data.OleDb;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.OleDb.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Properties;

namespace Microsoft.Practices.EnterpriseLibrary.Data.OleDb
{
    [ConfigurationElementType(typeof(OleDbDatabaseData))]
    public class OleDbDatabase : Database
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbDatabase"/>.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public OleDbDatabase(string connectionString)
            : base(connectionString, OleDbFactory.Instance)
        {

        }

        /// <summary>
        /// Determines whether the database provider supports parameter discovery. This depends on the underlying
        /// OLE DB provider.
        /// </summary>
        /// <value>Returns <b>true</b>, but you should consult the documentation for the underlying OLE DB provider.</value>
        /// <seealso cref="DeriveParameters(DbCommand)"/>
        public override bool SupportsParemeterDiscovery => true;

        /// <summary>
        /// Retrieves parameter information from the stored procedure specified in the <see cref="DbCommand"/> and
        /// populates the Parameters collection of the specified <see cref="DbCommand"/> object.
        /// </summary>
        /// <param name="discoveryCommand">The <see cref="DbCommand"/> to do the discovery.</param>
        /// <exception cref="InvalidCastException"><paramref name="discoveryCommand"/> is not an <see cref="OleDbCommand"/>.</exception>
        /// <exception cref="InvalidOperationException">The underlying OLE DB provider does not support returning stored
        /// procedure parameter information, the command text is not a valid stored procedure name, or the CommandType
        /// specified was not <see cref="CommandType.StoredProcedure" />.</exception>
        protected override void DeriveParameters(DbCommand discoveryCommand)
        {
            OleDbCommandBuilder.DeriveParameters((OleDbCommand)discoveryCommand);
        }

        ///<inheritdoc/>
        protected override bool SameNumberOfParametersAndValues(DbCommand command, object[] values)
        {
            OleDbParameter returnParam = command.Parameters.Cast<OleDbParameter>().FirstOrDefault(p => p.Direction == ParameterDirection.ReturnValue);
            int returnParameterCount = returnParam == null ? 0 : 1;
            int numberOfParametersToStoredProcedure = command.Parameters.Count - returnParameterCount;
            int numberOfValuesProvidedForStoredProcedure = values.Length;
            return numberOfParametersToStoredProcedure == numberOfValuesProvidedForStoredProcedure;
        }

        /// <inheritdoc/>
        protected override void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            ((OleDbDataAdapter)adapter).RowUpdated += OleDbDataAdapter_RowUpdated;
        }

        /// <summary>
        /// Listens for the RowUpdate event on a data adapter to support UpdateBehavior.Continue
        /// </summary>
        /// <param name="sender">The <see cref="OleDbDataAdapter"/> which raised the event</param>
        /// <param name="e">The event arguments.</param>
        private void OleDbDataAdapter_RowUpdated(object sender, OleDbRowUpdatedEventArgs e)
        {
            if (e.RecordsAffected == 0 && e.Errors != null)
            {
                e.Row.RowError = Resources.ExceptionMessageUpdateDataSetRowFailure;
                e.Status = UpdateStatus.SkipCurrentRow;
            }
        }

        ///<inheritdoc/>
        protected override int UserParametersStartIndex() => 1;
    }
}
