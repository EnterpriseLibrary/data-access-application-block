// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.Data.SqlCe;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.SqlCeDatabaseFixtures
{
    /// <summary>
    /// Tests the dataset execution capabilities of the sql ce provider for the data application block.
    /// </summary>
    [TestClass]
    public class ExecuteDataSetFixture : SqlCeDatabaseFixtureBase
    {
        public ExecuteDataSetFixture()
            : base("ConfigFiles.OlderTestsConfiguration.config")
        {
        }

        [TestInitialize()]
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// To test that a dataset be returned from the sql ce database by the executing the command through the sql ce provider and basic
        /// sql operations of insert, update and delete can be performed successfully.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset with the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void CrudOperationsAreSuccessful()
        {
            string commandString = "insert into items(itemid, itemdescription, price, qtyinhand, qtyrequired)  values (4, 'HyperDrive', 9876.54, 1234, 1000)";
            DataSet dsActualResult = db.ExecuteDataSetSql(commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(4, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
            Assert.AreEqual("HyperDrive", dsActualResult.Tables[0].Rows[3][0].ToString().Trim());
            Assert.AreEqual("9876.54", dsActualResult.Tables[0].Rows[3][1].ToString().Trim());

            commandString = "update items set itemdescription = 'klystron', price = 34567.67 where itemid = 4";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(4, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
            Assert.AreEqual("klystron", dsActualResult.Tables[0].Rows[3][0].ToString().Trim());
            Assert.AreEqual("34567.67", dsActualResult.Tables[0].Rows[3][1].ToString().Trim());

            commandString = "delete from items where itemid = 4";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset be returned from the sql ce database by the executing the command through the sql ce provider.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset with the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void CrudOperationsAreSuccessfulWhenParametersAreSet()
        {
            string commandString = "insert into items(itemid, itemdescription, price, qtyinhand, qtyrequired)  values (@Id, @Description, @Price, @Qh, @Qr)";
            SqlCeParameter[] paramInputs = new SqlCeParameter[5];
            paramInputs[0] = (SqlCeParameter)db.CreateParameter("@Id", DbType.Int16, 0, 4);
            paramInputs[1] = (SqlCeParameter)db.CreateParameter("@Description", DbType.String, 20, "HyperDrive");
            paramInputs[2] = (SqlCeParameter)db.CreateParameter("@Price", DbType.Decimal, 0, 9876.54);
            paramInputs[3] = (SqlCeParameter)db.CreateParameter("@Qh", DbType.Double, 0, 1234);
            paramInputs[4] = (SqlCeParameter)db.CreateParameter("@Qr", DbType.Double, 0, 1000);
            DataSet dsActualResult = db.ExecuteDataSetSql(commandString, paramInputs);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(4, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");
            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
            Assert.AreEqual("HyperDrive", dsActualResult.Tables[0].Rows[3][0].ToString().Trim());
            Assert.AreEqual("9876.54", dsActualResult.Tables[0].Rows[3][1].ToString().Trim());

            commandString = "update items set itemdescription = @Description, price = @Price where itemid = @Id";
            paramInputs = new SqlCeParameter[3];
            paramInputs[0] = (SqlCeParameter)db.CreateParameter("@Id", DbType.Int16, 0, 4);
            paramInputs[1] = (SqlCeParameter)db.CreateParameter("@Description", DbType.String, 20, "klystron");
            paramInputs[2] = (SqlCeParameter)db.CreateParameter("@Price", DbType.Decimal, 0, 34567.67);
            dsActualResult = db.ExecuteDataSetSql(commandString, paramInputs);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(4, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");
            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
            Assert.AreEqual("klystron", dsActualResult.Tables[0].Rows[3][0].ToString().Trim());
            Assert.AreEqual("34567.67", dsActualResult.Tables[0].Rows[3][1].ToString().Trim());

            commandString = "delete from items where itemid = @Id";
            paramInputs = new SqlCeParameter[1];
            paramInputs[0] = (SqlCeParameter)db.CreateParameter("@Id", DbType.Int16, 0, 4);
            dsActualResult = db.ExecuteDataSetSql(commandString, paramInputs);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");
            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset be returned from the sql ce database by executing the command through the sql ce provider and basic
        /// sql operations of insert, update and delete can be performed successfully.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset with the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void DataSetIsLoadedWheUsingTableName()
        {
            DataSet dsActualResult = new DataSet();

            string commandString = "Select ItemDescription, Price from Items order by ItemID";
            SqlCeCommand cmd = new SqlCeCommand(commandString);
            db.LoadDataSet(cmd, dsActualResult, "Items");

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables["Items"].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables["Items"].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables["Items"].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables["Items"].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables["Items"].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables["Items"].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset be returned from the sql ce database by the executing the command through the sql ce provider and basic
        /// sql operations of insert, update and delete can be performed successfully.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset with the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void DataSetIsLoadedWhenUsingMultipleTables()
        {
            DataSet dsActualResult = new DataSet();

            string commandString = "Select ItemDescription, Price from Items order by ItemID";
            SqlCeCommand cmd = new SqlCeCommand(commandString);
            db.LoadDataSet(cmd, dsActualResult, "Items");

            Assert.AreEqual<int>(3, dsActualResult.Tables["Items"].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables["Items"].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables["Items"].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables["Items"].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables["Items"].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables["Items"].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables["Items"].Rows[2][1].ToString().Trim());

            commandString = "select CustomerName, QtyOrdered from CustomersOrders order by CustomerID";
            cmd = new SqlCeCommand(commandString);
            db.LoadDataSet(cmd, dsActualResult, "Customers");

            Assert.AreEqual<int>(3, dsActualResult.Tables["Customers"].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Eastaff", dsActualResult.Tables["Customers"].Rows[0][0].ToString().Trim());
            Assert.AreEqual("100", dsActualResult.Tables["Customers"].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Tom", dsActualResult.Tables["Customers"].Rows[1][0].ToString().Trim());
            Assert.AreEqual("100", dsActualResult.Tables["Customers"].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Jack", dsActualResult.Tables["Customers"].Rows[2][0].ToString().Trim());
            Assert.AreEqual("100", dsActualResult.Tables["Customers"].Rows[2][1].ToString().Trim());

            Assert.AreEqual<int>(3, dsActualResult.Tables["Items"].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables["Items"].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables["Items"].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables["Items"].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables["Items"].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables["Items"].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables["Items"].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset be returned from the sql ce database by the executing the command through the sql ce provider with data definition
        /// statements
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset with the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void ExecuteDataSetCreatesTableWithDataDefinitionStatements()
        {
            string commandString = "create table TestTable(col111 int, col222 nvarchar(20), col333 float)";
            DataSet dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<bool>(true, db.TableExists("TestTable"));

            commandString = "drop table Testtable";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<bool>(false, db.TableExists("TestTable"));
        }

        /// <summary>
        /// To test that a dataset is  returned properly  from the sql ce database by the executing the command through the sql ce provider within a transaction that is not explicitly committed.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void UpdatedRecordIsNotSavedWhenTransactionNotCommitted()
        {
            string commandString = "Update Items Set Price = 5000 where ItemID = 2";

            SqlCeCommand cmd = (SqlCeCommand)db.GetSqlStringCommand(commandString);
            SqlCeConnection conn = (SqlCeConnection)db.CreateConnection();
            conn.Open();
            SqlCeTransaction trans = conn.BeginTransaction();

            DataSet dsActualResult = db.ExecuteDataSet(cmd, trans);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            cmd = (SqlCeCommand)db.GetSqlStringCommand(commandString);
            dsActualResult = db.ExecuteDataSet(cmd, trans);

            conn.Close();
            trans.Dispose();
            conn.Dispose();

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("5000", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset is  returned properly  from the sql ce database by the executing the command through the sql ce provider within a transaction that is  committed.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void UpdatedRecordIsSavedWhenTransactionCommitted()
        {
            string commandString = "Update Items Set Price = 5000 where ItemID = 2";

            SqlCeCommand cmd = (SqlCeCommand)db.GetSqlStringCommand(commandString);
            SqlCeConnection conn = (SqlCeConnection)db.CreateConnection();
            conn.Open();
            SqlCeTransaction trans = conn.BeginTransaction();

            DataSet dsActualResult = db.ExecuteDataSet(cmd, trans);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            cmd = (SqlCeCommand)db.GetSqlStringCommand(commandString);
            dsActualResult = db.ExecuteDataSet(cmd, trans);

            trans.Commit();

            conn.Close();
            trans.Dispose();
            conn.Dispose();

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("5000", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset is  returned properly  from the sql ce database by the executing the command through the sql ce provider within a transaction that is  committed.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void TableExists()
        {
            SqlCeDatabase db = new SqlCeDatabase("Data Source='TestDb.sdf'");

            Assert.IsTrue(db.TableExists("Credits"));
        }

        /// <summary>
        /// To test that a dataset that is returned from the sql ce database is rolledback by the executing the command through the sql ce provider when the transaction is rolled back.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsNotUpdatedWhenTransactionNotCommitted()
        {
            string commandString = "Update Items Set Price = 5000 where ItemID = 2";

            SqlCeCommand cmd = (SqlCeCommand)db.GetSqlStringCommand(commandString);
            SqlCeConnection conn = (SqlCeConnection)db.CreateConnection();
            conn.Open();
            SqlCeTransaction trans = conn.BeginTransaction();

            DataSet dsActualResult;

            try
            {
                dsActualResult = db.ExecuteDataSet(cmd, trans);

                commandString = "Update Items Set QtyinHand = 50000000000000000 where ItemID = 3";
                cmd = (SqlCeCommand)db.GetSqlStringCommand(commandString);
                dsActualResult = db.ExecuteDataSet(cmd, trans);

                Assert.Fail("Exception should have been thrown on update");

                trans.Commit();
            }
            catch { }

            conn.Close();
            trans.Dispose();
            conn.Dispose();

            commandString = "Select QtyinHand, Price from Items order by ItemID";

            dsActualResult = db.ExecuteDataSetSql(commandString);
            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("25", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("95", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("34", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset is  returned properly  from the sql ce database by the executing the command through the sql ce provider within a transaction that is not explicitly committed.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsUpdatedWhenTransactionCommittedInTransactionScope()
        {
            DataSet dsActualResult = null;
            using (TransactionScope tranScope = new TransactionScope())
            {
                string commandString = "Update Items Set Price = 5000 where ItemID = 2";

                dsActualResult = db.ExecuteDataSetSql(commandString);

                commandString = "Select ItemDescription, Price from Items order by ItemID";
                dsActualResult = db.ExecuteDataSetSql(commandString);

                tranScope.Complete();
            }

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("5000", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset that is returned from the sql ce database is rolled back by the executing the command through the sql ce provider when the transaction is rolled back.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void ChangesAreRolledbackWhenAnExceptionOccursInTransactionScope()
        {
            DataSet dsActualResult = new DataSet();
            string commandString;

            try
            {
                using (SqlCeConnection sConn = new SqlCeConnection("Data Source=TestDb.sdf"))
                {
                    using (TransactionScope tranScope = new TransactionScope())
                    {
                        sConn.Open();
                        SqlCeCommand cmd = new SqlCeCommand("Update Items Set Price = 5000 where ItemID = 2");
                        cmd.Connection = sConn;
                        SqlCeDataAdapter da = new SqlCeDataAdapter(cmd);
                        da.Fill(dsActualResult);

                        dsActualResult = new DataSet();
                        cmd = new SqlCeCommand("Update Items Set QtyinHand = 5000000000000000000 where ItemID = 3");
                        cmd.Connection = sConn;
                        da = new SqlCeDataAdapter(cmd);
                        da.Fill(dsActualResult);

                        dsActualResult = new DataSet();
                        cmd = new SqlCeCommand("Select ItemDescription, Price from Items order by ItemID");
                        cmd.Connection = sConn;
                        da = new SqlCeDataAdapter(cmd);
                        da.Fill(dsActualResult);

                        Assert.Fail("Exception should have been thrown");

                        tranScope.Complete();
                    }
                }
            }
            catch { }

            commandString = "Select QtyinHand, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSetSql(commandString);

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("25", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("95", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("34", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset is returned from the sql ce database by mentioning the command text
        /// and the command type to the sqlce facade of the data block and the all insert, delete and update statements are
        /// executed properly.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void CrudOperationsAreSuccessfulWhenCommandTextAndCommandType()
        {
            string commandString = "insert into items(itemid, itemdescription, price, qtyinhand, qtyrequired)  values (4, 'HyperDrive', 9876.54, 1234, 1000)";
            DataSet dsActualResult = db.ExecuteDataSet(CommandType.Text, commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSet(CommandType.Text, commandString);

            Assert.AreEqual<int>(4, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
            Assert.AreEqual("HyperDrive", dsActualResult.Tables[0].Rows[3][0].ToString().Trim());
            Assert.AreEqual("9876.54", dsActualResult.Tables[0].Rows[3][1].ToString().Trim());

            commandString = "update items set itemdescription = 'klystron', price = 34567.67 where itemid = 4";
            dsActualResult = db.ExecuteDataSet(CommandType.Text, commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSet(CommandType.Text, commandString);

            Assert.AreEqual<int>(4, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
            Assert.AreEqual("klystron", dsActualResult.Tables[0].Rows[3][0].ToString().Trim());
            Assert.AreEqual("34567.67", dsActualResult.Tables[0].Rows[3][1].ToString().Trim());

            commandString = "delete from items where itemid = 4";
            dsActualResult = db.ExecuteDataSet(CommandType.Text, commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSet(CommandType.Text, commandString);

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("98.95", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }
        /// <summary>
        /// To test that a dataset is returned from the sql ce database by mentioning the command text
        /// and the command type to the sqlce facade of the data block and the all insert, delete and update statements are
        /// executed properly.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsUpdatedWhenCommandTextAndCommandTypeWithCommittedTransaction()
        {
            string commandString = "Update Items Set Price = 5000 where ItemID = 2";

            SqlCeConnection conn = (SqlCeConnection)db.CreateConnection();
            conn.Open();
            SqlCeTransaction trans = conn.BeginTransaction();

            DataSet dsActualResult = db.ExecuteDataSet(trans, CommandType.Text, commandString);

           commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSet(trans, CommandType.Text, commandString);

           trans.Commit();

            conn.Close();
            trans.Dispose();
            conn.Dispose();

            Assert.AreEqual<int>(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("5000", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());
        }

        /// <summary>
        /// To test that a dataset that is returned from the sql ce database is rolledback by the executing the command through the sql ce provider when the transaction is rolled back.
        /// </summary>
        /// <expectedoutput>
        /// Proper dataset containing the expected data rows.
        /// </expectedoutput>
        [TestMethod]
        [DeploymentItem(@"TestDb.sdf")]
        public void RecordIsUpdatedWhenUsingCommandTextAndCommandTypeIBeforeRollback()
        {
            string commandString = "Update Items Set Price = 5000 where ItemID = 2";

            SqlCeConnection conn = (SqlCeConnection)db.CreateConnection();
            conn.Open();
            SqlCeTransaction trans = conn.BeginTransaction();

            DataSet dsActualResult = db.ExecuteDataSet(trans, CommandType.Text, commandString);

            commandString = "Select ItemDescription, Price from Items order by ItemID";
            dsActualResult = db.ExecuteDataSet(trans, CommandType.Text, commandString);

            Assert.AreEqual(3, dsActualResult.Tables[0].Rows.Count, "Mismatch in number of rows in the returned dataset. Problem with the test data or with the execute dataset.");

            Assert.AreEqual("Digital Image Pro", dsActualResult.Tables[0].Rows[0][0].ToString().Trim());
            Assert.AreEqual("38.95", dsActualResult.Tables[0].Rows[0][1].ToString().Trim());
            Assert.AreEqual("Excel 2003", dsActualResult.Tables[0].Rows[1][0].ToString().Trim());
            Assert.AreEqual("5000", dsActualResult.Tables[0].Rows[1][1].ToString().Trim());
            Assert.AreEqual("Infopath", dsActualResult.Tables[0].Rows[2][0].ToString().Trim());
            Assert.AreEqual("89", dsActualResult.Tables[0].Rows[2][1].ToString().Trim());

            trans.Rollback();

            conn.Close();
            trans.Dispose();
            conn.Dispose();
        }
    }
}

