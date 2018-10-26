// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class AsyncExecuteScalarFixture : AsyncFixtureBase
    {
        protected const string GetCategoryIdByName = "Select CategoryID from Categories where CategoryName=@Category";

        public AsyncExecuteScalarFixture()
            : base("ConfigFiles.AsyncExecuteScalarFixture.config")
        {
        }

        # region Helpers

        private DbAsyncState BeginExecuteScalar(Database db, DbCommand command, DbTransaction trans, AsyncCallback cb)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(command, trans, cb, (object)stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private DbAsyncState BeginExecuteScalar(Database db, DbCommand command, AsyncCallback cb)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(command, cb, (object)stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private DbAsyncState BeginExecuteScalar(Database db, CommandType type, string text, AsyncCallback cb)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(type, text, cb, (object)stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private DbAsyncState BeginExecuteScalar(Database db, string storedProcName, AsyncCallback cb, params object[] paramsArray)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(storedProcName, cb, stateObject, paramsArray);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private DbAsyncState BeginExecuteScalar(Database db, DbTransaction transaction, CommandType type, string commandText, AsyncCallback cb, params object[] paramsArray)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(transaction, type, commandText, cb, stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private DbAsyncState BeginExecuteScalar(Database db, DbTransaction transaction, string storedProc, AsyncCallback cb, params object[] paramsArray)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(transaction, storedProc, cb, stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private void EndExecuteScalar(IAsyncResult result)
        {
            SqlConnection connection = null;
            DbAsyncState state = null;
            try
            {
                // Retrieve the original command object, passed
                // to this procedure in the AsyncState property
                // of the IAsyncResult parameter.
                DaabAsyncResult blockResult = (DaabAsyncResult)result;
                SqlCommand command = (SqlCommand)blockResult.Command;
                state = (DbAsyncState)blockResult.AsyncState;

                connection = command.Connection;
                state.State = (object)state.Database.EndExecuteScalar(blockResult);
                state.AsyncResult = blockResult;
            }
            catch (Exception exp)
            {
                state.Exception = exp;
            }
            finally
            {
                if (state != null)
                {
                    state.AutoResetEvent.Set();
                }
            }
        }

        #endregion

        [TestMethod]
        public void ValueIsReturnedWhenExecutingScalarWithSqlCommandTypeAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
            DbAsyncState state = BeginExecuteScalar(db, CommandType.Text, "Select CategoryID from Categories where CategoryName=\'Seafood\'", cb);
            Assert.AreEqual<int>(8, (int)state.State);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void CorrectCountIsReturnedWhenExecutingScalarWithTransactionCommandAndCallBack()
        {
            string commandText = GetCategoryCountQuery;
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            DbAsyncState state = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();
                    db.ExecuteNonQuery(transaction, CommandType.Text, InsertCategorySql);

                    AsyncCallback cb = new AsyncCallback(EndExecuteScalar);

                    SqlCommand command = new SqlCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = commandText;

                    //within transaction should return initial+1
                    state = BeginExecuteScalar(db, command, transaction, cb);

                    transaction.Rollback();

                    int expected = (int)db.ExecuteScalar(CommandType.Text, commandText);
                    Assert.AreEqual<int>(1, (int)state.State - expected);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void PriceIsUpdatedWhenExecutingScalarWithTransactionStoredProcAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            DbAsyncState state = null;
            string initialPrice = null;
            string initialState = null;

            try
            {
                initialPrice = db.ExecuteScalar(CommandType.Text, "select UnitPrice from Products where ProductName=\'Chai\'").ToString();
                initialState = (string)db.ExecuteScalar(CommandType.Text, "select ProductName from Products where UnitPrice=(select max(UnitPrice) from Products)");

                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();
                    db.ExecuteNonQuery(transaction, CommandType.Text, "update Products set UnitPrice=99999.99 where ProductName=\'Chai\'");
                    AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
                    //within transaction should return chai
                    state = BeginExecuteScalar(db, transaction, "[Ten Most Expensive Products]", cb);
                    transaction.Rollback();
                    Assert.AreNotEqual<string>(initialState, state.State.ToString());
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, "update Products set UnitPrice=" + initialPrice + " where ProductName =\'Chai\'");
            }
        }

        [TestMethod]
        public void CategoryIdIsReturnedWhenExecutingScalarWithCommandAndCallBack()
        {
            string commandText = "Select CategoryID from Categories where CategoryName=\'Seafood\'";
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            DbAsyncState state = null;

            AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            state = BeginExecuteScalar(db, command, cb);
            Assert.AreEqual<int>(8, (int)state.State);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ProductNameIsReturnedWhenExecutingScalarWithStoredProcParamsAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
            object[] paramsArray = { "Beverages", "1996" };
            DbAsyncState state = BeginExecuteScalar(db, "SalesByCategory", cb, paramsArray);
            Assert.AreEqual<string>("Chai", (string)state.State);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void RecordIsInsertedWhenExecutingScalarWithTransactionCommandTextAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbAsyncState state = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();

                    AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
                    db.ExecuteNonQuery(transaction, CommandType.Text, InsertCategorySql);
                    state = BeginExecuteScalar(db, transaction, CommandType.Text, GetCategoryCountQuery, cb);
                    transaction.Rollback();
                    int rowCount = (int)db.ExecuteScalar(CommandType.Text, GetCategoryCountQuery);
                    Assert.AreEqual<int>(1, (int)state.State - rowCount);

                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void CategoryIdIsReturnedWhenExecutingScalarWithParameterizedQueryCommandAndCallBack()
        {
            string commandText = GetCategoryIdByName;
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            DbAsyncState state = null;

            AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
            SqlCommand command = new SqlCommand();
            command.Parameters.Add("@Category", SqlDbType.VarChar, 15);
            command.Parameters[0].Value = "Seafood";

            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            state = BeginExecuteScalar(db, command, cb);
            Assert.AreEqual<int>(8, (int)state.State);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ExceptionIsReturnedWhenExecutingScalarWithParameterizedQueryWithInvalidParameterCommandAndCallBack()
        {
            string commandText = GetCategoryIdByName;
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            DbAsyncState state = null;

            AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
            SqlCommand command = new SqlCommand();
            command.Parameters.Add("@Category1", SqlDbType.VarChar, 15);
            command.Parameters[0].Value = "Seafood";

            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            state = BeginExecuteScalar(db, command, cb);
            if (state.Exception != null)
            {
                Console.WriteLine(state.Exception.ToString());
            }

            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ExceptionIsReturnedWhenExecutingScalarWithParameterizedQueryWithNoParameterCommandAndCallBack()
        {
            string commandText = GetCategoryIdByName;
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            DbAsyncState state = null;

            AsyncCallback cb = new AsyncCallback(EndExecuteScalar);
            SqlCommand command = new SqlCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            state = BeginExecuteScalar(db, command, cb);
            if (state.Exception != null)
            {
                Console.WriteLine(state.Exception.ToString());
            }

            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }
    }
}

