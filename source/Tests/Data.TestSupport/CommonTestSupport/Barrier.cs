// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Practices.EnterpriseLibrary.Common.TestSupport
{
    public class Barrier
    {
        private readonly object lockObj = new object();
        private readonly int originalCount;
        private int currentCount;

        public Barrier(int count)
        {
            originalCount = count;
            currentCount = count;
        }

        public void Await()
        {
            Await(TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        public void Await(int timeoutInMs)
        {
            Await(TimeSpan.FromMilliseconds(timeoutInMs));
        }

        public void Await(TimeSpan timeout)
        {
            lock(lockObj)
            {
                if(currentCount > 0)
                {
                    --currentCount;

                    if(currentCount == 0)
                    {
                        Monitor.PulseAll(lockObj);
                        currentCount = originalCount;
                    }
                    else
                    {
                        if(!Monitor.Wait(lockObj, timeout))
                        {
                            throw new TimeoutException();
                        }
                    }
                }
            }
        }
    }
}
