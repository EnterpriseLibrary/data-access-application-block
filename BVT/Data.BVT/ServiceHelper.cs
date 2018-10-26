// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    public static class ServiceHelper
    {
        public static void Stop(string serviceName)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "net";
                process.StartInfo.Arguments = "stop " + serviceName;
                process.Start();
                process.WaitForExit(30000);
            }
        }

        public static void Start(string serviceName)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "net";
                process.StartInfo.Arguments = "start " + serviceName;
                process.Start();
                process.WaitForExit(30000);
            }

        }

        public static void Restart(string serviceName)
        {
            Restart(serviceName, 0);

        }

        public static void Restart(string serviceName, int seconds)
        {
            Start(serviceName);
            System.Threading.Thread.Sleep(seconds);
            Stop(serviceName);

        }
    }
}

