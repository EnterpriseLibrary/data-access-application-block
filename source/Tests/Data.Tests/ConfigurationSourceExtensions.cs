using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Tests
{
    internal static class ConfigurationSourceExtensions
    {
        internal static void AddOracleDatabaseProviderMapping(this IConfigurationSource source)
        {
            var dbSettings = new DatabaseSettings();
            DbProviderMapping mapping = new DbProviderMapping(DbProviderMapping.DefaultOracleProviderName, typeof(OracleDatabase));
            dbSettings.ProviderMappings.Add(mapping);
            source.Add(DatabaseSettings.SectionName, dbSettings);
        }
    }
}
