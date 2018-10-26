// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.DatabaseFixtures
{
    [TestClass]
    public class DatabaseValidConfigurationFixture : EntLibFixtureBase
    {
        private const int NewRegionID = 5;
        private const int CurrentRegionID = 4;
        private const int OrderID = 10248;
        private const string MaxRegionSqlQuery = "SELECT MAX(RegionID) FROM Region";
        private const string NewRegionName = "TestRegion";
        private const string GetOrdersStoredProcName = "CustOrdersDetail";
        private const string GetOrdersQuery = "SELECT ProductName, UnitPrice=ROUND(Od.UnitPrice, 2), Quantity, Discount=CONVERT(int, Discount * 100),  ExtendedPrice=ROUND(CONVERT(money, Quantity * (1 - Discount) * Od.UnitPrice), 2) FROM Products P, [Order Details] Od WHERE Od.ProductID = P.ProductID and Od.OrderID = 10248";
        private Database database;

        public DatabaseValidConfigurationFixture()
            : base("ConfigFiles.DatabaseValidFixture.config")
        {
        }

        [TestInitialize]
        public void Init()
        {
            database = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();
        }

        [TestMethod]
        public void CorrectDatabaseTypeIsReturnedWhenDefaultDatabaseIsResolved()
        {
            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
        }

        [TestMethod]
        public void CorrectDatabaseTypeIsReturnedWhenNamedDatabaseIsResolved()
        {
            var defaultDatabase = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSql");

            Assert.IsInstanceOfType(defaultDatabase, typeof(SqlDatabase));
        }

        [TestMethod]
        public void ConnectionIsOpenedWhenConnectingToADatabaseUsingDefaultDatabase()
        {
            using (var connection = database.CreateConnection())
            {
                connection.Open();
                Assert.IsTrue(connection.State == System.Data.ConnectionState.Open);
                connection.Close();
            }
        }

        [TestMethod]
        public void ConnectionIsOpenedWhenConnectingToADatabaseUsingToANamedDatabase()
        {
            database = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSql");

            using (var connection = database.CreateConnection())
            {
                connection.Open();
                Assert.IsTrue(connection.State == System.Data.ConnectionState.Open);
                connection.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionIsThrownWhenConnectingToADatabaseUsingInvalidConnectionStringArgument()
        {
            database = new DatabaseProviderFactory(base.ConfigurationSource).Create("InvalidConnectionString");

            database.CreateConnection();
        }

        [TestMethod]
        public void ParametersAreAddedToStoredProcCommandWhenUsingParameterDirection()
        {
            var command = database.GetStoredProcCommand("Mock");

            database.AddParameter(command, "param1", System.Data.DbType.String, ParameterDirection.Input, "source", DataRowVersion.Default, "Mock");
            database.AddParameter(command, "outParam", System.Data.DbType.String, 10, ParameterDirection.Output, true, 1, 1, "source", DataRowVersion.Default, "Value");

            Assert.AreEqual(2, command.Parameters.Count);
            Assert.AreEqual(ParameterDirection.Input, command.Parameters[0].Direction);
            Assert.AreEqual(ParameterDirection.Output, command.Parameters[1].Direction);
        }

        [TestMethod]
        public void ParametersAreAddedToStoredProcCommandWhenDatabaseAddMethodUsed()
        {
            var command = database.GetStoredProcCommand("Mock");

            database.AddInParameter(command, "param1", System.Data.DbType.String);
            database.AddOutParameter(command, "outParam", System.Data.DbType.String, 10);

            Assert.AreEqual(2, command.Parameters.Count);
            Assert.AreEqual("@outParam", command.Parameters[1].ParameterName);
            Assert.AreEqual(ParameterDirection.Output, command.Parameters[1].Direction);
        }

        [TestMethod]
        public void ParametersAreAddedToStoredProcCommandWhenDatabaseAddInParameterMethodUsed()
        {
            var command = database.GetStoredProcCommand("Mock");

            database.AddInParameter(command, "param1", System.Data.DbType.String);
            database.AddInParameter(command, "param2", System.Data.DbType.Guid);

            Assert.AreEqual(2, command.Parameters.Count);
            Assert.AreEqual(System.Data.DbType.String, command.Parameters["@param1"].DbType);
            Assert.AreEqual(ParameterDirection.Input, command.Parameters[0].Direction);
            Assert.AreEqual("@param2", command.Parameters[1].ParameterName);
            Assert.AreEqual(ParameterDirection.Input, command.Parameters[1].Direction);
        }

        [TestMethod]
        public void ParameterValueIsSet()
        {
            var command = database.GetStoredProcCommand("Mock");

            database.AddInParameter(command, "param1", System.Data.DbType.String, "Test");

            string result = database.GetParameterValue(command, "param1") as string;

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public void ParameterNameIsBuiltWhenUsingSqlDatabase()
        {
            var sqlDatabase = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault() as SqlDatabase;

            Assert.IsNotNull(sqlDatabase);

            string result = sqlDatabase.BuildParameterName("parameter1");

            Assert.AreEqual(result, "@parameter1");
        }

        [TestMethod]
        [Ignore] // ignore until we get a resolution to the argument validation issue
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenGettingStoredProcCommandWithSourceColumnsWithoutSourceColumns()
        {
            Assert.IsNotNull(database);

            database.GetStoredProcCommandWithSourceColumns(GetOrdersStoredProcName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenGettingStoredProcCommandWithSourceColumnsWithoutStoredProcName()
        {
            Assert.IsNotNull(database);

            database.GetStoredProcCommandWithSourceColumns("", "Mock");
        }

        [TestMethod]
        public void StoredProcedureParameterIsDerivedWhenGettingStoredProcCommandWithSourceColumns()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommandWithSourceColumns(GetOrdersStoredProcName, "OrderID");

            Assert.AreEqual("OrderID", dbCommand.Parameters[1].SourceColumn);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenGettingStoredProcCommandWithSourceColumnsWithInvalidStoredProcName()
        {
            Assert.IsNotNull(database);

            database.GetStoredProcCommandWithSourceColumns("InvalidSP", "OrderID");
        }

        [TestMethod]
        public void ParameterValueIsSetWhenGettingStoredProcCommandPassingParamaters()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, 10);

            Assert.AreEqual(10, dbCommand.Parameters[1].Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenGettingStoredProcCommandWithEmptyName()
        {
            Assert.IsNotNull(database);

            database.GetStoredProcCommand("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenGettingStoredProcCommandPassingWrongParamaters()
        {
            Assert.IsNotNull(database);

            database.GetStoredProcCommand(GetOrdersStoredProcName, 10, 3);
        }

        [TestMethod]
        public void ParametersAreFoundWhenDiscoveringParameters()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName);

            database.DiscoverParameters(dbCommand);

            Assert.AreEqual(2, dbCommand.Parameters.Count);
            Assert.AreEqual(DbType.Int32, dbCommand.Parameters[0].DbType);
            Assert.AreEqual(DbType.Int32, dbCommand.Parameters[1].DbType);
        }

        [TestMethod]
        public void ValueIsReturnedWhenExecutingADataReaderPassingADbCommand()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, OrderID);

            using (IDataReader reader = database.ExecuteReader(dbCommand))
            {
                Assert.IsFalse(reader.IsClosed);

                Assert.AreEqual(5, reader.FieldCount);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingADataReaderPassingAnEmptyStoredProcName()
        {
            Assert.IsNotNull(database);

            database.ExecuteReader("", OrderID);
        }

        [TestMethod]
        public void FieldsAreReturnedWhenExecutingADataReaderPassingStoredProcAndValues()
        {
            Assert.IsNotNull(database);

            using (IDataReader reader = database.ExecuteReader(GetOrdersStoredProcName, OrderID))
            {
                Assert.IsFalse(reader.IsClosed);

                Assert.AreEqual(5, reader.FieldCount);
            }
        }

        [TestMethod]
        public void FieldsAreReturnedWhenExecutingADataReaderPassingACommandTextQuery()
        {
            Assert.IsNotNull(database);

            using (IDataReader reader = database.ExecuteReader(CommandType.Text, GetOrdersQuery))
            {
                Assert.IsFalse(reader.IsClosed);

                Assert.AreEqual(5, reader.FieldCount);
            }
        }

        [TestMethod]
        public void FieldsAreReturnedWhenExecutingADataReaderPassingDbCommandAndTransaction()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, OrderID);

            using (var connection = database.CreateConnection())
            {
                connection.Open();

                var trx = connection.BeginTransaction();

                using (IDataReader reader = database.ExecuteReader(dbCommand, trx))
                {
                    Assert.IsFalse(reader.IsClosed);

                    Assert.AreEqual(5, reader.FieldCount);
                }

                trx.Commit();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentExceptionIsThrownWhenExecutingADataReaderPassingNullTransaction()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, OrderID);

            database.ExecuteReader(dbCommand, null);
        }

        [TestMethod]
        public void StringValueIsReturnedWhenExecutingAScalarPassingStoredProcAndValues()
        {
            Assert.IsNotNull(database);

            string result = database.ExecuteScalar(GetOrdersStoredProcName, OrderID) as string;

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(string), result.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingAScalarPassingAnEmptyStoredProc()
        {
            Assert.IsNotNull(database);

            database.ExecuteScalar("", OrderID);
        }

        [TestMethod]
        public void StringValueIsReturnedWhenExecutingAScalarPassingAQueryString()
        {
            Assert.IsNotNull(database);

            string result = database.ExecuteScalar(CommandType.Text, GetOrdersQuery) as string;

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(string), result.GetType());
        }

        [TestMethod]
        public void StringValueIsReturnedWhenExecutingAScalarPassingDbCommandAndTransaction()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, OrderID);

            using (var connection = database.CreateConnection())
            {
                connection.Open();

                var trx = connection.BeginTransaction();

                object result = database.ExecuteScalar(dbCommand, trx);

                Assert.IsNotNull(result);
                Assert.AreEqual(typeof(string), result.GetType());

                trx.Commit();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentExceptionIsThrownWhenExecutingAScalarPassingNullTransaction()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, OrderID);

            database.ExecuteScalar(dbCommand, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingNonQueryPassingAnEmptyStoredProcName()
        {
            Assert.IsNotNull(database);

            database.ExecuteNonQuery("", 1);
        }

        [TestMethod]
        public void ValueIsReturnedWhenExecutingNonQuery()
        {
            Database db = null;
            int regionID = -1;
            try
            {
                db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

                Assert.IsNotNull(db);

                database.ExecuteNonQuery("AddRegion", NewRegionName);

                regionID = (int)db.ExecuteScalar(CommandType.Text, MaxRegionSqlQuery);
                var retrievedRegion = (string)db.ExecuteScalar(CommandType.Text, "select regionDescription from region where regionID=" + regionID.ToString());

                Assert.AreEqual(NewRegionName, retrievedRegion.Trim());
            }
            finally
            {
                // Cleanup
                if (db != null)
                {
                    db.ExecuteNonQuery("RemoveRegion", NewRegionID);
                }
            }
        }

        [TestMethod]
        public void ValueIsReturnedWhenExecutingNonQueryInATransaction()
        {
            using (var connection = database.CreateConnection())
            {
                connection.Open();

                var trx = connection.BeginTransaction();

                Assert.IsNotNull(database);

                var dbCommand = database.GetStoredProcCommand("AddRegion", NewRegionName);

                trx.Rollback();

                var regionID = (int)database.ExecuteScalar(CommandType.Text, MaxRegionSqlQuery);

                Assert.AreEqual(CurrentRegionID, regionID);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentExceptionIsThrownWhenExecutingANonQueryPassingNullTransaction()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand("RemoveRegion", NewRegionName);

            database.ExecuteNonQuery(dbCommand, null);
        }

        [TestMethod]
        public void TableIsReturnedWhenExecutingADataSet()
        {
            Assert.IsNotNull(database);

            var dbCommand = database.GetStoredProcCommand(GetOrdersStoredProcName, OrderID);

            var ds = database.ExecuteDataSet(dbCommand);

            Assert.AreEqual(1, ds.Tables.Count);
            Assert.AreEqual("Table", ds.Tables[0].TableName);
        }

        [TestMethod]
        public void TableIsReturnedWhenExecutingADataSetPassingATextQuery()
        {
            Assert.IsNotNull(database);

            var ds = database.ExecuteDataSet(CommandType.Text, GetOrdersQuery);

            Assert.AreEqual(1, ds.Tables.Count);
            Assert.AreEqual("Table", ds.Tables[0].TableName);
        }

        [TestMethod]
        public void TableIsReturnedWhenExecutingADataSetPassingStoredProcNameAndValues()
        {
            Assert.IsNotNull(database);

            var ds = database.ExecuteDataSet(GetOrdersStoredProcName, OrderID);

            Assert.AreEqual(1, ds.Tables.Count);
            Assert.AreEqual("Table", ds.Tables[0].TableName);
        }

        [TestMethod]
        public void TableIsReturnedWhenExecutingADataSetWithTransactionPassingStoredProcNameAndValues()
        {
            Assert.IsNotNull(database);

            using (var connection = database.CreateConnection())
            {
                connection.Open();

                var trx = connection.BeginTransaction();

                var ds = database.ExecuteDataSet(GetOrdersStoredProcName, OrderID);

                Assert.AreEqual(1, ds.Tables.Count);
                Assert.AreEqual("Table", ds.Tables[0].TableName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingADataSetWithNullTransaction()
        {
            Assert.IsNotNull(database);

            database.ExecuteDataSet(null, GetOrdersStoredProcName, OrderID);
        }
    }

    [TestClass]
    public class GivenSqlProviderMappedToGenericDatabaseConfiguration : EntLibFixtureBase
    {
        public GivenSqlProviderMappedToGenericDatabaseConfiguration()
            : base("ConfigFiles.SqlProviderMappedToGenericDatabase.config")
        {
        }

        [TestMethod]
        public void CorrectDatabaseTypeIsReturnedWhenDefaultDatabaseIsResolved()
        {
            var database = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            Assert.IsInstanceOfType(database, typeof(GenericDatabase));
        }

        [TestMethod]
        public void CorrectDatabaseTypeIsReturnedWhenNamedDatabaseIsResolved()
        {
            var database = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSql");

            Assert.IsInstanceOfType(database, typeof(GenericDatabase));
        }
    }
}

