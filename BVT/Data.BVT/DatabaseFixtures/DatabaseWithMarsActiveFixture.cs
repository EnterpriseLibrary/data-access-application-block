// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.DatabaseFixtures
{
    [TestClass]
    public class DatabaseWithMarsActiveFixture : EntLibFixtureBase
    {
        private const string GetOrdersQuery = "SELECT ProductName, UnitPrice=ROUND(Od.UnitPrice, 2), Quantity, Discount=CONVERT(int, Discount * 100),  ExtendedPrice=ROUND(CONVERT(money, Quantity * (1 - Discount) * Od.UnitPrice), 2) FROM Products P, [Order Details] Od WHERE Od.ProductID = P.ProductID and Od.OrderID = 10248";
        private const string GetProductIDQuery = "SELECT ProductID FROM Products WHERE ProductName = @ProductName";
        private Database database;

        public DatabaseWithMarsActiveFixture()
            : base("ConfigFiles.DatabaseMarsFixture.config")
        {
        }

        [TestInitialize]
        public void Init()
        {
            database = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlMARS");
        }

        [TestMethod]
        public void DoesDatabaseSupportMARS()
        {
            var databaseConnection = database.CreateConnection();
            databaseConnection.Open();

            Assert.IsInstanceOfType(database, typeof(SqlDatabase));
            Assert.IsInstanceOfType(databaseConnection, typeof(SqlConnection));

            int serverVersion = int.Parse((databaseConnection as SqlConnection).ServerVersion.Split('.')[0]);
            Assert.IsTrue(serverVersion > 9 && serverVersion < 1000);

            databaseConnection.Dispose();
        }

        [TestMethod]
        public void NestedReadersReadRecordsWhenReadingSynchronously()
        {
            Assert.IsNotNull(database);

            var connection = database.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();
            using (transaction)
            {
                var outerCommand = database.GetSqlStringCommand(GetOrdersQuery);

                using (IDataReader reader = database.ExecuteReader(outerCommand, transaction))
                {
                    Assert.IsFalse(reader.IsClosed);
                    Assert.AreEqual(5, reader.FieldCount);

                    int readerRowCount = 0;
                    while (reader.Read())
                    {
                        readerRowCount++;
                        Assert.IsFalse(reader.IsClosed);
                        Assert.AreEqual(5, reader.FieldCount);

                        using (var innerCommand = database.GetSqlStringCommand(GetProductIDQuery))
                        {
                            database.AddInParameter(innerCommand, "@ProductName", DbType.String, reader[0]);

                            using (IDataReader innerReader = database.ExecuteReader(innerCommand, transaction))
                            {
                                Assert.IsFalse(innerReader.IsClosed);
                                Assert.AreEqual(1, innerReader.FieldCount);

                                int innerReaderRowCount = 0;
                                while (innerReader.Read())
                                {
                                    innerReaderRowCount++;
                                    Assert.IsFalse(innerReader.IsClosed);
                                    Assert.AreEqual(1, innerReader.FieldCount);
                                }
                                Assert.AreEqual(1, innerReaderRowCount);
                            }
                        }
                    }
                    Assert.AreEqual(3, readerRowCount);
                }
            }
        }

        [TestMethod]
        public void NestedReadersReadRecordsWhenReadingASynchronously()
        {
            var databaseAsync = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlMarsAsync");

            Assert.IsNotNull(databaseAsync);

            var connection = databaseAsync.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();

            using (transaction)
            {
                var outerCommand = databaseAsync.GetSqlStringCommand(GetOrdersQuery);

                var asyncReader = databaseAsync.BeginExecuteReader(outerCommand, transaction, ar => EmptyAsyncCallback(ar), null);
                using (IDataReader reader = databaseAsync.EndExecuteReader(asyncReader))
                {
                    Assert.IsFalse(reader.IsClosed);
                    Assert.AreEqual(5, reader.FieldCount);

                    int readerRowCount = 0;
                    while (reader.Read())
                    {
                        readerRowCount++;
                        Assert.IsFalse(reader.IsClosed);
                        Assert.AreEqual(5, reader.FieldCount);

                        using (var innerCommand = databaseAsync.GetSqlStringCommand(GetProductIDQuery))
                        {
                            databaseAsync.AddInParameter(innerCommand, "@ProductName", DbType.String, reader[0]);

                            var asyncInnerReader = databaseAsync.BeginExecuteReader(innerCommand, transaction, ar => EmptyAsyncCallback(ar), null);

                            using (IDataReader innerReader = databaseAsync.EndExecuteReader(asyncInnerReader))
                            {
                                Assert.IsFalse(innerReader.IsClosed);
                                Assert.AreEqual(1, innerReader.FieldCount);

                                int innerReaderRowCount = 0;
                                while (innerReader.Read())
                                {
                                    innerReaderRowCount++;
                                    Assert.IsFalse(innerReader.IsClosed);
                                    Assert.AreEqual(1, innerReader.FieldCount);
                                }
                                Assert.AreEqual(1, innerReaderRowCount);
                            }
                        }
                    }
                    Assert.AreEqual(3, readerRowCount);
                }
            }
        }

        private object EmptyAsyncCallback(IAsyncResult ar)
        {
            return null;
        }
    }
}

