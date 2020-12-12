using System;
using System.Data.Common;
using System.Data.OleDb;

namespace Microsoft.Practices.EnterpriseLibrary.Data.OleDb
{
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
        /// Retrieves parameter information from the stored procedure specified in the <see cref="DbCommand"/> and populates the Parameters collection of the specified <see cref="DbCommand"/> object. 
        /// </summary>
        /// <param name="discoveryCommand">The <see cref="DbCommand"/> to do the discovery.</param>
        /// <exception cref="InvalidCastException"><paramref name="discoveryCommand"/> is not an <see cref="OleDbCommand"/>.</exception>
        /// <exception cref="InvalidOperationException">The underlying OLE DB provider does not support returning stored
        /// procedure parameter information, the command text is not a valid stored procedure name, or the CommandType
        /// specified was not <see cref="CommandType.StoredProcedure" />".</exception>
        protected override void DeriveParameters(DbCommand discoveryCommand)
        {
            OleDbCommandBuilder.DeriveParameters((OleDbCommand)discoveryCommand);
        }
    }
}
