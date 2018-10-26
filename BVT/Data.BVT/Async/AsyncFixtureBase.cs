// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    public class AsyncFixtureBase : EntLibFixtureBase
    {
        protected const string InsertCategorySql = "Insert into Categories(CategoryName,Description) values(\'TestCategory\',\'TestDescription\')";
        protected const string InsertCategory1Sql = "Insert into Categories(CategoryName,Description) values(\'TestCategory1\',\'TestDescription1\')";
        protected const string GetProductsCountForOrderQuery = "SELECT Count(ProductName) FROM Products P, [Order Details] Od WHERE Od.ProductID = P.ProductID and Od.OrderID = 10248";
        protected const string DeleteCategoriesSql = "delete from Categories where CategoryName=\'TestCategory\'";
        protected const string InsertCategory123Sql = "Insert into Categories(CategoryName123,Description) values(\'TestCategory\',\'TestDescription\')";
        protected const string GetTestCategoryCountQuery = "Select count(*) from Categories where CategoryName=\'TestCategory\'";
        protected const string GetCategoryCountQuery = "Select count(*) from Categories";
        protected const string GetTestCategoryQuery = "Select * from Categories where CategoryName=\'TestCategory\'";

        public AsyncFixtureBase(string configSourceFileName)
            : base(configSourceFileName)
        { }

        public TestContext TestContext
        {
            get;
            set;
        }

        protected DbAsyncState GetNewStateObject(Database db)
        {
            return new DbAsyncState(db);
        }

        protected DbAsyncState BeginExecute(Database db, AsyncCallback cb, Func<DbAsyncState, DaabAsyncResult> execute)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = execute(stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }
    }
}

