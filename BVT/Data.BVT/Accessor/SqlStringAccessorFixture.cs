// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    [TestClass]
    public class SqlStringAccessorFixture : EntLibFixtureBase
    {
        SqlDatabase sqlDatabase = null;

        public SqlStringAccessorFixture()
            : base("ConfigFiles.DatabaseSqlAccessor.config")
        {
        }

        [TestInitialize]
        public void Setup()
        {
            sqlDatabase = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault() as SqlDatabase;
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringWithNoMappings()
        {
            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
                MapBuilder<Customer>.MapNoProperties().Build());

            var first = rowMapper.First();

            Assert.AreEqual(default(int), first.CustomerID);
            Assert.AreEqual(default(string), first.CompanyName);
            Assert.AreEqual(default(DateTime), first.BirthDate);
            Assert.AreEqual(default(bool), first.IsEmployee);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlString()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'");

            var list = result.ToList();

            Assert.AreEqual(1, list.Count);

            var customer = list[0];

            Assert.AreEqual(1, customer.CustomerID);
            Assert.AreEqual("Company", customer.CompanyName);
            Assert.AreEqual(true, customer.IsEmployee);
            Assert.AreEqual(DateTime.Parse("01/01/2000"), customer.BirthDate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void InvalidCastExceptionIsThrownWhenExecutingSqlStringWithNonMatchingResultTypes()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 'a' AS CustomerID, 1 AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'");

            result.ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingSqlStringWithAnOutputMapperContainingMorePropertiesThanTheColumnsReturned()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT '1' AS CustomerID, 1 AS CompanyName");

            result.ToList();
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringAndResultContainsReadOnlyProperties()
        {
            IEnumerable<ReadOnlyProperties> result = sqlDatabase.ExecuteSqlStringAccessor<ReadOnlyProperties>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'");

            var list = result.ToList();

            Assert.AreEqual(1, list.Count);

            var customer = list[0];

            Assert.AreEqual(default(int), customer.CustomerID);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringAndDoNotMapProperties()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
                MapBuilder<Customer>.MapAllProperties()
                .DoNotMap(c => c.CompanyName)
                .DoNotMap(c => c.CustomerID)
                .DoNotMap(c => c.BirthDate)
                .DoNotMap(c => c.IsEmployee)
                .Build());

            var customer = result.First();

            Assert.AreEqual(default(int), customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(DateTime), customer.BirthDate);
            Assert.AreEqual(default(bool), customer.IsEmployee);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingDoNotMapWithPropInfo()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
                MapBuilder<Customer>.MapAllProperties()
                .DoNotMap(typeof(Customer).GetProperty("CustomerID"))
                .DoNotMap(typeof(Customer).GetProperty("CompanyName"))
                .DoNotMap(typeof(Customer).GetProperty("BirthDate"))
                .Build());

            var customer = result.First();

            Assert.AreEqual(default(int), customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(DateTime), customer.BirthDate);
            Assert.AreEqual(true, customer.IsEmployee);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingSqlStringUsingDoNotMapWithNullAsPropInfo()
        {
            sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
                    MapBuilder<Customer>.MapAllProperties()
                    .DoNotMap(typeof(Customer).GetProperty("InvalidProperty"))
                    .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingDoNotMapOnANotInheritedClass()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<NotInheritedPerson>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '19790103') AS 'BirthDate'",
                MapBuilder<NotInheritedPerson>.MapAllProperties()
                .DoNotMap(c => c.CustomerID)
                .DoNotMap(c => c.IsEmployee)
                .DoNotMap(c => c.BirthDate)
                .Build());

            var customer = result.First();

            Assert.AreEqual(default(int), customer.CustomerID);
            Assert.AreEqual("Company", customer.CompanyName);
            Assert.AreEqual(default(bool), customer.IsEmployee);
            Assert.AreEqual(default(DateTime), customer.BirthDate);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenMappingAClassWithIndexerProperty()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<ClassWithIndexerProperty>("SELECT 2 AS Item");

            var item = result.First();
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenMappingAClassWithHiddenProperty()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<InheritedPersonWithHiddenProperties>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '19790103') AS 'BirthDate'",
                MapBuilder<InheritedPersonWithHiddenProperties>.MapAllProperties()
                .DoNotMap(typeof(InheritedPersonWithHiddenProperties).GetProperty("CustomerID"))
                .Build());

            var item = result.First();

            Assert.AreEqual(0, item.CustomerID);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenMappingAClassWithStaticProperty()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<ClassWithStaticProperty>("SELECT 'Sample data' as Test",
                MapBuilder<ClassWithStaticProperty>.MapAllProperties()
                .Build());

            var item = result.First();

            Assert.AreEqual(default(string), ClassWithStaticProperty.Test);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void SqlExceptionIsThrownWhenExecutingAnInvalidQuery()
        {
            var result = sqlDatabase.ExecuteSqlStringAccessor<ClassWithStaticProperty>("nothing to query",
                MapBuilder<ClassWithStaticProperty>.MapAllProperties()
                .Build());

            result.First();
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingNullableData()
        {
            string insertQuery = "INSERT INTO [Northwind].[dbo].[Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(\'Test\',null,null,null,null,null,null,null,0)";

            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSql");

            try
            {
                db.ExecuteNonQuery(CommandType.Text, insertQuery);

                IEnumerable<ProductSupplier> productSuppliers = db.ExecuteSqlStringAccessor("Select ProductName,SupplierID from Products", MapBuilder<ProductSupplier>.MapAllProperties().Build());

                var productsWithNullSuppliers = from p in productSuppliers where p.SupplierID == null select p;

                Assert.AreEqual(1, productsWithNullSuppliers.Count());
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, "Delete from products where ProductName=\'Test\'");
            }
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringWithColumnReturnedNotAProperty()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'TestName' AS CustomerName, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'");

            var list = result.ToList();
            Assert.AreEqual(1, list.Count);

            var customer = list[0];

            Assert.AreEqual(1, customer.CustomerID);
            Assert.AreEqual("Company", customer.CompanyName);
            Assert.AreEqual(true, customer.IsEmployee);
            Assert.AreEqual(DateTime.Parse("01/01/2000"), customer.BirthDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingWithEmptyQueryString()
        {
            sqlDatabase.ExecuteSqlStringAccessor<Customer>(string.Empty,
                MapBuilder<Customer>.MapAllProperties()
                .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingBuildAllPropertiesWithSchemaSubsetAndCalculatedFieldInEntity()
        {
            int expectedRowCount = (int)sqlDatabase.ExecuteScalar(CommandType.Text, "Select count(*) from Customers");
            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<CustomerWithCalculatedField>("SELECT CustomerID, CompanyName, City, Country, CONVERT(DATETIME, '20000101') AS 'BirthDate' from Customers",
               MapBuilder<CustomerWithCalculatedField>.BuildAllProperties());
            DateTime expectedDateTime = DateTime.Parse("01/01/2000");

            var list = from c in rowMapper where c.BirthDate == expectedDateTime select c;
            Assert.AreEqual(expectedRowCount, list.Count());

        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingMap()
        {
            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .Map(a => a.CompanyName)
               .WithFunc(b => b.GetString(1).Substring(3))
               .Build());

            var first = rowMapper.First();

            Assert.AreEqual(default(int), first.CustomerID);
            Assert.AreEqual("pany", first.CompanyName);
            Assert.AreEqual(default(DateTime), first.BirthDate);
            Assert.AreEqual(default(bool), first.IsEmployee);
        }

        [TestMethod]
        [Ignore] //until raised bug is fixed
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingSqlStringUsingMapWithFuncNull()
        {
            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .Map(a => a.CompanyName)
               .WithFunc(null)
               .Build());

            rowMapper.First();
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingMapByName()
        {
            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .MapByName(a => a.CustomerID)
               .Build());

            var first = rowMapper.First();

            Assert.AreEqual(1, first.CustomerID);
            Assert.AreEqual(default(string), first.CompanyName);
            Assert.AreEqual(default(DateTime), first.BirthDate);
            Assert.AreEqual(default(bool), first.IsEmployee);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingMapByNameWithPropInfo()
        {
            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .MapByName(typeof(Customer).GetProperty("CustomerID"))
               .MapByName(typeof(Customer).GetProperty("CompanyName"))
               .Build());

            var first = rowMapper.First();

            Assert.AreEqual(1, first.CustomerID);
            Assert.AreEqual("Company", first.CompanyName);
            Assert.AreEqual(default(DateTime), first.BirthDate);
            Assert.AreEqual(default(bool), first.IsEmployee);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingSqlStringUsingMapByNameWithInvalidProp()
        {
            sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .MapByName(typeof(Customer).GetProperty("CustomerID"))
               .MapByName(typeof(Customer).GetProperty("InvalidProperty"))
               .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringWithToColumn()
        {
            IEnumerable<CustomerDetails> result = sqlDatabase.ExecuteSqlStringAccessor<CustomerDetails>("SELECT 'CustID' AS CustomerID, 'Company' AS CompanyName, 'Redmond' AS City, 'USA' AS Country",
               MapBuilder<CustomerDetails>.MapAllProperties()
               .Map(a => a.City)
               .ToColumn("Country")
               .Build());

            var list = result.ToList();
            Assert.AreEqual(1, list.Count);

            var customer = list[0];
            Assert.AreEqual("CustID", customer.CustomerID);
            Assert.AreEqual("Company", customer.CompanyName);
            Assert.AreEqual("USA", customer.City);
            Assert.AreEqual("USA", customer.Country);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenMappingToAGenericProperty()
        {
            IEnumerable<ClassWithGenericProperty> result = sqlDatabase.ExecuteSqlStringAccessor<ClassWithGenericProperty>("SELECT 'sample data' AS Something",
               MapBuilder<ClassWithGenericProperty>.MapAllProperties()
               .Build());

            var list = result.ToList();
            Assert.AreEqual(1, list.Count);

            var customer = list[0];
            Assert.AreEqual("sample data", customer.Something);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenMappingToACollection()
        {
            IEnumerable<CustomerWithOrders> result = sqlDatabase.ExecuteSqlStringAccessor<CustomerWithOrders>("SELECT 'Country' AS Country, 'City' AS City, 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate', 'Details' AS Details",
               MapBuilder<CustomerWithOrders>.MapAllProperties()
               .Build());

            var list = result.ToList();
            Assert.AreEqual(1, list.Count);

            var customer = list[0];
            Assert.IsNull(customer.Details);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingSqlStringWithInvalidToColumn()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapAllProperties()
               .Map(a => a.CustomerID)
               .ToColumn("InvalidColumn")
               .Build());

            result.ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void InvalidCastExceptionIsThrownWhenExecutingSqlStringWithToColumnInvalidTypeCast()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapAllProperties()
               .Map(a => a.CustomerID)
               .ToColumn("CompanyName")
               .Build());

            result.ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingSqlStringWithEmptyToColumn()
        {
            IEnumerable<Customer> result = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapAllProperties()
               .Map(a => a.CustomerID)
               .ToColumn(string.Empty)
               .Build());

            result.ToList();
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingOneSqlStringInTransactionScopeNoDependenceOnDTC()
        {
            SqlDatabase sqlDatabase1 = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSql") as SqlDatabase;
            ServiceHelper.Stop("MSDTC");
            using (TransactionScope scope = new TransactionScope())
            {
                var result = sqlDatabase1.ExecuteSqlStringAccessor<CustomerDetails>("SELECT * from Customers",
                    MapBuilder<CustomerDetails>.MapAllProperties()
                    .Build());

                var first = result.First();
                result.Skip(1);
                result.Last();
                int count = result.Count();
                var resultSubset = from r in result where r.CompanyName == "Alfreds Futterkiste" select r;
                resultSubset.First();
                resultSubset.Skip(1);
                scope.Complete();
            }
            ServiceHelper.Start("MSDTC");
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingMapPropInfo()
        {
            PropertyInfo propCompName = typeof(Customer).GetProperty("CompanyName");

            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .Map(propCompName)
               .WithFunc(a => a.GetString(1))
               .Build());

            var first = rowMapper.First();
            Assert.AreEqual(default(int), first.CustomerID);
            Assert.AreEqual("Company", first.CompanyName);
            Assert.AreEqual(default(bool), first.IsEmployee);
            Assert.AreEqual(default(DateTime), first.BirthDate);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingDoNotMapPropInfo()
        {
            PropertyInfo prop = typeof(CustomerDetails).GetProperty("CompanyName");

            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapAllProperties()
               .DoNotMap(prop)
               .Build());

            var first = rowMapper.First();
            Assert.AreEqual(1, first.CustomerID);
            Assert.AreEqual(default(string), first.CompanyName);
            Assert.AreEqual(true, first.IsEmployee);
            Assert.AreEqual(DateTime.Parse("01/01/2000"), first.BirthDate);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringUsingMapByNamePropInfo()
        {
            PropertyInfo propCompName = typeof(Customer).GetProperty("CompanyName");
            PropertyInfo propCustID = typeof(Customer).GetProperty("CustomerID");

            var rowMapper = sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .MapByName(propCustID)
               .MapByName(propCompName)
               .Build());

            var first = rowMapper.First();
            Assert.AreEqual(1, first.CustomerID);
            Assert.AreEqual("Company", first.CompanyName);
            Assert.AreEqual(default(bool), first.IsEmployee);
            Assert.AreEqual(default(DateTime), first.BirthDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingSqlStringUsingNullPropInfo()
        {
            sqlDatabase.ExecuteSqlStringAccessor<Customer>("SELECT 1 AS CustomerID, 'Company' AS CompanyName, CAST(1 AS bit) AS IsEmployee, CONVERT(DATETIME, '20000101') AS 'BirthDate'",
               MapBuilder<Customer>.MapNoProperties()
               .Map(null)
               .WithFunc(a => a.GetString(1))
               .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessor()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers");
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            int count = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Customers");
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            Assert.IsNull(state.Exception);
            Assert.AreEqual<int>(count, resultSet.Count());
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers", MapBuilder<CustomerDetails>.MapAllProperties().Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            int count = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Customers");
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            Assert.IsNull(state.Exception);
            Assert.AreEqual<int>(count, resultSet.Count());
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            CustomParameterMapper defaultParamMapper = new CustomParameterMapper(sqlDatabase);
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers", defaultParamMapper);
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            int count = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Customers");
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            Assert.IsNull(state.Exception);
            Assert.AreEqual<int>(count, resultSet.Count());
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithParameterMapperRowMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            CustomParameterMapper defaultParamMapper = new CustomParameterMapper(sqlDatabase);
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers", defaultParamMapper, MapBuilder<CustomerDetails>.MapAllProperties().Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            int count = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Customers");
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            Assert.IsNull(state.Exception);
            Assert.AreEqual<int>(count, resultSet.Count());
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorInvalidQuery()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("nothing to query");
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;

            if (state.Exception != null)
            {
                Console.Write(state.Exception.ToString());
            }
            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));

        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperDoNotMap()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapAllProperties()
                .DoNotMap(a => a.CompanyName)
                .DoNotMap(a => a.Country)
                .DoNotMap(a => a.City)
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CustomerID select c;
            var customer = orderedCustomers.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual("ALFKI", customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(string), customer.City);
            Assert.AreEqual(default(string), customer.Country);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperDoNotMapWithPropInfo()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapAllProperties()
                .DoNotMap(typeof(CustomerDetails).GetProperty("CompanyName"))
                .DoNotMap(typeof(CustomerDetails).GetProperty("City"))
                .DoNotMap(typeof(CustomerDetails).GetProperty("Country"))
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CustomerID select c;
            var customer = orderedCustomers.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual("ALFKI", customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(string), customer.City);
            Assert.AreEqual(default(string), customer.Country);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperDoNotMapAsNull()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                            MapBuilder<CustomerDetails>
                            .MapAllProperties()
                            .DoNotMap(null)
                            .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperMapNoProperties()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapNoProperties()
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var customer = resultSet.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual(default(string), customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(string), customer.City);
            Assert.AreEqual(default(string), customer.Country);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperInvalidQuery()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("nothing to query",
                MapBuilder<CustomerDetails>
                .MapAllProperties()
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;

            if (state.Exception != null)
            {
                Console.WriteLine(state.Exception.ToString());
            }
            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperEmptyStringQuery()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            db.CreateSqlStringAccessor<CustomerDetails>(string.Empty,
            MapBuilder<CustomerDetails>
            .MapAllProperties()
            .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringAccessorWithCustomRowMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SqlStringAccessor<CustomerDetails>(db, "SELECT 'Redmond' AS City, 'MS' AS CompanyName, 'USA' AS Country, 'First' AS CustomerID", new CustomerDetailsRowMapper());

            var results = accessor.Execute();

            var customerDetail = results.First();

            Assert.AreEqual("Redmond", customerDetail.City);
            Assert.AreEqual("MS", customerDetail.CompanyName);
            Assert.AreEqual("USA", customerDetail.Country);
            Assert.AreEqual("First", customerDetail.CustomerID);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingSqlStringAccessorWithParametersAndWithoutParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SqlStringAccessor<CustomerDetails>(db, "SELECT 'Redmond' AS City, 'MS' AS CompanyName, 'USA' AS Country, 'First' AS CustomerID", new CustomerDetailsRowMapper());

            var results = accessor.Execute(new object[1] { "invalid operation" });

            results.First();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingSqlStringAccessorWithEmptyQuery()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            new SqlStringAccessor<CustomerDetails>(db, "", new CustomerDetailsRowMapper());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingSqlStringAccessorWithCustomResultSetMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SqlStringAccessor<CustomerDetails>(db, "Select * From Customers", new CustomerDetailsResultSet());

            var results = accessor.Execute().ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("ALFKI", results[0].CustomerID);
            Assert.AreEqual("Germany", results[0].Country);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingSqlStringAccessorWithInvalidDatabase()
        {
            new SqlStringAccessor<CustomerDetails>(null, "Select * From Customers", new CustomerDetailsRowMapper());
        }

        [TestMethod]
        public void WhenExecutingSqlStringAccessorWithParametersAndParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SqlStringAccessor<CustomerDetails>(db, "SELECT * From Customers where CustomerID = @ID", new GetCustomerByIdParameterMapper(), new CustomerDetailsRowMapper());

            var results = accessor.Execute(new object[1] { "ALFKI" });

            var customerDetail = results.First();
            Assert.AreEqual("ALFKI", customerDetail.CustomerID);
            Assert.AreEqual("Berlin", customerDetail.City);
            Assert.AreEqual("Germany", customerDetail.Country);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperUsingMap()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
               MapBuilder<CustomerDetails>
               .MapNoProperties()
               .Map(a => a.CustomerID)
               .WithFunc(b => b.GetString(0).Substring(3))
               .Build());

            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;
            var customer = orderedCustomers.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual("KI", customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(string), customer.City);
            Assert.AreEqual(default(string), customer.Country);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperUsingMapNull()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
               MapBuilder<CustomerDetails>
               .MapNoProperties()
               .Map(null)
               .WithFunc(b => b.GetString(0).Substring(3))
               .Build());
        }

        [TestMethod]
        [Ignore] //until raised bug is fixed
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperUsingFuncNull()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
               MapBuilder<CustomerDetails>
               .MapNoProperties()
               .Map(a => a.CustomerID)
               .WithFunc(null)
               .Build());

            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;

            orderedCustomers.First();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperMapByName()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapNoProperties()
                .MapByName(a => a.CustomerID)
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;

            var customer = orderedCustomers.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual("ALFKI", customer.CustomerID);
            Assert.AreEqual(default(string), customer.CompanyName);
            Assert.AreEqual(default(string), customer.City);
            Assert.AreEqual(default(string), customer.Country);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperMapByNamePropInfo()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapNoProperties()
                .MapByName(typeof(CustomerDetails).GetProperty("CustomerID"))
                .MapByName(typeof(CustomerDetails).GetProperty("CompanyName"))
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;

            var customer = orderedCustomers.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual("ALFKI", customer.CustomerID);
            Assert.AreEqual("Alfreds Futterkiste", customer.CompanyName);
            Assert.AreEqual(default(string), customer.City);
            Assert.AreEqual(default(string), customer.Country);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperMapByNameInvalidPropInfo()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapNoProperties()
                .MapByName(typeof(CustomerDetails).GetProperty("CustomerID"))
                .MapByName(typeof(CustomerDetails).GetProperty("InvalidProperty"))
                .Build());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperMapByNameNull()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapNoProperties()
                .MapByName(null)
                .Build());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorWithRowMapperMapToColumn()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapAllProperties()
                .Map(a => a.CustomerID)
                .ToColumn("CompanyName")
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;

            var customer = orderedCustomers.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual("Alfreds Futterkiste", customer.CompanyName);
            Assert.AreEqual(customer.CustomerID, customer.CompanyName);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperMapInvalidToColumn()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapAllProperties()
                .Map(a => a.CustomerID)
                .ToColumn("InvalidColumn")
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;

            orderedCustomers.First();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithRowMapperMapEmptyToColumn()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerDetails> accessor = db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers",
                MapBuilder<CustomerDetails>
                .MapAllProperties()
                .Map(a => a.CustomerID)
                .ToColumn("InvalidColumn")
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerDetails>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<CustomerDetails> resultSet = (IEnumerable<CustomerDetails>)state.State;
            var orderedCustomers = from c in resultSet orderby c.CompanyName select c;

            orderedCustomers.First();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);

        }

        [TestMethod]
        [Ignore] // until raised bug is fixed
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorNullableData()
        {
            string insertQuery = "INSERT INTO [Northwind].[dbo].[Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(\'Test\',null,null,null,null,null,null,null,0)";
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            try
            {
                db.ExecuteNonQuery(CommandType.Text, insertQuery);
                DataAccessor<ProductSupplier> accessor = db.CreateSqlStringAccessor<ProductSupplier>("Select * from Products",
                    MapBuilder<ProductSupplier>
                    .MapAllProperties()
                    .Build());
                AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<ProductSupplier>);
                DbAsyncState state = new DbAsyncState(db, accessor);
                IAsyncResult result = accessor.BeginExecute(cb, state);
                state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
                int count = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Products");
                IEnumerable<ProductSupplier> resultSet = (IEnumerable<ProductSupplier>)state.State;
                // resultSet is NULL here
                Assert.AreEqual<int>(count, resultSet.Count());

                var productsWithNullSuppliers = from p in resultSet where p.SupplierID == null select p;

                Assert.IsNull(state.Exception);
                Assert.AreEqual(1, productsWithNullSuppliers.Count());
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, "Delete from products where ProductName=\'Test\'");
            }
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorBuildAllWithSchemaSubsetAndCalculatedFieldInEntity()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<CustomerWithCalculatedField> accessor = db.CreateSqlStringAccessor<CustomerWithCalculatedField>("SELECT CustomerID, CompanyName, City, Country, CONVERT(DATETIME, '20000101') AS 'BirthDate' from Customers",
               MapBuilder<CustomerWithCalculatedField>
               .BuildAllProperties());

            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<CustomerWithCalculatedField>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            int count = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Customers");
            IEnumerable<CustomerWithCalculatedField> resultSet = (IEnumerable<CustomerWithCalculatedField>)state.State;

            DateTime expectedDateTime = DateTime.Parse("01/01/2000");
            var list = from c in resultSet where c.BirthDate == expectedDateTime select c;
            Assert.AreEqual(count, list.Count());
        }

        [TestMethod]
        public void ResultSetIsReturnedWhenExecutingAsyncSqlStringAccessorResultContainsReadOnlyProperties()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<ReadOnlyProperties> accessor = db.CreateSqlStringAccessor<ReadOnlyProperties>("SELECT * from Customers");

            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<ReadOnlyProperties>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<ReadOnlyProperties> resultSet = (IEnumerable<ReadOnlyProperties>)state.State;
            var customer = resultSet.First();

            Assert.IsNull(state.Exception);
            Assert.AreEqual(default(int), customer.CustomerID);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncSqlStringAccessorWithNullParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            CustomParameterMapper customParameterMapper = null;
            db.CreateSqlStringAccessor<CustomerDetails>("Select * from Customers", customParameterMapper);
        }

        private void EndExecuteAccessor<T>(IAsyncResult result)
        {
            DaabAsyncResult daabResult = (DaabAsyncResult)result;
            DbAsyncState state = (DbAsyncState)daabResult.AsyncState;
            try
            {
                DataAccessor<T> accessor = (DataAccessor<T>)state.Accessor;
                state.State = accessor.EndExecute(result);
                state.AsyncResult = (DaabAsyncResult)result;
            }
            catch (Exception e)
            {
                state.Exception = e;

            }
            finally
            {
                state.AutoResetEvent.Set();
            }
        }
    }
}

