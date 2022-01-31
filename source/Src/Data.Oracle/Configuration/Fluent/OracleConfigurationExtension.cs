// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Practices.EnterpriseLibrary.Common.Properties;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.Fluent
{


    internal class OracleConfigurationExtension : DatabaseConfigurationExtension,
                                                  IDatabaseOraclePackageConfiguration,
                                                  IDatabaseOracleConfiguration
    {
        private OracleConnectionSettings currentOracleSettings;
        private OraclePackageData currentOraclePackageData;
        private OracleConnectionData currentOracleConnectionData;

        public OracleConfigurationExtension(IDatabaseConfigurationProviders context) : base(context)
        {
            ConnectionString.ProviderName = DbProviderMapping.DefaultOracleProviderName;
        }

        IDatabaseOracleConfiguration IDatabaseOracleConfiguration.WithConnectionString(string connectionString)
        {
            base.WithConnectionString(connectionString);
            return this;
        }

#pragma warning disable 612, 618
        IDatabaseOracleConfiguration IDatabaseOracleConfiguration.WithConnectionString(OracleConnectionStringBuilder builder)
        {
            base.WithConnectionString(builder);
            return this;
        }
#pragma warning restore 612, 618

        /// <summary>
        /// Adds the specified <paramref name="prefix"/> to the package
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="prefix"/> is <b>null</b> or empty.</exception>
        IDatabaseConfigurationProperties IDatabaseOraclePackageConfiguration.AndPrefix(string prefix)
        {
            if (String.IsNullOrEmpty(prefix))
                throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, nameof(prefix));

            currentOraclePackageData.Prefix = prefix;
            return this;
        }

        /// <summary>
        /// Adds a package with the specified <paramref name="name"/> to the configuration.
        /// </summary>
        public IDatabaseOraclePackageConfiguration WithPackageNamed(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, nameof(name));


            EnsureOracleSettings();
            EnsureOracleConnectionData();

            currentOraclePackageData = new OraclePackageData() { Name = name };
            currentOracleConnectionData.Packages.Add(currentOraclePackageData);

            return this;
        }

        private void EnsureOracleSettings()
        {
            currentOracleSettings = Builder.Get<OracleConnectionSettings>(OracleConnectionSettings.SectionName);
            if (currentOracleSettings != null) return;
            currentOracleSettings = new OracleConnectionSettings();
            Builder.AddSection(OracleConnectionSettings.SectionName, currentOracleSettings);
        }

        private void EnsureOracleConnectionData()
        {
            if (currentOracleConnectionData != null) return;
            currentOracleConnectionData = new OracleConnectionData() { Name = ConnectionString.Name };
            currentOracleSettings.OracleConnectionsData.Add(currentOracleConnectionData);
        }
    }
}
