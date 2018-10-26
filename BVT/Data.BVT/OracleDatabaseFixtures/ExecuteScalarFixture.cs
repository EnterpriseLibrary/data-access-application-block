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
    public class ExecuteScalarFixture : EntLibFixtureBase
    {
        public ExecuteScalarFixture()
            : base(@"ConfigFiles.OracleDatabaseFixture.config")
        { }

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
        public void ValueIsReturnedWhenUsingText()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            string countryName = (string)db.ExecuteScalar(CommandType.Text, "select CountryName from Country where CountryCode='IN'");
            Assert.AreEqual("India", countryName);
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingCommand()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetSqlStringCommand("select CountryName from Country where CountryCode='US'");
            string countryName = (string)db.ExecuteScalar(dbCommand);
            Assert.AreEqual("UnitedStates", countryName);
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingCOmmandAndOutParameter()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            DbCommand dbCommand = db.GetStoredProcCommand("GetProductName");
            db.AddInParameter(dbCommand, "vProductId", DbType.Int32, 1);
            db.AddOutParameter(dbCommand, "vResult", DbType.String, 100);
            db.ExecuteScalar(dbCommand);
            Assert.AreEqual("Product1", db.GetParameterValue(dbCommand, "vResult"));
        }

        [TestMethod]
        public void RecordsAreChangedWhenUsingTextAndTransactionIsCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            int debitId = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select max(DebitId) from Debits"));

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    debitId = debitId + 1;
                    string query = "INSERT INTO DEBITS ( DebitId, AccountId, Amount )  VALUES ( " + debitId.ToString() + " , 5, 5000 )";
                    db.ExecuteScalar(transaction, CommandType.Text, query);
                    query = "DELETE FROM DEBITS where DebitId =  " + debitId.ToString();
                    db.ExecuteScalar(transaction, CommandType.Text, query);
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
                string result = "select * from Debits where DebitID = " + debitId;
                using (DataSet ds = db.ExecuteDataSet(CommandType.Text, result))
                {
                    Assert.IsTrue(0 == ds.Tables[0].Rows.Count);
                }
            }
        }

        [TestMethod]
        public void RecordsAreNotChangedWhenUsingTextAndTransactionIsRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            int debitId = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select max(DebitId),count(DebitID) from Debits"));

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    string query = "DELETE FROM DEBITS WHERE DEBITID = " + debitId.ToString();
                    db.ExecuteScalar(transaction, CommandType.Text, query);
                    debitId = debitId + 1;
                    query = "INSERT INTO DEBITS ( DebitId, AccountId, Amount )  VALUES ( " + debitId.ToString() + " , 5, 5000 )";
                    db.ExecuteScalar(transaction, CommandType.Text, query);
                    db.ExecuteScalar(transaction, CommandType.Text, query);
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
                string result = "select * from Debits where DebitID in ( " + (debitId - 1) + "," + debitId + "," + (debitId + 1) + ")";

                using (DataSet ds = db.ExecuteDataSet(CommandType.Text, result))
                {
                    Assert.IsTrue(1 == ds.Tables[0].Rows.Count);
                    Assert.IsTrue((debitId - 1) == Convert.ToInt32(ds.Tables[0].Rows[0]["DebitId"]));
                }
            }
        }

        [TestMethod]
        public void RecordsAreSavedWhenUsingTextWithParametersAndTransactionIsCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");
            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteScalar(transaction, "AddCountry", new object[] { "SA", "South Africa" });
                    db.ExecuteScalar(transaction, "DeleteCountry", new object[] { "SA" });
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
        public void RecordsAreNotSavedWhenUsingTextWithParametersAndTransactionIsRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteScalar(transaction, "DeleteCountry", new object[] { "IN" });
                    db.ExecuteScalar(transaction, "UpdateCountry", new object[] { "US", "United States of America" });
                    db.ExecuteScalar(transaction, "AddCountry", new object[] { "SA", "South Africa" });
                    db.ExecuteScalar(transaction, "AddCountry", new object[] { "SA", "South Africa" });
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

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Country where CountryCode in ('SA','IN','US') order by CountryCode "))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 2);
                    Assert.AreEqual("India", result.Tables[0].Rows[0]["CountryName"]);
                    Assert.IsFalse("United States of America" == (string)result.Tables[0].Rows[1]["CountryName"]);
                }
            }
        }

        [TestMethod]
        public void RecordsAreSavedWhenUsingCommandWithParametersAndTransactionIsCommitted()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();

                int lastEmployeeId = Convert.ToInt32(db.ExecuteScalar(CommandType.Text, "select max(EmployeeID) from Employees"));
                DbCommand addEmployee = db.GetStoredProcCommand("AddEmployees");
                DbCommand deleteEmployee = db.GetStoredProcCommand("DeleteEmployees");

                db.AddInParameter(addEmployee, "vEmployeeId", DbType.Int32, lastEmployeeId + 1);
                db.AddInParameter(addEmployee, "vFirstName", DbType.String, "First Name");
                db.AddInParameter(addEmployee, "vLastName", DbType.String, "Last Name");
                db.AddInParameter(addEmployee, "vReportsTo", DbType.Int32, lastEmployeeId);

                db.AddInParameter(deleteEmployee, "vEmployeeId", DbType.Int32, lastEmployeeId + 1);

                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(addEmployee, transaction);
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

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Employees where EmployeeId = " + (lastEmployeeId + 1)))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 0);
                }
            }
        }

        [TestMethod]
        public void RecordsAreNotSavedWhenUsingCommandWithParametersAndTransactionIsRolledBack()
        {
            Database db = DatabaseFactory.CreateDatabase("OracleTest");

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();

                DbCommand deleteEmployee = db.GetStoredProcCommand("DeleteEmployees");
                DbCommand addEmployee = db.GetStoredProcCommand("AddEmployees");

                db.AddInParameter(deleteEmployee, "vEmployeeId", DbType.Int32, 107);

                db.AddInParameter(addEmployee, "vEmployeeId", DbType.Int32, 1);
                db.AddInParameter(addEmployee, "vFirstName", DbType.String, "Unknown");
                db.AddInParameter(addEmployee, "vLastName", DbType.String, "Unknown");
                db.AddInParameter(addEmployee, "vReportsTo", DbType.Int32, null);

                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    db.ExecuteNonQuery(deleteEmployee, transaction);
                    db.ExecuteNonQuery(addEmployee, transaction);
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

                using (DataSet result = db.ExecuteDataSet(CommandType.Text, "select * from Employees where EmployeeId in (1,107) order by employeeid"))
                {
                    Assert.IsTrue(result.Tables[0].Rows.Count == 2);
                    Assert.IsFalse("Unknown" == (string)result.Tables[0].Rows[0]["FirstName"]);
                    Assert.IsFalse("Unknown" == (string)result.Tables[0].Rows[0]["LastName"]);
                }
            }
        }
    }
}

