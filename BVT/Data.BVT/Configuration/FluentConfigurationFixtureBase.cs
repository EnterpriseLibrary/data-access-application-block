// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;
using EnterpriseLibrary.Common.Configuration;
using EnterpriseLibrary.Data.Configuration;
using EnterpriseLibrary.Data.Configuration.Fluent;
using EnterpriseLibrary.Data.Oracle.Configuration;
using EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class FluentConfigurationFixtureBase : EntLibFixtureBase
    {
        protected IDataConfiguration configurationStart;
        protected ConfigurationSourceBuilder builder;
        protected const string DatabaseName = "Database";
        protected const string DataSource = "local";
        protected const string InitialCatalog = "Northwind";
        protected const bool IntegratedSecurity = true;
        protected const string Driver = "{Microsoft Access Driver (*.mdb)}";
        protected const string Dbq = "Database.mdb";
        protected const string DataSourcePropertyName = "DataSource";
        protected const string Uid = "UserID";
        protected const string Pwd = "Password";
        protected const string DbqPropertyName = "Dbq";
        protected const string UidPropertyName = "Uid";
        protected const string PwdPropertyName = "Pwd";
        protected const string OdbcProviderName = "System.Data.Odbc";
        protected const bool OleDbPersistSecurityInfo = true;
        protected const string OleDbFileName = "Database.mdb";
        protected const string OleDbProvider = "Microsoft.Jet.OLEDB.4.0";
        protected const string OleDbDataSource = "Sample.mdb";
        protected const int OleDbServicesValue = 5;
        protected const string OleDbProviderName = "System.Data.OleDb";
        protected const string OracleDataSource = "tns";
        protected const string OracleUserID = "Admin";
        protected const string OraclePassword = "Password";
        protected const bool OracleIntegratedSecurity = false;
        protected const string OracleDatabaseName = "Oracle instance";
        protected const string PackageName = "Package";
        protected const string Prefix = "Prefix";
        protected const string SqlCeConnectionString = "bad info";
        protected const string SqlCeProviderName = "System.Data.SqlServerCe";
        protected const string GenericProviderName = "Generic Provider";
        protected const string GenericConnectionString = "Uid=UserID;Pwd=Password;DataSource=local";
        protected const string DefaultConnectionString = @"Database=Database;Server=(local)\SQLEXPRESS;Integrated Security=SSPI";
        protected DictionaryConfigurationSource configSource;

        public FluentConfigurationFixtureBase()
        {
            configSource = new DictionaryConfigurationSource();
        }

        protected void ConfigureContainer()
        {
            builder.UpdateConfigurationWithReplace(configSource);
        }

        protected T GetSettings<T>() where T : ConfigurationSection
        {
            builder.UpdateConfigurationWithReplace(configSource);

            FieldInfo field = typeof(T).GetField("SectionName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            var sectionName = (string)field.GetValue(null);

            return (T)configSource.GetSection(sectionName);
        }

        protected ConnectionStringsSection GetConnectionStringSettings()
        {
            builder.UpdateConfigurationWithReplace(configSource);

            return configSource.GetSection("connectionStrings") as ConnectionStringsSection;
        }

        public override void Initialize()
        {
            builder = new ConfigurationSourceBuilder();
            configurationStart = builder.ConfigureData();
        }
    }
}

