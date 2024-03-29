// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// Provides parameter caching services for dynamic parameter discovery of stored procedures.
    /// Eliminates the round-trip to the database to derive the parameters and types when a command
    /// is executed more than once.
    /// </summary>
    public class ParameterCache
    {
        private CachingMechanism cache = new CachingMechanism();

        /// <summary>
        /// Populates the parameter collection for a command wrapper from the cache 
        /// or performs a round-trip to the database to query the parameters.
        /// </summary>
        /// <param name="command">
        /// <para>The command to add the parameters.</para>
        /// </param>
        /// <param name="database">
        /// <para>The database to use to set the parameters.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">One of the arguments is <b>null</b>.</exception>
        public void SetParameters(DbCommand command, Database database)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (database == null) throw new ArgumentNullException(nameof(database));


            if (AlreadyCached(command, database))
            {
                AddParametersFromCache(command, database);
            }
            else
            {
                database.DiscoverParameters(command);
                IDataParameter[] copyOfParameters = CreateParameterCopy(command);

                this.cache.AddParameterSetToCache(database.ConnectionString, command, copyOfParameters);
            }
        }

        /// <summary>
        /// Empties the parameter cache.
        /// </summary>
        protected internal void Clear()
        {
            this.cache.Clear();
        }

        /// <summary>
        /// Adds parameters to a command using the cache.
        /// </summary>
        /// <param name="command">
        /// <para>The command to add the parameters.</para>
        /// </param>
        /// <param name="database">The database to use.</param>
        protected virtual void AddParametersFromCache(DbCommand command, Database database)
        {
            IDataParameter[] parameters = this.cache.GetCachedParameterSet(database.ConnectionString, command);

            foreach (IDataParameter p in parameters)
            {
                command.Parameters.Add(p);
            }
        }

        /// <summary>
        /// Checks to see if a cache entry exists for a specific command on a specific connection.
        /// </summary>
        /// <param name="command">
        /// <para>The command to check.</para>
        /// </param>
        /// <param name="database">The database to check.</param>
        /// <returns>True if the parameters are already cached for the provided command, false otherwise</returns>
        private bool AlreadyCached(IDbCommand command, Database database)
        {
            return this.cache.IsParameterSetCached(database.ConnectionString, command);
        }

        private static IDataParameter[] CreateParameterCopy(DbCommand command)
        {
            IDataParameterCollection parameters = command.Parameters;
            IDataParameter[] parameterArray = new IDataParameter[parameters.Count];
            parameters.CopyTo(parameterArray, 0);

            return CachingMechanism.CloneParameters(parameterArray);
        }
    }
}
