// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Design;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration
{
    /// <summary>
    /// Oracle-specific configuration section.
    /// </summary>
    [ResourceDescription(typeof(DesignResources),nameof(DesignResources.OracleConnectionSettingsDescription))]
    [ResourceDisplayName(typeof(DesignResources), nameof(DesignResources.OracleConnectionSettingsDisplayName))]
    public class OracleConnectionSettings : SerializableConfigurationSection
    {
        private const string oracleConnectionDataCollectionProperty = "";

        /// <summary>
        /// The section name for the <see cref="OracleConnectionSettings"/>.
        /// </summary>
        public const string SectionName = "oracleConnectionSettings";

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleConnectionSettings"/> class with default values.
        /// </summary>
        public OracleConnectionSettings()
        {
        }

        /// <summary>
        /// Retrieves the <see cref="OracleConnectionSettings"/> from the configuration source.
        /// </summary>
        /// <param name="configurationSource">The configuration source to retrieve the configuration from.</param>
        /// <returns>The configuration section, or <see langword="null"/> (<b>Nothing</b> in Visual Basic) 
        /// if not present in the configuration source.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="configurationSource"/> is <b>null</b>.</exception>
        public static OracleConnectionSettings GetSettings(IConfigurationSource configurationSource)
        {
            if (configurationSource == null) throw new ArgumentNullException(nameof(configurationSource));

            return configurationSource.GetSection(SectionName) as OracleConnectionSettings;
        }

        /// <summary>
        /// Collection of Oracle-specific connection information.
        /// </summary>
        [ConfigurationProperty(oracleConnectionDataCollectionProperty, IsRequired=false, IsDefaultCollection=true)]
        [ConfigurationCollection(typeof(OracleConnectionData))]
        [ResourceDescription(typeof(DesignResources), nameof(DesignResources.OracleConnectionSettingsOracleConnectionsDataDescription))]
        [ResourceDisplayName(typeof(DesignResources), nameof(DesignResources.OracleConnectionSettingsOracleConnectionsDataDisplayName))]
        public NamedElementCollection<OracleConnectionData> OracleConnectionsData
        {
            get { return (NamedElementCollection<OracleConnectionData>)base[oracleConnectionDataCollectionProperty]; }
        }
    }
}
