// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.BVT;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle;
using Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.OracleDatabaseFixtures
{
    [TestClass]
    public class CommonFixture : EntLibFixtureBase
    {
        public CommonFixture()
            : base(@"ConfigFiles.OracleDatabaseFixture.config")
        {
        }

        [TestInitialize]
        public override void Initialize()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);
        }

        [TestCleanup()]
        public override void Cleanup()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            db.ExecuteNonQuery(CommandType.Text, "delete from SimpleDataType where Col1 in ('Data1', 'Data2','Data3')");
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        [TestMethod]
        public void CorrectDatabaseTypeIsReturnedWhenNamedDatabaseIsResolved()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            Assert.IsTrue(db is OracleDatabase);
        }

        [TestMethod]
        public void DictionaryConfigurationSourceCanConfigureFactoryAndExecuteScalar()
        {
            DictionaryConfigurationSource source = new DictionaryConfigurationSource();

            DatabaseSettings setting = new DatabaseSettings();
            setting.DefaultDatabase = "OracleTest";
            ConnectionStringsSection connectionSection = new ConnectionStringsSection();
            connectionSection.ConnectionStrings.Add(new ConnectionStringSettings("OracleTest",
                ConfigurationManager.AppSettings["oracleConnectionString"]
                , "System.Data.OracleClient"));
            source.Add("connectionStrings", connectionSection);
            source.Add("dataConfiguration", setting);
            OracleConnectionSettings oracleSetting = new OracleConnectionSettings();
            OracleConnectionData oracleData = new OracleConnectionData();
            oracleData.Name = "OracleTest";
            oracleSetting.OracleConnectionsData.Add(oracleData);
            source.Add("oracleConnectionSettings", oracleSetting);

            DatabaseProviderFactory factory = new DatabaseProviderFactory(source);
            Database db = factory.Create("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetProductName");
            db.AddInParameter(dbCommand, "vProductId", DbType.Int32, 1);
            db.AddOutParameter(dbCommand, "vResult", DbType.String, 100);
            db.ExecuteScalar(dbCommand);
            Assert.AreEqual("Product1", db.GetParameterValue(dbCommand, "vResult"));
        }

        [TestMethod]
        public void DataTypesAreAddedAsParametersAndReturnedInDataSet()
        {
            byte[] b = new byte[10] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2 };
            DateTime curDate = DateTime.Now;
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "AddSimpleDataType";
            DbCommand dbCommand = db.GetStoredProcCommand(spName);
            db.AddInParameter(dbCommand, "Col1", DbType.String, "Data1");
            db.AddInParameter(dbCommand, "Col2", DbType.String, "Char Col");
            db.AddInParameter(dbCommand, "Col3", DbType.Int32, 1);
            db.AddInParameter(dbCommand, "Col4", DbType.Int32, 2);
            db.AddInParameter(dbCommand, "Col5", DbType.DateTime, curDate);
            db.AddInParameter(dbCommand, "Col6", DbType.Int64, 100);
            db.AddInParameter(dbCommand, "Col7", DbType.Binary, b);
            db.AddInParameter(dbCommand, "Col8", DbType.String, "NVarchar");
            db.AddInParameter(dbCommand, "Col9", DbType.String, "NChar");
            db.AddInParameter(dbCommand, "Col10", DbType.Double, 12.256);
            db.AddInParameter(dbCommand, "Col11", DbType.Int16, 1234);
            db.AddInParameter(dbCommand, "Col12", DbType.Int32, 120000);
            db.AddInParameter(dbCommand, "Col13", DbType.Int32, 120000);
            db.AddInParameter(dbCommand, "Col14", DbType.Double, 1234.6789);
            db.AddInParameter(dbCommand, "Col15", DbType.Int32, 1234);
            db.AddInParameter(dbCommand, "Col16", DbType.String, "V");

            db.ExecuteNonQuery(dbCommand);

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from SimpleDataType where Col1='Data1'"))
            {
                Assert.IsTrue(1 == ds.Tables[0].Rows.Count);
                Assert.AreEqual("Data1", ds.Tables[0].Rows[0]["Col1"].ToString().Trim());
                Assert.AreEqual("Char Col", ds.Tables[0].Rows[0]["Col2"].ToString().Trim());
                Assert.AreEqual(1, Convert.ToInt32(ds.Tables[0].Rows[0]["Col3"]));
                Assert.AreEqual(2, Convert.ToInt32(ds.Tables[0].Rows[0]["Col4"]));
                Assert.AreEqual(curDate.ToString(), Convert.ToDateTime(ds.Tables[0].Rows[0]["Col5"]).ToString());
                Assert.AreEqual(100, Convert.ToInt32(ds.Tables[0].Rows[0]["Col6"]));
                Assert.AreEqual(Convert.ToBase64String(b), Convert.ToBase64String((byte[])ds.Tables[0].Rows[0]["Col7"]));
                Assert.AreEqual("NVarchar", Convert.ToString(ds.Tables[0].Rows[0]["Col8"]));
                Assert.AreEqual("NChar     ", Convert.ToString(ds.Tables[0].Rows[0]["Col9"]));
                Assert.AreEqual(12.256, Convert.ToDouble(ds.Tables[0].Rows[0]["Col10"]));
                Assert.AreEqual(1234, Convert.ToInt32(ds.Tables[0].Rows[0]["Col11"]));
                Assert.AreEqual(120000, Convert.ToInt32(ds.Tables[0].Rows[0]["Col12"]));
                Assert.AreEqual(120000, Convert.ToInt32(ds.Tables[0].Rows[0]["Col13"]));
                Assert.AreEqual(1234.6789, Convert.ToDouble(ds.Tables[0].Rows[0]["Col14"]));
                Assert.AreEqual(1234, Convert.ToInt32(ds.Tables[0].Rows[0]["Col15"]));
                Assert.AreEqual("V", Convert.ToString(ds.Tables[0].Rows[0]["Col16"]));
            }
        }

        [TestMethod]
        public void GuidIsPassedAsPrameterAndReturnedAsOutput()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            Guid myVal = new Guid("33333333333333333333333333444444");
            string spName = "sp_GUIDTEST";
            DbCommand dbCommand = db.GetStoredProcCommand(spName);
            db.AddInParameter(dbCommand, "vGuidInput", DbType.Guid);
            db.SetParameterValue(dbCommand, "vGuidInput", myVal);
            db.AddParameter(dbCommand, "vGuidOutput", DbType.Guid, ParameterDirection.Output, "", DataRowVersion.Current, null);
            db.ExecuteNonQuery(dbCommand);
            Guid outputVal = (Guid)db.GetParameterValue(dbCommand, "vGuidOutput");
            Assert.IsTrue(myVal.Equals(outputVal));
        }

        [TestMethod]
        public void StoredProcedureParameterDiscoverySucceedsWhenUsingDbCommand()
        {
            byte[] b = new byte[10] { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2 };
            DateTime curDate = DateTime.Now;
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "AddSimpleDataType";
            DbCommand dbCommand = db.GetStoredProcCommand(spName, new object[] { "Data2", "Char Col", 1, 2, curDate, 100, b, "NVarchar", "NChar", 12.256, 1234, 120000, 120000, 1234.6789, 1234, "V" });

            db.ExecuteNonQuery(dbCommand);

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from SimpleDataType where Col1='Data2'"))
            {
                Assert.IsTrue(1 == ds.Tables[0].Rows.Count);
                Assert.AreEqual("Data2", ds.Tables[0].Rows[0]["Col1"].ToString().Trim());
                Assert.AreEqual("Char Col", ds.Tables[0].Rows[0]["Col2"].ToString().Trim());
                Assert.AreEqual(1, Convert.ToInt32(ds.Tables[0].Rows[0]["Col3"]));
                Assert.AreEqual(2, Convert.ToInt32(ds.Tables[0].Rows[0]["Col4"]));
                Assert.AreEqual(curDate.ToString(), Convert.ToDateTime(ds.Tables[0].Rows[0]["Col5"]).ToString());
                Assert.AreEqual(100, Convert.ToInt32(ds.Tables[0].Rows[0]["Col6"]));
                Assert.AreEqual(Convert.ToBase64String(b), Convert.ToBase64String((byte[])ds.Tables[0].Rows[0]["Col7"]));
                Assert.AreEqual("NVarchar", Convert.ToString(ds.Tables[0].Rows[0]["Col8"]));
                Assert.AreEqual("NChar     ", Convert.ToString(ds.Tables[0].Rows[0]["Col9"]));
                Assert.AreEqual(12.256, Convert.ToDouble(ds.Tables[0].Rows[0]["Col10"]));
                Assert.AreEqual(1234, Convert.ToInt32(ds.Tables[0].Rows[0]["Col11"]));
                Assert.AreEqual(120000, Convert.ToInt32(ds.Tables[0].Rows[0]["Col12"]));
                Assert.AreEqual(120000, Convert.ToInt32(ds.Tables[0].Rows[0]["Col13"]));
                Assert.AreEqual(1234.6789, Convert.ToDouble(ds.Tables[0].Rows[0]["Col14"]));
                Assert.AreEqual(1234, Convert.ToInt32(ds.Tables[0].Rows[0]["Col15"]));
                Assert.AreEqual("V", Convert.ToString(ds.Tables[0].Rows[0]["Col16"]));
            }
        }

        [TestMethod]
        public void OutParametersAreReturnedWhenUsingOracleSpecificPackage()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "GetProductDetailsById";
            DbCommand dbCommand = db.GetStoredProcCommand(spName);
            db.AddInParameter(dbCommand, "vProductId", DbType.Int32, 1);
            db.AddOutParameter(dbCommand, "vProductName", DbType.String, 20);
            db.AddOutParameter(dbCommand, "vUnitPrice", DbType.Int32, 5);
            db.ExecuteNonQuery(dbCommand);
            Assert.AreEqual("Product1", db.GetParameterValue(dbCommand, "vProductName").ToString());
            Assert.AreEqual(1000, Convert.ToInt32(db.GetParameterValue(dbCommand, "vUnitPrice")));
        }

        [TestMethod]
        public void OutParameterIsReturnedWhenUsingOracleSpecificPackage()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleDefaultPackageTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetProductName");
            db.AddInParameter(dbCommand, "vProductId", DbType.Int32, 1);
            db.AddOutParameter(dbCommand, "vResult", DbType.String, 100);
            db.ExecuteScalar(dbCommand);
            Assert.AreEqual("Product1", db.GetParameterValue(dbCommand, "vResult"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenOraclePackageIsNull()
        {
            OracleDatabase db = new OracleDatabase("server=bg1016-ent.redmond.corp.microsoft.com;user id=testone;password=testone", null);
        }

        [TestMethod]
        [ExpectedException(typeof(OracleException))]
        public void ExceptionIsThrownWhenInvalidProcedureName()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "AddCountryInvalidSP";
            db.ExecuteNonQuery(spName, new object[] { "AUS", "Australia" });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionIsThrownWhenInvalidDatabaseInstance()
        {
            Database db = DatabaseFactory.CreateDatabase("InvalidInstanceName");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenNullCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = null;
            db.ExecuteScalar(dbCommand);
        }

        [TestMethod]
        [ExpectedException(typeof(OracleException))]
        public void ExceptionIsThrownWhenInvalidNumberOfParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetProductName");
            db.AddInParameter(dbCommand, "vProductId", DbType.Int32, 1);
            db.ExecuteNonQuery(dbCommand);
        }

        [TestMethod]
        [ExpectedException(typeof(OracleException))]
        public void ExceptionIsThrownWhenInvalidDataType()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetProductName");
            db.AddInParameter(dbCommand, "vProductId", DbType.String, 1);
            db.AddOutParameter(dbCommand, "vResult", DbType.Int32, 50);
            db.ExecuteNonQuery(dbCommand);
        }

        [TestMethod]
        [ExpectedException(typeof(OracleException))]
        public void ExceptionIsThrownWhenInvalidConnectionString()
        {
            Database db = DatabaseFactory.CreateDatabase("InvalidConnectionString");
            string countryName = (string)db.ExecuteScalar(CommandType.Text, "select CountryName from Country where CountryCode='IN'");
            Assert.AreEqual("India", countryName);
        }

        [TestMethod]
        public void ParametersAreAddedWhenUsingDbCommand()
        {
            OracleDatabase db = (OracleDatabase)DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCmd = db.GetStoredProcCommand("AddCountryInvalidSP");
            db.AddInParameter(dbCmd, "Param", DbType.Int32, 10);
            db.AddInParameter(dbCmd, "Param", DbType.Int32, 20);
            Assert.AreEqual(2, dbCmd.Parameters.Count);
            Assert.AreEqual(10, (int)db.GetParameterValue(dbCmd, "Param"));
            DbCommand dbCmd1 = db.GetSqlStringCommand("select * from InvalidType");
            Guid guid = Guid.NewGuid();
            db.AddInParameter(dbCmd1, "Param1", DbType.Guid, guid);
            Assert.AreEqual(guid, (Guid)db.GetParameterValue(dbCmd1, "Param1"));
            DbCommand dbCmd2 = db.GetSqlStringCommand("select * from InvalidType");
            db.AddInParameter(dbCmd2, "Param3", DbType.Boolean, true);
            Assert.AreEqual(true, (bool)db.GetParameterValue(dbCmd2, "Param3"));
        }
    }
}

