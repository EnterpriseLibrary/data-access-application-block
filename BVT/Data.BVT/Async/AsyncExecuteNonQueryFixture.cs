// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class AsyncExecuteNonQueryFixture : AsyncFixtureBase
    {
        public AsyncExecuteNonQueryFixture()
            : base("ConfigFiles.AsyncExecuteNonQueryFixture.config")
        {
        }

        # region Helpers

        private DbAsyncState BeginExecuteNonQuery(Database db, DbCommand command, DbTransaction trans, AsyncCallback cb)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteNonQuery(command, trans, cb, (object)stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteNonQuery(Database db, DbCommand command, AsyncCallback cb)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteNonQuery(command, cb, (object)stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteNonQuery(Database db, CommandType type, string text, AsyncCallback cb)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteNonQuery(type, text, cb, (object)stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteNonQuery(Database db, string storedProcName, AsyncCallback cb, params object[] paramsArray)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteNonQuery(storedProcName, cb, stateObject, paramsArray);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteNonQuery(Database db, DbTransaction transaction, CommandType type, string commandText, AsyncCallback cb, params object[] paramsArray)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteNonQuery(transaction, type, commandText, cb, stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private void EndExecuteNonQuery(IAsyncResult result)
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
                state.State = (object)state.Database.EndExecuteNonQuery(blockResult);
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
        public void RecordIsInsertedWhenExecutingNonQueryWithSqlCommandTypeAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            try
            {
                AsyncCallback cb = new AsyncCallback(EndExecuteNonQuery);
                DbAsyncState state = BeginExecuteNonQuery(db, CommandType.Text, InsertCategorySql, cb);
                DbAsyncState state1 = BeginExecuteNonQuery(db, CommandType.Text, InsertCategory1Sql, cb);
                Assert.AreEqual<int>(1, (int)state.State);
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void RecordIsNotInsertedWhenColumnNameInvalidAndExecutingNonQueryWithTransactionCommandAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            try
            {
                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();

                    AsyncCallback cb = new AsyncCallback(EndExecuteNonQuery);
                    DbCommand command = new SqlCommand();

                    command.CommandText = InsertCategorySql;
                    command.CommandType = CommandType.Text;
                    DbCommand command1 = new SqlCommand();

                    command1.CommandText = InsertCategory123Sql;
                    command1.CommandType = CommandType.Text;

                    DbAsyncState state = BeginExecuteNonQuery(db, command, transaction, cb);

                    DbAsyncState state1 = BeginExecuteNonQuery(db, command1, transaction, cb);
                    if (state1.Exception != null)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                    int rowCount = (int)db.ExecuteScalar(CommandType.Text, GetTestCategoryCountQuery);
                    Assert.AreEqual<int>(0, rowCount);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state1.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void RecordIsInsertedWhenExecutingNonQueryWithSqlCommandAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            try
            {
                AsyncCallback cb = new AsyncCallback(EndExecuteNonQuery);
                DbCommand command = new SqlCommand();
                command.CommandText = InsertCategorySql;
                command.CommandType = CommandType.Text;

                DbAsyncState state = BeginExecuteNonQuery(db, command, cb);
                Assert.AreEqual<int>(1, (int)state.State);
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void NegativeValueIsReturnedWhenExecutingNonQueryWithStoredProcParamsAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            AsyncCallback cb = new AsyncCallback(EndExecuteNonQuery);
            object[] paramsArray = { "Beverages", "1996" };
            DbAsyncState state = BeginExecuteNonQuery(db, "SalesByCategory", cb, paramsArray);
            Assert.AreEqual<int>(-1, (int)state.State);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void RecordsAreInsertedAndThenRolledbackWhenExecutingNonQueryWithTransactionCommandTextAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbAsyncState state = null;
            DbAsyncState state1 = null;
            DbAsyncState state2 = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();

                    AsyncCallback cb = new AsyncCallback(EndExecuteNonQuery);
                    state = BeginExecuteNonQuery(db, transaction, CommandType.Text, InsertCategorySql, cb);
                    state1 = BeginExecuteNonQuery(db, transaction, CommandType.Text, InsertCategorySql, cb);
                    state2 = BeginExecuteNonQuery(db, transaction, CommandType.Text, InsertCategory123Sql, cb);
                    if (state2.Exception != null)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                    int rowCount = (int)db.ExecuteScalar(CommandType.Text, GetTestCategoryCountQuery);
                    Assert.AreEqual<int>(1, (int)state.State);
                    Assert.AreEqual<int>(1, (int)state1.State);
                    Assert.AreEqual<int>(0, rowCount);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state1.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        private object EmptyCallback(IAsyncResult ar)
        {
            return null;
        }

        private object TriggerCallback(IAsyncResult ar)
        {
            var asyncState = ar.AsyncState as DbAsyncState;

            asyncState.AutoResetEvent.Set();

            return null;
        }

        private object WorkCallback(IAsyncResult ar)
        {
            var asyncState = ar.AsyncState as DbAsyncState;

            try
            {
                int scalarValue = (int)asyncState.Database.EndExecuteScalar(asyncState.AsyncResult);
                Assert.AreEqual(3, scalarValue);
            }
            catch (Exception e)
            {
                asyncState.Exception = e;
            }

            asyncState.AutoResetEvent.Set();

            return null;
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingCommandTextAndWorkCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            var asyncState = GetNewStateObject(db);

            asyncState.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(CommandType.Text, GetProductsCountForOrderQuery, ar => WorkCallback(ar), asyncState);

            asyncState.AutoResetEvent.WaitOne();

            Assert.IsNull(asyncState.Exception);
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingCommandText()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            var asyncState = GetNewStateObject(db);

            asyncState.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(CommandType.Text, GetProductsCountForOrderQuery, ar => EmptyCallback(ar), asyncState);

            int scalarValue = (int)asyncState.Database.EndExecuteScalar(asyncState.AsyncResult);
            Assert.AreEqual(3, scalarValue);
        }

        [TestMethod]
        public void ValueIsReturnedWhenUsingCommandTextAndTriggerCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");

            var asyncState = GetNewStateObject(db);

            asyncState.AsyncResult = (DaabAsyncResult)db.BeginExecuteScalar(CommandType.Text, GetProductsCountForOrderQuery, ar => TriggerCallback(ar), asyncState);

            asyncState.AutoResetEvent.WaitOne();

            int scalarValue = (int)asyncState.Database.EndExecuteScalar(asyncState.AsyncResult);
            Assert.AreEqual(3, scalarValue);
        }

        [TestMethod]
        public void RecordIsInsertedWhenExecutingNonQueryWithSqlCommandAndNullCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            try
            {
                DbCommand command = new SqlCommand();
                command.CommandText = InsertCategorySql;
                command.CommandType = CommandType.Text;
                DbAsyncState state = new DbAsyncState(db);

                DaabAsyncResult result = (DaabAsyncResult)db.BeginExecuteNonQuery(command, null, state);
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, 20));
                int rowCount = (int)db.ExecuteScalar(CommandType.Text, GetTestCategoryCountQuery);

                Assert.AreEqual<int>(1, rowCount);
                result.Connection.Close();

            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }
    }
}

