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
    public class ExecuteNonQueryFixture : EntLibFixtureBase
    {
        public ExecuteNonQueryFixture()
            : base(@"ConfigFiles.OracleDatabaseFixture.config")
        {
        }

        [TestInitialize]
        public void Intialize()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory(), false);
        }

        [TestCleanup()]
        public override void Cleanup()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            db.ExecuteNonQuery(CommandType.Text, "delete from Country where CountryCode in ('AUS','UK','CHI')");
            DatabaseFactory.ClearDatabaseProviderFactory();
            base.Cleanup();
        }

        [TestMethod]
        public void CrudOperationsAreSuccessfulWhenCommandText()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            int DebitID = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select max(DebitId) from Debits"));
            DebitID += 1;

            string sqlText = "Insert into Debits (DebitID,AccountID,Amount) values ( " + DebitID + ",5,5000 )";
            db.ExecuteNonQuery(CommandType.Text, sqlText);

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from Debits where DebitID = " + DebitID))
            {
                Assert.AreEqual(5, Convert.ToInt32(ds.Tables[0].Rows[0]["AccountID"]));
                Assert.AreEqual(5000, Convert.ToInt32(ds.Tables[0].Rows[0]["Amount"]));
            }

            sqlText = "Update Debits SET AccountID=5000 where DebitId = " + DebitID;
            db.ExecuteNonQuery(CommandType.Text, sqlText);
            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from Debits where DebitID = " + DebitID))
            {
                Assert.AreEqual(5000, Convert.ToInt32(ds.Tables[0].Rows[0]["AccountID"]));
            }

            sqlText = "delete from Debits where DebitID = " + DebitID;
            db.ExecuteNonQuery(CommandType.Text, sqlText);
            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from Debits where DebitID = " + DebitID))
            {
                Assert.IsTrue(ds.Tables[0].Rows.Count == 0);
            }
        }

        [TestMethod]
        public void RecordIsInsertedWhenStoredProcWithParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "AddCountry";
            db.ExecuteNonQuery(spName, new object[] { "AUS", "Australia" });

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode='AUS'"))
            {
                Assert.IsTrue(1 == ds.Tables[0].Rows.Count);
                Assert.AreEqual("Australia", ds.Tables[0].Rows[0]["CountryName"].ToString().Trim());
            }
        }

        [TestMethod]
        public void RecordIsInsertedWhenDbCommandStoredProc()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetSqlStringCommand("insert into Country values ('CHI','China')");

            db.ExecuteNonQuery(dbCommand);

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode='CHI'"))
            {
                Assert.IsTrue(1 == ds.Tables[0].Rows.Count);
                Assert.AreEqual("China", ds.Tables[0].Rows[0]["CountryName"].ToString().Trim());
            }
        }

        [TestMethod]
        public void RecordIsInsertedWhenDbCommandStoredProcAndAddParameters()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string spName = "AddCountry";
            DbCommand dbCommand = db.GetStoredProcCommand(spName);
            db.AddInParameter(dbCommand, "vCountryCode", DbType.String);
            db.AddInParameter(dbCommand, "vCountryName", DbType.String);
            db.SetParameterValue(dbCommand, "vCountryCode", "UK");
            db.SetParameterValue(dbCommand, "vCountryName", "United Kingdom");

            db.ExecuteNonQuery(dbCommand);

            using (DataSet ds = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode='UK'"))
            {
                Assert.IsTrue(1 == ds.Tables[0].Rows.Count);
                Assert.AreEqual("United Kingdom", ds.Tables[0].Rows[0]["CountryName"].ToString().Trim());
            }
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingStoredProcOutParameterWithDbCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetProductName");
            db.AddInParameter(dbCommand, "vProductId", DbType.Int32, 1);
            db.AddOutParameter(dbCommand, "vResult", DbType.String, 50);
            db.ExecuteNonQuery(dbCommand);
            string productName = (string)db.ExecuteScalar(CommandType.Text, "select ProductName from Products where ProductId=1");
            Assert.AreEqual(productName, Convert.ToString(db.GetParameterValue(dbCommand, "vResult")));
        }

        [TestMethod]
        public void RecordsAreSavedWhenUsingSqlTextAndTransactionIsCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            int debitId = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select max(DebitId),count(DebitID) from Debits"));

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    debitId = debitId + 1;
                    string query = "INSERT INTO DEBITS ( DebitId, AccountId, Amount )  VALUES ( " + debitId.ToString() + " , 5, 5000 )";
                    db.ExecuteNonQuery(transaction, CommandType.Text, query);
                    debitId += 1;
                    query = "INSERT INTO DEBITS ( DebitId, AccountId, Amount )  VALUES ( " + debitId.ToString() + " , 6, 6000 )";
                    db.ExecuteNonQuery(transaction, CommandType.Text, query);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    Assert.Fail("The transaction is rolled back.");
                }
                finally
                {
                    connection.Close();
                }

                string result = "select * from Debits where DebitID in ( " + (debitId - 1) + "," + debitId + ")";
                using (DataSet ds = db.ExecuteDataSet(CommandType.Text, result))
                {
                    Assert.IsTrue(2 == ds.Tables[0].Rows.Count);
                    foreach (DataRow drResult in ds.Tables[0].Rows)
                    {
                        if (Convert.ToInt32(drResult["DebitId"]) == (debitId - 1))
                        {
                            Assert.AreEqual(5, Convert.ToInt32(drResult["AccountID"]));
                            Assert.AreEqual(5000, Convert.ToInt32(drResult["Amount"]));
                        }
                        else
                        {
                            Assert.AreEqual(6, Convert.ToInt32(drResult["AccountID"]));
                            Assert.AreEqual(6000, Convert.ToInt32(drResult["Amount"]));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void RecordIsNotInsertedWhenTransactionIsRolledbackUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            int debitId = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select max(DebitId),count(DebitID) from Debits"));

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    debitId = debitId + 1;
                    string query = "INSERT INTO DEBITS ( DebitId, AccountId, Amount )  VALUES ( " + debitId.ToString() + " , 5, 5000 )";
                    db.ExecuteNonQuery(transaction, CommandType.Text, query);
                    db.ExecuteNonQuery(transaction, CommandType.Text, query);
                    transaction.Commit();
                    Assert.Fail("Exception should have been thrown");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
                finally
                {
                    connection.Close();
                }
                string result = "select * from Debits where DebitID in ( " + debitId + "," + (debitId + 1) + ")";

                using (DataSet ds = db.ExecuteDataSet(CommandType.Text, result))
                {
                    Assert.IsTrue(0 == ds.Tables[0].Rows.Count);
                }
            }
        }

        [TestMethod]
        public void RecordsAreChangedWhenStoredProcCommandAndTransactionIsCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbCommand updateEmployee = db.GetStoredProcCommand("UpdateEmployees");
                DbCommand deleteEmployee = db.GetStoredProcCommand("DeleteEmployees");

                db.AddInParameter(updateEmployee, "vEmployeeId", DbType.Int32, 1);
                db.AddInParameter(updateEmployee, "vFirstName", DbType.String, "First Name");
                db.AddInParameter(updateEmployee, "vLastName", DbType.String, "Last Name");
                db.AddInParameter(updateEmployee, "vAge", DbType.Int32, 20);

                db.AddInParameter(deleteEmployee, "vEmployeeId", DbType.Int32, 101);

                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(updateEmployee, transaction);
                    db.ExecuteNonQuery(deleteEmployee, transaction);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    Assert.Fail("Transaction Rolled back");
                }
                finally
                {
                    connection.Close();
                }

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Employees where EmployeeId in (1,101) order by employeeid"))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 1);
                    Assert.AreEqual("First Name", result.Tables[0].Rows[0]["FirstName"]);
                    Assert.AreEqual("Last Name", result.Tables[0].Rows[0]["LastName"]);
                    Assert.AreEqual(20, Convert.ToInt32(result.Tables[0].Rows[0]["Age"]));
                }
            }
        }

        [TestMethod]
        public void RecordsAreNotChangedWhenStoredProcCommandAndTransactionIsRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbCommand deleteEmployee = db.GetStoredProcCommand("DeleteEmployees");
                DbCommand addEmployee = db.GetStoredProcCommand("AddEmployeesGetCount");

                db.AddInParameter(addEmployee, "vEmployeeId", DbType.Int32, 1);
                db.AddInParameter(addEmployee, "vFirstName", DbType.String, "Anonymous");
                db.AddInParameter(addEmployee, "vLastName", DbType.String, "Anonymous");
                db.AddInParameter(addEmployee, "vAge", DbType.Int32, 50);
                db.AddOutParameter(addEmployee, "vEmployeeCount", DbType.Int32, 50);

                db.AddInParameter(deleteEmployee, "vEmployeeId", DbType.Int32, 107);

                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(deleteEmployee, transaction);
                    db.ExecuteNonQuery(addEmployee, transaction);
                    transaction.Commit();
                    Assert.Fail("Exception should have been thrown.");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
                finally
                {
                    connection.Close();
                }

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Employees where EmployeeId in (1,107) order by employeeid"))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 2);
                    Assert.IsFalse("Anonymous" == (string)result.Tables[0].Rows[0]["FirstName"]);
                    Assert.IsFalse("Anonymous" == (string)result.Tables[0].Rows[0]["LastName"]);
                    Assert.IsFalse(50 == Convert.ToInt32(result.Tables[0].Rows[0]["Age"]));
                }
            }
        }

        [TestMethod]
        public void RecordsAreChangedWhenStoredProcTextAndTransactionIsCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(transaction, "AddCountry", new object[] { "SA", "South Africa" });
                    db.ExecuteNonQuery(transaction, "DeleteCountry", new object[] { "SA" });
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    Assert.Fail("Transaction Rolled back");
                }
                finally
                {
                    connection.Close();
                }

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode = 'SA'"))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 0);
                }
            }
        }

        [TestMethod]
        public void RecordIsNotSavedWhenStoredProcTextAndTransactionIsRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(transaction, "AddCountry", new object[] { "SA", "South Africa" });
                    db.ExecuteNonQuery(transaction, "AddCountry", new object[] { "SA", "South Africa" });
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
                finally
                {
                    connection.Close();
                }

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode = 'SA'"))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 0);
                }
            }
        }
    }
}

