// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    public class DbAsyncState
    {
        public DbAsyncState(Database db)
        {
            Database = db;
            AutoResetEvent = new AutoResetEvent(false);
        }
        public DbAsyncState(Database db, object accessor)
        {
            Database = db;
            AutoResetEvent = new AutoResetEvent(false);
            Accessor = accessor;

        }
        public Database Database { get; set; }
        public Exception Exception { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }
        public object State { get; set; }
        public DaabAsyncResult AsyncResult { get; set; }
        public object Accessor { get; set; }
    }
}

