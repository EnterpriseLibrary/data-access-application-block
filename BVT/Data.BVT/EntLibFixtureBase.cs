// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    public class EntLibFixtureBase
    {
        private IConfigurationSource source;
        protected string connStr;

        public EntLibFixtureBase()
            : this(null, false, Assembly.GetCallingAssembly())
        {
        }

        public EntLibFixtureBase(string configSourceFileName)
            : this(configSourceFileName, false, Assembly.GetCallingAssembly())
        {
        }

        public EntLibFixtureBase(string configSourceFileName, bool useMultipleSources, Assembly resourceAssembly)
        {
            ResourceAssembly = resourceAssembly;

            ConfigurationSourceFileName = configSourceFileName;

            UseMultipleSources = useMultipleSources;
            ConfigurationSource = DoSetup();
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            if (ConfigurationSource != null)
            {
                ConfigurationSource.Dispose();
            }
        }

        public virtual void Initialize()
        {
            ConnectionStringsSection section = (ConnectionStringsSection)this.ConfigurationSource.GetSection("connectionStrings");
            connStr = section.ConnectionStrings["DataSQLTest"].ConnectionString;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connStr;
            conn.Open();
            SqlCommand cmd = new SqlCommand("begin Delete from Items where itemId > 3 " +
                                            "Update Items set ItemDescription='Digital Image Pro',QtyInHand = 25, QtyRequired=100 where ItemId = 1 " +
                                            "Update Items set ItemDescription='Excel 2003',QtyInHand = 95, QtyRequired=100 where ItemId = 2 " +
                                            "Update Items set ItemDescription='Infopath',QtyInHand = 34, QtyRequired=100 where ItemId = 3 " +
                                            "delete from CustomersOrders where CustomerId > 3 " +
                                            "Update CustomersOrders set QtyOrdered=100 " +
                                            "end", conn);

            SqlDataReader dr = cmd.ExecuteReader();
            dr.Close();
            conn.Close();
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);

        }

        protected string ConfigurationSourceFileName { get; private set; }

        protected Assembly ResourceAssembly { get; private set; }

        protected bool UseMultipleSources { get; private set; }

        protected IConfigurationSource ConfigurationSource
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }

        protected virtual IConfigurationSource DoSetup()
        {
            if (source == null)
            {
                if (ConfigurationSourceFileName == null)
                {
                    source = ConfigurationSourceFactory.Create();
                }
                else
                {
                    WriteEmbeddedFileToDisk(ResourceAssembly, ConfigurationSourceFileName);

                    if (!UseMultipleSources)
                    {
                        source = new FileConfigurationSource(ConfigurationSourceFileName);
                    }
                    else
                    {
                        ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                        fileMap.ExeConfigFilename = ConfigurationSourceFileName;

                        System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                        // Added support for multiple sources
                        ConfigurationSourceSection configurationSourceSection
                            = configuration.GetSection(ConfigurationSourceSection.SectionName) as ConfigurationSourceSection;

                        if (configurationSourceSection != null)
                        {
                            string systemSourceName = configurationSourceSection.SelectedSource;
                            if (!string.IsNullOrEmpty(systemSourceName))
                            {
                                ConfigurationSourceElement objectConfiguration
                                    = configurationSourceSection.Sources.Get(systemSourceName);

                                source = objectConfiguration.CreateSource();
                            }
                        }
                    }
                }
            }

            return source;
        }

        public static void WriteEmbeddedFileToDisk(Assembly assembly, string fileName)
        {
            WriteEmbeddedFileToDisk(assembly, fileName, fileName);
        }

        public static void WriteEmbeddedFileToDisk(Assembly assembly, string sourceFileName, string targetFileName)
        {
            //if file exists, delete it or it will be old...
            if (File.Exists(targetFileName))
            {
                File.Delete(targetFileName);
            }

            using (var stream = GetEmbeddedFileStream(assembly, sourceFileName))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                File.WriteAllBytes(targetFileName, bytes);
            }
        }

        public static Stream GetEmbeddedFileStream(Assembly assembly, string fileName)
        {
            string assemblyName = assembly.GetName().Name;

            string resourceName = assemblyName + "." + fileName;

            var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new ApplicationException(string.Format("Unable to find embedded resource: {0}", resourceName));
            }

            return stream;
        }

        protected virtual void CreateCommandParameter(Database db,
                                    DbCommand comm,
                                    string Parametername,
                                    DbType dbType,
                                    ParameterDirection dir,
                                    object value)
        {
            if (dir == ParameterDirection.Input)
            {
                db.AddInParameter(comm, Parametername, dbType, value);
            }
            else if (dir == ParameterDirection.Output)
            {
                db.AddOutParameter(comm, Parametername, dbType, (int)value);
            }
        }

        # region Helper Method

        protected virtual void CreateCommandParameter(Database db,
                                    DbCommand comm,
                                    string Parametername,
                                    DbType dbType,
                                    ParameterDirection dir,
                                    string SrcColumn,
                                    DataRowVersion rowVersion)
        {
            if (dir == ParameterDirection.Input)
            {
                db.AddInParameter(comm, Parametername, dbType, SrcColumn, rowVersion);
            }
        }

        # endregion
    }
}

