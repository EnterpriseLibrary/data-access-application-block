// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Transactions;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    [TestClass]
    public class SProcAccessorFixture : EntLibFixtureBase
    {
        SqlDatabase sqlDatabase = null;

        public SProcAccessorFixture()
            : base("ConfigFiles.DatabaseSPAccessor.config")
        {
        }

        [TestInitialize]
        public void Setup()
        {
            sqlDatabase = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault() as SqlDatabase;
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void SqlExceptionIsThrownWhenExecutingAnInvalidStoredProc()
        {
            var result = sqlDatabase.ExecuteSprocAccessor<Sale>("nothing to query",
                MapBuilder<Sale>.MapAllProperties()
                .Build());

            result.First();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterlessStoredProcUsingDefaultMapper()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>("Ten Most Expensive Products");

            var first = rowMapper.First();

            Assert.AreEqual("Côte de Blaye", first.TenMostExpensiveProducts);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterlessStoredProcUsingMapper()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>("Ten Most Expensive Products",
                MapBuilder<TopTenProduct>.MapAllProperties()
                .DoNotMap(c => c.UnitPrice)
                .Build());

            var first = rowMapper.First();

            Assert.AreEqual("Côte de Blaye", first.TenMostExpensiveProducts);
            Assert.AreEqual(default(decimal), first.UnitPrice);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterStoredProcUsingMapper()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<Sale>("SalesByCategory",
                MapBuilder<Sale>.MapAllProperties()
                .DoNotMap(s => s.Quantity)
                .Build(),
                "Beverages", "1998");

            var first = rowMapper.First();

            Assert.AreEqual("Chai", first.ProductName);
            Assert.AreEqual(default(int), first.Quantity);
            Assert.AreEqual(6296.00m, first.TotalPurchase);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingStoredProcWithWrongParametersUsingMapper()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<Sale>("SalesByCategory",
                MapBuilder<Sale>.MapAllProperties()
                .Build(),
                "Beverages", "1998");

            rowMapper.First();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterlessStoredProcWithNoMappings()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>("Ten Most Expensive Products",
                MapBuilder<TopTenProduct>.MapNoProperties()
                .Build());

            var first = rowMapper.First();

            Assert.AreEqual(default(string), first.TenMostExpensiveProducts);
            Assert.AreEqual(default(decimal), first.UnitPrice);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterlessStoredProcWithFunc()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>("Ten Most Expensive Products",
                MapBuilder<TopTenProduct>.MapAllProperties()
                .Map(p => p.TenMostExpensiveProducts)
                .WithFunc(r => r.GetString(0).Substring(5))
                .Build());

            var first = rowMapper.First();

            Assert.AreEqual("de Blaye", first.TenMostExpensiveProducts);
        }

        [TestMethod]
        [Ignore]// Ignore until the raised bug is fixed
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingParameterlessStoredProcWithNullFunc()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>(" ",
                MapBuilder<TopTenProduct>.MapAllProperties()
                .Map(p => p.TenMostExpensiveProducts)
                .WithFunc(null)
                .Build());

            rowMapper.First();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcrocAccessorWithCustomRowMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SprocAccessor<TopTenProduct>(db, "Ten Most Expensive Products", new TopTenProductRowMapper());

            var results = accessor.Execute();

            var topTenProduct = results.First();

            Assert.AreEqual("Côte de Blaye", topTenProduct.TenMostExpensiveProducts);
            Assert.AreEqual(263.50M, topTenProduct.UnitPrice);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcrocAccessorWithCustomResultSetMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SprocAccessor<TopTenProduct>(db, "Ten Most Expensive Products", new TopTenProductResultSet());

            var results = accessor.Execute().ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("Côte de Blaye", results[0].TenMostExpensiveProducts);
            Assert.AreEqual(263.50M, results[0].UnitPrice);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingSqlStringAccessorWithParametersAndParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SprocAccessor<CategoryBySale>(db, "SalesByCategory", new GetCategoryByIdParameterMapper(), new CategoryRowMapper());

            var results = accessor.Execute(new object[2] { "Beverages", "1998" });

            var category = results.First();
            Assert.AreEqual("Chai", category.ProductName);
            Assert.AreEqual(6296.00M, category.TotalPurchase);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcrocAccessorWithParametersAndParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            var accessor = new SprocAccessor<CategoryBySale>(db, "SalesByCategory", new GetCategoryByIdParameterMapper(), new CategoryRowMapper());

            var results = accessor.Execute(new object[2] { "Beverages", "1998" });

            var category = results.First();
            Assert.AreEqual("Chai", category.ProductName);
            Assert.AreEqual(6296.00M, category.TotalPurchase);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingStoredProcAccessorWithNullAsParameters()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            new SprocAccessor<CategoryBySale>(db, "SalesByCategory", null, new CategoryRowMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownWhenExecutingStoredProcAccessorWithEmptyStoredProc()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).CreateDefault();

            new SprocAccessor<CategoryBySale>(db, "", new GetCategoryByIdParameterMapper(), new CategoryRowMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingStoredProcAccessorWithInvalidDatabase()
        {
            new SprocAccessor<CategoryBySale>(null, "SalesByCategory", new CategoryRowMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void InvalidCastExceptionIsThrownWhenExecutingParameterlessStoredProcWithInvalidFunc()
        {
            var result = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>("Ten Most Expensive Products",
                 MapBuilder<TopTenProduct>.MapAllProperties()
                 .Map(p => p.TenMostExpensiveProducts)
                 .WithFunc(r => r.GetDecimal(0).ToString())
                 .Build());

            result.First();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterlessStoredProcWithMapNoPropertiesWithFunc()
        {
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<TopTenProduct>("Ten Most Expensive Products",
                MapBuilder<TopTenProduct>.MapNoProperties()
                .Map(p => p.TenMostExpensiveProducts)
                .WithFunc(r => r.GetString(0).Substring(5))
                .Build());

            var first = rowMapper.First();
            Assert.AreEqual("de Blaye", first.TenMostExpensiveProducts);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingParameterlessStoredProcWithNullableProperties()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
            MapBuilder<Test>.MapAllProperties()
           .DoNotMap(s => s.CreatedDate)
           .Build());

            var first = result.First();
            var last = result.Last();
            Assert.AreEqual("Test10", last.TestName);
            Assert.AreEqual(null, last.BugsCreated);
            Assert.AreEqual("Test1", first.TestName);
            Assert.AreEqual(1, first.BugsCreated);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterWithMapNoPropertiesWithFunc()
        {
            object[] paramsArray = { "10" };
            var rowMapper = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest",
                MapBuilder<Test>.MapNoProperties()
                .Map(s => s.TestID)
                .WithFunc(r => r.GetInt32(0))
                .Build(), paramsArray);

            var first = rowMapper.First();
            Assert.AreEqual(10, first.TestID);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamter()
        {
            object[] paramsArray = { "10" };
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest", paramsArray);
            var last = result.Last();
            Assert.AreEqual("Test10", last.TestName);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterAndCustomMapper()
        {
            object[] paramsArray = { "10" };
            CustomParameterMapper defaultParamMapper = new CustomParameterMapper(sqlDatabase);
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest", defaultParamMapper,
           paramsArray);

            var last = result.Last();
            Assert.AreEqual("Test10", last.TestName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingStoredProcWithParamterAndNullMapper()
        {
            object[] paramsArray = { "1" };
            CustomParameterMapper defaultParamMapper = null;
            sqlDatabase.ExecuteSprocAccessor<Test>("GetTest", defaultParamMapper, paramsArray);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingInValidStoredProcWithParamter()
        {
            object[] paramsArray = { "1" };
            CustomParameterMapper defaultParamMapper = new CustomParameterMapper(sqlDatabase);
            sqlDatabase.ExecuteSprocAccessor<Test>("GetTestInvalid", defaultParamMapper, paramsArray);

        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void FormatExceptionIsThrownWhenExecutingStoredProcWithInValidParamter()
        {
            // Expected parameter type is int but invalid parameter string is passed.
            object[] paramsArray = { "a" };
            CustomParameterMapper defaultParamMapper = new CustomParameterMapper(sqlDatabase);
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest", defaultParamMapper,
           paramsArray);

            result.First();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingStoredProcWithMoreParamters()
        {
            // Expected number of parameter is one but we try to pass more parameters.
            object[] paramsArray = { "1", "2" };
            sqlDatabase.ExecuteSprocAccessor<Test>("GetTest", paramsArray);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void SqlExceptionIsThrownWhenExecutingStoredProcWithLessParamters()
        {
            // Expected number of parameter is one but we do not pass any paramters.
            object[] paramsArray = { };

            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest", paramsArray);

            result.First();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterRowMapping()
        {
            object[] paramsArray = { "10" };
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest",
           MapBuilder<Test>.MapAllProperties()
           .DoNotMap(s => s.CreatedDate)
           .Build(),
           paramsArray);

            var first = result.First();
            Assert.AreEqual("Test10", first.TestName);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterWithNoMappings()
        {
            object[] paramsArray = { "10" };
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTest",
           MapBuilder<Test>.MapNoProperties()
           .Build(),
           paramsArray);

            var first = result.First();

            Assert.AreEqual(default(int), first.TestID);
            Assert.AreEqual(default(string), first.TestName);
            Assert.AreEqual(default(string), first.TestDescription);
            Assert.AreEqual(null, first.BugsCreated);
            Assert.AreEqual(default(DateTime), first.CreatedDate);
            Assert.AreEqual(null, first.UpdatedDate);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterWithFunc()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapAllProperties()
               .Map(p => p.BugsCreated)
               .WithFunc(r => r.IsDBNull(3) ? -1 : r.GetInt32(3)).Build());



            var first = result.First();
            Assert.AreEqual("Test1", first.TestName);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcSameResultSetMultipleOperationsDoesNotEscalateToDTC()
        {
            SqlDatabase sqlDatabase1 = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync") as SqlDatabase;
            ServiceHelper.Stop("MSDTC");
            using (TransactionScope scope = new TransactionScope())
            {
                var result = sqlDatabase1.ExecuteSprocAccessor<Sale>("SalesByCategory",
               MapBuilder<Sale>.MapAllProperties()
               .DoNotMap(s => s.Quantity)
               .Build(),
               "Beverages", "1998");

                int count = result.Count();
                var first = result.First();
                var second = result.Skip(1).First();
                var resultSubset = from r in result where r.ProductName == "Chai" select r;
                int countSubset = resultSubset.Count();



                scope.Complete();
            }
            ServiceHelper.Start("MSDTC");

        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterUsingMapByName()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapNoProperties()
               .MapByName(a => a.TestName)
               .Build());

            var first = result.First();

            Assert.AreEqual("Test1", first.TestName);
            Assert.AreEqual(default(int), first.TestID);
            Assert.AreEqual(default(string), first.TestDescription);
            Assert.AreEqual(null, first.BugsCreated);
            Assert.AreEqual(default(DateTime), first.CreatedDate);
            Assert.AreEqual(null, first.UpdatedDate);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterUsingMapByNameWithPropInfo()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapNoProperties()
               .MapByName(typeof(Test).GetProperty("TestID"))
               .MapByName(typeof(Test).GetProperty("TestName"))
               .Build());

            var first = result.First();

            Assert.AreEqual(1, first.TestID);
            Assert.AreEqual("Test1", first.TestName);
            Assert.AreEqual(default(string), first.TestDescription);
            Assert.AreEqual(null, first.BugsCreated);
            Assert.AreEqual(default(DateTime), first.CreatedDate);
            Assert.AreEqual(null, first.UpdatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingStoredProcWithParamterUsingMapByNameWithInvalidProp()
        {
            sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapNoProperties()
               .MapByName(typeof(Test).GetProperty("TestID"))
               .MapByName(typeof(Test).GetProperty("InvalidProperty"))
               .Build());
        }

        [TestMethod]
        [Ignore] // until raised bug is fixed
        public void ResultsAreReturnedWhenExecutingStoredProcWithParamterWithToColumn()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapAllProperties()
               .Map(a => a.UpdatedDate)
               .ToColumn("CreatedDate")
               .Build());

            var first = result.First();

            Assert.AreEqual(1, first.TestID);
            Assert.AreEqual("Test1", first.TestName);
            Assert.AreEqual("Test1", first.TestDescription);
            Assert.AreEqual(null, first.BugsCreated);
            Assert.AreEqual(first.CreatedDate, first.UpdatedDate);
        }

        [TestMethod]
        [Ignore] //until raised bug is fixed
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidOperationExceptionIsThrownWhenExecutingStoredProcWithParamterWithEmptyToColumn()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapAllProperties()
               .Map(a => a.CreatedDate)
               .ToColumn(string.Empty)
               .Build());

            result.ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void InvalidCastExceptionIsThrownWhenExecutingStoredProcWithParamterWithToColumnInvalidTypeCast()
        {
            IEnumerable<Test> result = sqlDatabase.ExecuteSprocAccessor<Test>("GetTestData",
               MapBuilder<Test>.MapAllProperties()
               .Map(a => a.CreatedDate)
               .ToColumn("TestID")
               .Build());

            result.ToList();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessor()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<TopTenProduct> accessor = db.CreateSprocAccessor<TopTenProduct>("Ten Most Expensive Products");
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<TopTenProduct>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<TopTenProduct> resultSet = (IEnumerable<TopTenProduct>)state.State;

            object[] paramsArray = { };
            DataSet ds = db.ExecuteDataSet("Ten Most Expensive Products", paramsArray);
            Assert.IsNull(state.Exception);
            Assert.AreEqual<int>(ds.Tables[0].Rows.Count, resultSet.Count());
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorInValidStoredProc()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<Test> accessor = db.CreateSprocAccessor<Test>("GetTestData1");
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<Test>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<Test> resultSet = (IEnumerable<Test>)state.State;
            if (state.Exception != null)
            {
                Console.WriteLine(state.Exception.ToString());
            }
            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncStoredProcAccessorWithNullParameterMapper()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            CustomParameterMapper defaultParamMapper = null;
            db.CreateSprocAccessor<TopTenProduct>("GetTestData", defaultParamMapper);
        }


        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorWithMapAll()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<TopTenProduct> accessor = db.CreateSprocAccessor<TopTenProduct>("Ten Most Expensive Products", MapBuilder<TopTenProduct>.MapAllProperties().Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<TopTenProduct>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<TopTenProduct> resultSet = (IEnumerable<TopTenProduct>)state.State;
            var first = resultSet.First();

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);
            Assert.AreEqual("Côte de Blaye", first.TenMostExpensiveProducts);
            Assert.AreEqual(263, (int)first.UnitPrice);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorMapNoPropertiesWithFunc()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<TopTenProduct> accessor = db.CreateSprocAccessor<TopTenProduct>("Ten Most Expensive Products", MapBuilder<TopTenProduct>.MapNoProperties()
                .Map(s => s.UnitPrice)
                .WithFunc(r => r.GetDecimal(1))
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<TopTenProduct>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<TopTenProduct> resultSet = (IEnumerable<TopTenProduct>)state.State;
            var first = resultSet.First();

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);


            Assert.AreEqual(default(string), first.TenMostExpensiveProducts);
            Assert.AreEqual(263, (int)first.UnitPrice);
        }

        [TestMethod]
        [Ignore] // until raised bug is fixed
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorWithNullableProperties()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<Test> accessor = db.CreateSprocAccessor<Test>("GetTestData", MapBuilder<Test>.MapAllProperties()
           .DoNotMap(s => s.CreatedDate)
           .Build());

            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<Test>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<Test> resultSet = (IEnumerable<Test>)state.State;

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);

            var first = resultSet.First();
            var last = resultSet.Last();
            Assert.AreEqual("Test10", last.TestName);
            Assert.AreEqual(null, last.BugsCreated);
            Assert.AreEqual("Test1", first.TestName);
            Assert.AreEqual(1, first.BugsCreated);
        }

        [TestMethod]
        [Ignore] // until raised bug is fixed
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncStoredProcAccessorWithRowMapperWithNullFunction()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<Test> accessor = db.CreateSprocAccessor<Test>("GetTestData", MapBuilder<Test>.MapAllProperties()
                .Map(p => p.TestDescription)
                .WithFunc(null)
                .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<Test>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<Test> resultSet = (IEnumerable<Test>)state.State;
            resultSet.First();
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorMapByName()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<TopTenProduct> accessor = db.CreateSprocAccessor<TopTenProduct>("Ten Most Expensive Products", MapBuilder<TopTenProduct>.MapNoProperties()
               .MapByName(a => a.TenMostExpensiveProducts)
               .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<TopTenProduct>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<TopTenProduct> resultSet = (IEnumerable<TopTenProduct>)state.State;

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);

            var first = resultSet.First();

            Assert.AreEqual("Côte de Blaye", first.TenMostExpensiveProducts);
            Assert.AreEqual(default(decimal), first.UnitPrice);
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorMapByNameWithPropInfo()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<Test> accessor = db.CreateSprocAccessor<Test>("GetTestData", MapBuilder<Test>.MapNoProperties()
               .MapByName(typeof(Test).GetProperty("TestID"))
               .MapByName(typeof(Test).GetProperty("TestName"))
               .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<Test>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<Test> resultSet = (IEnumerable<Test>)state.State;

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);

            var first = resultSet.First();

            Assert.AreEqual(1, first.TestID);
            Assert.AreEqual("Test1", first.TestName);
            Assert.AreEqual(default(string), first.TestDescription);
            Assert.AreEqual(null, first.BugsCreated);
            Assert.AreEqual(default(DateTime), first.CreatedDate);
            Assert.AreEqual(null, first.UpdatedDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNullExceptionIsThrownWhenExecutingAsyncStoredProcAccessorMapByNameWithInvalidProp()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            db.CreateSprocAccessor<Test>("GetTestData", MapBuilder<Test>.MapNoProperties()
               .MapByName(typeof(Test).GetProperty("TestID"))
               .MapByName(typeof(Test).GetProperty("InvalidProperty"))
               .Build());
        }

        [TestMethod]
        public void ResultsAreReturnedWhenExecutingAsyncStoredProcAccessorWithRowMapperToColumn()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<TopTenProductWithSalePrice> accessor = db.CreateSprocAccessor<TopTenProductWithSalePrice>("Ten Most Expensive Products", MapBuilder<TopTenProductWithSalePrice>.MapAllProperties()
               .Map(p => p.SellingPrice)
               .ToColumn("UnitPrice").Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<TopTenProductWithSalePrice>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<TopTenProductWithSalePrice> resultSet = (IEnumerable<TopTenProductWithSalePrice>)state.State;

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);

            var first = resultSet.First();
            Assert.AreEqual(first.SellingPrice, first.UnitPrice);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void InvalidCastExceptionIsThrownWhenExecutingAsyncStoredProcAccessorWithRowMapperToColumnInvalidTypeCast()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DataAccessor<Test> accessor = db.CreateSprocAccessor<Test>("GetTestData", MapBuilder<Test>.MapAllProperties()
               .Map(a => a.CreatedDate)
               .ToColumn("TestID")
               .Build());
            AsyncCallback cb = new AsyncCallback(EndExecuteAccessor<Test>);
            DbAsyncState state = new DbAsyncState(db, accessor);
            IAsyncResult result = accessor.BeginExecute(cb, state);
            state.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 15));
            IEnumerable<Test> resultSet = (IEnumerable<Test>)state.State;

            if (state.Exception != null)
                Console.WriteLine(state.Exception);
            Assert.IsNull(state.Exception);
            resultSet.ToList();
        }

        public void EndExecuteAccessor<T>(IAsyncResult result)
        {
            DaabAsyncResult daabResult = (DaabAsyncResult)result;
            DbAsyncState state = (DbAsyncState)daabResult.AsyncState;
            try
            {
                DataAccessor<T> accessor = (DataAccessor<T>)state.Accessor;
                state.State = accessor.EndExecute(result);
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

