// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// An implementation of <see cref="IDataReader"/> which also properly
    /// cleans up the reference count on the given inner <see cref="DatabaseConnectionWrapper"/>
    /// when the reader is closed or disposed.
    /// </summary>
    public class RefCountingDataReader : DataReaderWrapper
    {
        private readonly DatabaseConnectionWrapper connectionWrapper;

        /// <summary>
        /// Create a new <see cref='RefCountingDataReader'/> that wraps the given
        /// <paramref name="innerReader"/> and properly cleans the refcount on the given
        /// <paramref name="connection"/> when done.
        /// </summary>
        /// <param name="connection">Connection to close.</param>
        /// <param name="innerReader">Reader to do the actual work.</param>
        public RefCountingDataReader(DatabaseConnectionWrapper connection, IDataReader innerReader)
            : base(innerReader)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));
            Guard.ArgumentNotNull(innerReader, nameof(innerReader));

            connectionWrapper = connection;
            connectionWrapper.AddRef();
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.IDataReader"/> Object.
        /// </summary>
        public override void Close()
        {
            if (!IsClosed)
            {
                base.Close();
                connectionWrapper.Dispose();
            }
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        /// <param name="disposing">True if called from dispose, false if called from finalizer. We have no finalizer,
        /// so this will never be false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsClosed)
                {
                    base.Dispose(true);
                    connectionWrapper.Dispose();
                }
            }
        }
    }
}
