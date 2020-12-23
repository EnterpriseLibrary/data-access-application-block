using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Properties;

namespace Data.Odbc
{
    /// <summary>
    /// Represents an ODBC data provider wrapper.
    /// </summary>
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
        /// <exception cref="InvalidCastException"><paramref name="discoveryCommand"/> is not an <see cref="OdbcCommand"/>.</exception>
        /// <exception cref="InvalidOperationException">The underlying ODBC provider does not support returning stored
        /// procedure parameter information, the command text is not a valid stored procedure name, or the CommandType
        /// specified was not <see cref="CommandType.StoredProcedure" />.</exception>
        protected override void DeriveParameters(DbCommand discoveryCommand)
        {
            OdbcCommandBuilder.DeriveParameters((OdbcCommand)discoveryCommand);
        }

        /// <summary>
        /// Determines whether the database provider supports parameter discovery. This depends on the underlying
        /// ODBC provider.
        /// </summary>
        /// <value>Returns <b>true</b>, but you should consult the documentation for the underlying ODBC provider.</value>
        /// <seealso cref="DeriveParameters(DbCommand)"/>
        public override bool SupportsParemeterDiscovery => true;

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
