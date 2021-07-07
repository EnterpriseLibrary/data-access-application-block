// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// This class represents an asynchronous operation invoked from the <see cref="Database"/> class methods.
    /// </summary>
    public class DaabAsyncResult : IAsyncResult
    {
        readonly IAsyncResult innerAsyncResult;

        /// <summary>
        /// Construct a new <see cref="DaabAsyncResult"/> instance.
        /// </summary>
        /// <param name="innerAsyncResult">The <see cref='IAsyncResult'/> object returned from the underlying
        /// async operation.</param>
        /// <param name="command">Command that was executed.</param>
        /// <param name="disposeCommand">Should the command be disposed at EndInvoke time?</param>
        /// <param name="closeConnection">Should this connection be closed at EndInvoke time?</param>
        /// <param name="startTime">Time operation was invoked.</param>
        public DaabAsyncResult(
            IAsyncResult innerAsyncResult,
            DbCommand command,
            bool disposeCommand,
            bool closeConnection,
            DateTime startTime)
        {
            this.innerAsyncResult = innerAsyncResult;
            this.Command = command;
            this.DisposeCommand = disposeCommand;
            this.CloseConnection = closeConnection;
            this.StartTime = startTime;
        }

        /// <summary>
        /// The state object passed to the callback.
        /// </summary>
        public object AsyncState => innerAsyncResult.AsyncState;

        /// <summary>
        /// Wait handle to use to wait synchronously for completion.
        /// </summary>
        public WaitHandle AsyncWaitHandle => innerAsyncResult.AsyncWaitHandle;

        /// <summary>
        /// <b>true</b> if begin operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously => innerAsyncResult.CompletedSynchronously;

        /// <summary>
        /// Has the operation finished?
        /// </summary>
        public bool IsCompleted => innerAsyncResult.IsCompleted;

        /// <summary>
        /// The underlying <see cref="IAsyncResult"/> object.
        /// </summary>
        public IAsyncResult InnerAsyncResult => innerAsyncResult;

        /// <summary>
        /// Should the command be disposed by the End method?
        /// </summary>
        public bool DisposeCommand { get; }

        /// <summary>
        /// The command that was executed.
        /// </summary>
        public DbCommand Command { get; }

        /// <summary>
        /// Should the connection be closed by the End method?
        /// </summary>
        public bool CloseConnection { get; }

        /// <summary>
        /// Connection the operation was invoked on.
        /// </summary>
        public DbConnection Connection => Command.Connection;

        /// <summary>
        /// Time the operation was started.
        /// </summary>
        public DateTime StartTime { get; }
    }
}
