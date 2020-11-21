// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Tests.TestSupport
{
    internal class EnvironmentHelper
    {
        private static bool? oracleClientIsInstalled;
        private static string oracleClientNotInstalledErrorMessage;

        public static void AssertOracleClientIsInstalled()
        {
            if (!oracleClientIsInstalled.HasValue)
            {
                try
                {
                    var factory = new DatabaseProviderFactory(OracleTestConfigurationSource.CreateConfigurationSource());
                    var db = factory.Create("OracleTest");
                    var conn = db.CreateConnection();
                    conn.Open();
                    conn.Close();
                }
                catch (Exception ex) when (ex.Message != null
                    && ex.Message.Contains(DbProviderMapping.DefaultOracleProviderName)
                    && ex.Message.Contains("8.1.7"))
                {
                    oracleClientIsInstalled = false;
                    oracleClientNotInstalledErrorMessage = ex.Message;
                }
            }

            if (oracleClientIsInstalled.HasValue && oracleClientIsInstalled.Value == false)
            {
                Assert.Inconclusive(oracleClientNotInstalledErrorMessage);
            }
        }
    }
}
