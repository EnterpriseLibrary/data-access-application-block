// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.BVT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.OracleDatabaseFixtures
{
    [TestClass]
    public class UpdateDataSetFixture : EntLibFixtureBase
    {
        public UpdateDataSetFixture()
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
            db.ExecuteNonQuery(CommandType.Text, "delete from Country where CountryCode in ('HOL','FRA','FIN','SWI','SIN','JAP', 'RUS')");
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        [TestMethod]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });

            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["CountryCode"] };

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "HOL";
            row["CountryName"] = "Holland";

            ds.Tables[0].Rows.Add(row);

            DataRow modifyRow = ds.Tables[0].Rows.Find("HOL");
            modifyRow["CountryName"] = "HOLLAND";

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, transaction);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Assert.Fail("Transaction Rolled back : " + e.Message);
                }
            }

            DataRow result = ds.Tables[0].Rows.Find("HOL");
            Assert.IsNotNull(result);
            Assert.AreEqual("HOLLAND", result["CountryName"].ToString());
        }

        [TestMethod]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionRollback()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            DbCommand dbAddEmployee = db.GetStoredProcCommand("AddEmployees");

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });

            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["CountryCode"] };

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "JAP";
            row["CountryName"] = "Japan";

            ds.Tables[0].Rows.Add(row);

            DataRow modifyRow = ds.Tables[0].Rows.Find("JAP");
            modifyRow["CountryName"] = "JAPAN";

            DataRow deleteRow = ds.Tables[0].Rows.Find("IN");
            deleteRow.Delete();

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, transaction);
                    db.UpdateDataSet(ds, "Employees", dbAddEmployee, null, null, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }

            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('IN','JAP')"))
            {
                dsResult.Tables[0].PrimaryKey = new DataColumn[] { dsResult.Tables[0].Columns["CountryCode"] };
                DataRow result = dsResult.Tables[0].Rows.Find("IN");
                Assert.IsNotNull(result);
                result = dsResult.Tables[0].Rows.Find("JAP");
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdSqlAndContinue()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            DbCommand dbAddEmployee = db.GetStoredProcCommand("AddEmployees");

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "SIN";
            row["CountryName"] = "Singapore";

            ds.Tables[0].Rows.Add(row);
            row = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(row);

            db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, UpdateBehavior.Continue);

            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('SIN')"))
            {
                dsResult.Tables[0].PrimaryKey = new DataColumn[] { dsResult.Tables[0].Columns["CountryCode"] };
                DataRow result = dsResult.Tables[0].Rows.Find("SIN");
                Assert.IsNotNull(result);
                Assert.AreEqual("Singapore", Convert.ToString(result["CountryName"]));
            }
        }

        [TestMethod]
        public void ChangesAreNotSavedWhenUpdateDataSetWithCmdStoredProcAndTransactionalBehavioural()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            DbCommand dbAddEmployee = db.GetStoredProcCommand("AddEmployees");

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "HOL";
            row["CountryName"] = "Holland";

            ds.Tables[0].Rows.Add(row);
            row = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(row);
            try
            {
                db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, UpdateBehavior.Transactional);
            }
            catch { }

            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('HOL')"))
            {
                Assert.IsNotNull(0 == dsResult.Tables[0].Rows.Count);
            }
        }

        [TestMethod]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndUpdateBehaviorStandard()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            DbCommand dbAddEmployee = db.GetStoredProcCommand("AddEmployees");

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });
            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["CountryCode"] };

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "FIN";
            row["CountryName"] = "Finland";

            ds.Tables[0].Rows.Add(row);
            DataRow modifyRow = ds.Tables[0].Rows.Find("FIN");
            modifyRow["CountryName"] = "FINLAND";
            try
            {
                db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, UpdateBehavior.Standard);
            }
            catch { }
            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('FIN')"))
            {
                Assert.IsTrue(1 == dsResult.Tables[0].Rows.Count);
                Assert.AreEqual("FINLAND", Convert.ToString(dsResult.Tables[0].Rows[0]["CountryName"]));
            }
        }

        [TestMethod]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndUpdateBehaviorStandardAndError()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            DbCommand dbAddEmployee = db.GetStoredProcCommand("AddEmployees");

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "SWI";
            row["CountryName"] = "SwitzerLand";

            ds.Tables[0].Rows.Add(row);
            row = ds.Tables[0].NewRow();
            ds.Tables[0].Rows.Add(row);

            try
            {
                db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, UpdateBehavior.Standard);
            }
            catch { }

            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('SWI')"))
            {
                Assert.IsTrue(1 == dsResult.Tables[0].Rows.Count);
                Assert.AreEqual("SwitzerLand", Convert.ToString(dsResult.Tables[0].Rows[0]["CountryName"]));
            }
        }

        [TestMethod]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcAndUpdateBehaviorTransactionalAndError()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommand("AddCountry");
            db.AddInParameter(dbAddCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbAddCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbUpdateCountry = db.GetStoredProcCommand("UpdateCountry");
            db.AddInParameter(dbUpdateCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);
            db.AddInParameter(dbUpdateCountry, "vCountryName", DbType.String, "CountryName", DataRowVersion.Current);
            DbCommand dbDeleteCountry = db.GetStoredProcCommand("DeleteCountry");
            db.AddInParameter(dbDeleteCountry, "vCountryCode", DbType.String, "CountryCode", DataRowVersion.Current);

            DbCommand dbAddEmployee = db.GetStoredProcCommand("AddEmployees");

            db.LoadDataSet(CommandType.Text, "select CountryCode,CountryName from Country", ds, new string[] { "Country" });

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "FRA";
            row["CountryName"] = "France";

            ds.Tables[0].Rows.Add(row);

            db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, UpdateBehavior.Transactional);

            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('FRA')"))
            {
                Assert.AreEqual("France", Convert.ToString(dsResult.Tables[0].Rows[0]["CountryName"]));
            }
        }

        [TestMethod]
        public void ChangesAreSavedWhenUpdateDataSetWithCmdStoredProcUsingParameterOverloadAndTransactionCommit()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DataSet ds = new DataSet();

            DbCommand dbAddCountry = db.GetStoredProcCommandWithSourceColumns("AddCountry", new string[] { "CountryCode", "CountryName" });
            DbCommand dbUpdateCountry = db.GetStoredProcCommandWithSourceColumns("UpdateCountry", new string[] { "CountryCode", "CountryName" });
            DbCommand dbDeleteCountry = db.GetStoredProcCommandWithSourceColumns("DeleteCountry", new string[] { "CountryCode" });

            db.LoadDataSet(CommandType.Text, "select * from Country", ds, new string[] { "Country" });

            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["CountryCode"] };

            DataRow row = ds.Tables[0].NewRow();
            row["CountryCode"] = "RUS";
            row["CountryName"] = "Russia";

            ds.Tables[0].Rows.Add(row);

            DataRow modifyRow = ds.Tables[0].Rows.Find("RUS");
            modifyRow["CountryName"] = "RUSSIA";

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.UpdateDataSet(ds, "Country", dbAddCountry, dbUpdateCountry, dbDeleteCountry, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }

            using (DataSet dsResult = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('RUS')"))
            {
                DataRow result = dsResult.Tables[0].Rows[0];
                Assert.IsNotNull(result);
                Assert.AreEqual("RUSSIA", Convert.ToString(result["CountryName"]));
            }
        }
    }
}

