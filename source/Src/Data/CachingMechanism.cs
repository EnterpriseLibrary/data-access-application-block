// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Data;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// CachingMechanism provides caching support for stored procedure
    /// parameter discovery and caching
    /// </summary>
    internal class CachingMechanism
    {
        private Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Create and return a copy of the IDataParameter array.
        /// </summary>
        public static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
        {
            IDataParameter[] clonedParameters = new IDataParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        /// <summary>
        /// Empties all items from the cache
        /// </summary>
        public void Clear()
        {
            this.paramCache.Clear();
        }

        /// <summary>
        /// Add a parameter array to the cache for the command.
        /// </summary>
        public void AddParameterSetToCache(string connectionString, IDbCommand command, IDataParameter[] parameters)
        {
            string storedProcedure = command.CommandText;
            string key = CreateHashKey(connectionString, storedProcedure);
            this.paramCache[key] = parameters;
        }

        /// <summary>
        /// Gets a parameter array from the cache for the command. Returns null if no parameters are found.
        /// </summary>
        public IDataParameter[] GetCachedParameterSet(string connectionString, IDbCommand command)
        {
            string storedProcedure = command.CommandText;
            string key = CreateHashKey(connectionString, storedProcedure);
            IDataParameter[] cachedParameters = (IDataParameter[])(this.paramCache[key]);
            return CloneParameters(cachedParameters);
        }

        /// <summary>
        /// Gets if a given stored procedure on a specific connection string has a cached parameter set
        /// </summary>
        public bool IsParameterSetCached(string connectionString, IDbCommand command)
        {
            string hashKey = CreateHashKey(
                connectionString,
                command.CommandText);
            return this.paramCache[hashKey] != null;
        }

        private static string CreateHashKey(string connectionString, string storedProcedure)
        {
            return connectionString + ":" + storedProcedure;
        }
    }
}
