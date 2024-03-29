// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent;
using Microsoft.Practices.EnterpriseLibrary.Data.Properties;

namespace Microsoft.Practices.EnterpriseLibrary.Common.Configuration
{
    /// <summary>
    /// Data configuration fluent interface extensions to <see cref="IConfigurationSourceBuilder"/>
    /// </summary>
    /// <seealso cref="ConfigurationSourceBuilder"/>
    public static class DataConfigurationSourceBuilderExtensions
    {
        /// <summary>
        /// Configure database connections for Enterprise Library.
        /// </summary>
        /// <param name="configurationSourceBuilderRoot">Source builder root that is extended.</param>
        /// <returns></returns>
        public static IDataConfiguration ConfigureData(this IConfigurationSourceBuilder configurationSourceBuilderRoot)
        {
            return new DataConfigurationBuilder(configurationSourceBuilderRoot);
        }

        private class DataConfigurationBuilder : IDatabaseConfigurationProviders,
                                                 IDatabaseProviderExtensionContext,
                                                 IDatabaseProviderConfiguration,
                                                 IDatabaseConfigurationProperties
        {
            private IConfigurationSourceBuilder Builder { get; set; }
            private readonly ConnectionStringsSection connectionStringSection = new ConnectionStringsSection();
            private ConnectionStringSettings currentDatabaseConnectionInfo;
            private DatabaseSettings currentDatabaseSection;
            private DbProviderMapping currentProviderMapping;

            public DataConfigurationBuilder(IConfigurationSourceBuilder builder)
            {
                Builder = builder;
                builder.AddSection("connectionStrings", connectionStringSection);
            }

            /// <summary>
            /// Configure a named database.
            /// </summary>
            /// <param name="databaseName">Name of database to configure</param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"><paramref name="databaseName"/> is <b>null</b> or empty.</exception>
            public IDatabaseConfigurationProperties ForDatabaseNamed(string databaseName)
            {
                if (string.IsNullOrEmpty(databaseName)) throw new ArgumentException(Properties.Resources.ExceptionStringNullOrEmpty, nameof(databaseName));

                ResetForNewDatabase(databaseName);
                connectionStringSection.ConnectionStrings.Add(currentDatabaseConnectionInfo);

                return this;
            }

            /// <summary>
            /// Set this database as the default one in the configuration.
            /// </summary>
            /// <returns></returns>
            public IDatabaseConfigurationProperties AsDefault()
            {
                EnsureDatabaseSettings();
                currentDatabaseSection.DefaultDatabase = currentDatabaseConnectionInfo.Name;
                return this;
            }

            /// <summary>
            /// Specify the type of database.
            /// </summary>
            public IDatabaseConfigurationProviders ThatIs
            {
                get { return this; }
            }

            /// <summary>
            /// Adds a mapping of the specified <paramref name="providerName"/> to the GenericDatabase.
            /// </summary>
            /// <param name="providerName">The provider name</param>
            /// <returns>This <see cref="DataConfigurationBuilder"/> instance</returns>
            /// <exception cref="ArgumentException"><paramref name="providerName"/> is empty.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="providerName"/> is <b>null</b>.</exception>
            public IDatabaseProviderConfiguration WithProviderNamed(string providerName)
            {
                Guard.ArgumentNotNullOrEmpty(providerName, "providerName");

                EnsureDatabaseSettings();
                currentProviderMapping = new DbProviderMapping();
                currentProviderMapping.Name = providerName;
                currentProviderMapping.DatabaseType = typeof(GenericDatabase);
                currentDatabaseSection.ProviderMappings.Add(currentProviderMapping);
                return this;
            }

            /// <summary>
            /// Map the provider alias to the specified database type.
            /// </summary>
            /// <param name="databaseType">Maps the provider to a type that derives from <see cref="Database"/></param>
            /// <returns>This <see cref="DataConfigurationBuilder"/> instance</returns>
            /// <exception cref="ArgumentException"><paramref name="databaseType"/> doesn't inherit
            /// from <see cref="Database"/>.</exception>
            public IDataConfiguration MappedToDatabase(Type databaseType)
            {
                if (!typeof(Database).IsAssignableFrom(databaseType))
                {
                    throw new ArgumentException(Resources.ExceptionArgumentMustInheritFromDatabase, nameof(databaseType));
                }

                currentProviderMapping.DatabaseType = databaseType;
                return this;
            }

            /// <summary>
            /// Map the provider alias to the specified database type.
            /// </summary>
            /// <typeparam name="T">The type to map to the provider</typeparam>
            /// <returns>This <see cref="DataConfigurationBuilder"/> instance</returns>
            public IDataConfiguration MappedToDatabase<T>() where T : Database
            {
                return MappedToDatabase(typeof(T));
            }


            ConnectionStringSettings IDatabaseProviderExtensionContext.ConnectionString
            {
                get { return currentDatabaseConnectionInfo; }
            }

            IConfigurationSourceBuilder IDatabaseProviderExtensionContext.Builder
            {
                get { return Builder; }
            }

            private void ResetForNewDatabase(string databaseName)
            {
                currentDatabaseConnectionInfo = new ConnectionStringSettings
                {
                    Name = databaseName,
                    ProviderName = DbProviderMapping.DefaultSqlProviderName,
                    ConnectionString = Data.Properties.Resources.DefaultSqlConnctionString
                };
            }

            private void EnsureDatabaseSettings()
            {
                if (currentDatabaseSection != null) return;
                currentDatabaseSection = new DatabaseSettings();
                Builder.AddSection(DatabaseSettings.SectionName, currentDatabaseSection);
            }

        }
    }
}
