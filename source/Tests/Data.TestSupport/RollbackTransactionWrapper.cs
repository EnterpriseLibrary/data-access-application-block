// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;

namespace EnterpriseLibrary.Data.TestSupport
{
    /// <summary>
    /// Used by a few test fixtures to simplify the code to rollback a transaction
    /// </summary>
    public class RollbackTransactionWrapper : IDisposable
    {
        private DbTransaction transaction;

        public RollbackTransactionWrapper(DbTransaction transaction)
        {
            this.transaction = transaction;
        }

        public void Dispose()
        {
            transaction.Rollback();
        }

        public DbTransaction Transaction
        {
            get { return transaction; }
        }
    }
}

