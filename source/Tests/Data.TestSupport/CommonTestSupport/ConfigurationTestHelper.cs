// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Configuration;
using System.IO;
using EnterpriseLibrary.Common.Configuration;

namespace EnterpriseLibrary.Common.TestSupport.Configuration
{
    public static class ConfigurationTestHelper
    {
        public static string ConfigurationFileName = "test.exe.config";

        public static IConfigurationSource SaveSectionsAndReturnConfigurationSource(IDictionary<string, ConfigurationSection> sections)
        {
            System.Configuration.Configuration configuration
                = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            SaveSections(configuration, sections);

            return new SystemConfigurationSource(false);
        }

        public static IConfigurationSource SaveSectionsInFileAndReturnConfigurationSource(IDictionary<string, ConfigurationSection> sections)
        {
            System.Configuration.Configuration configuration
                = GetConfigurationForCustomFile(ConfigurationFileName);

            SaveSections(configuration, sections);

            return GetConfigurationSourceForCustomFile(ConfigurationFileName);
        }

        public static IConfigurationSource GetConfigurationSourceForCustomFile(string fileName)
        {
            return new FileConfigurationSource(fileName, false);
        }

        public static System.Configuration.Configuration GetConfigurationForCustomFile(string fileName)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = fileName;
            File.SetAttributes(fileMap.ExeConfigFilename, FileAttributes.Normal);
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        private static void SaveSections(System.Configuration.Configuration configuration,
                                    IDictionary<string, ConfigurationSection> sections)
        {
            foreach (string sectionName in sections.Keys)
            {
                configuration.Sections.Remove(sectionName);
                configuration.Sections.Add(sectionName, sections[sectionName]);
            }

            configuration.Save();
        }
    }
}
