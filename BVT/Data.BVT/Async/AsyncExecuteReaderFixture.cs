// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class AsyncExecuteReaderFixture : AsyncFixtureBase
    {
        protected const string GetCategoryByNameQuery = "Select * from Categories where CategoryName=@Category";
        protected const string GetAllCategoriesQuery = "Select * from Categories";

        public AsyncExecuteReaderFixture()
            : base("ConfigFiles.AsyncExecuteReaderFixture.config")
        {
        }

        #region Helpers

        private DbAsyncState BeginExecuteReader(Database db, CommandType type, string text, AsyncCallback cb)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteReader(type, text, cb, (object)stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteReader(Database db, DbCommand command, AsyncCallback cb)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteReader(command, cb, (object)stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteReader(Database db, DbCommand command, DbTransaction trans, AsyncCallback cb)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteReader(command, trans, cb, (object)stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteReader(Database db, DbTransaction transaction, CommandType type, string commandText, AsyncCallback cb, params object[] paramsArray)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteReader(transaction, type, commandText, cb, stateObject);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteReader(Database db, string storedProcName, AsyncCallback cb, params object[] paramsArray)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteReader(storedProcName, cb, stateObject, paramsArray);

            return this.BeginExecute(db, cb, execute);
        }

        private DbAsyncState BeginExecuteReader(Database db, string storedProcName, DbTransaction trans, AsyncCallback cb, params object[] paramsArray)
        {
            Func<DbAsyncState, DaabAsyncResult> execute = (stateObject) => (DaabAsyncResult)db.BeginExecuteReader(trans, storedProcName, cb, (object)stateObject, paramsArray);

            return this.BeginExecute(db, cb, execute);
        }

        private void EndExecuteReaderCallBack(IAsyncResult result)
        {
            DbAsyncState state = null;
            try
            {
                // Retrieve the original command object, passed
                // to this procedure in the AsyncState property
                // of the IAsyncResult parameter.
                DaabAsyncResult blockResult = (DaabAsyncResult)result;
                state = (DbAsyncState)blockResult.AsyncState;

                state.State = (object)state.Database.EndExecuteReader(blockResult);
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
        public void RecordsAreReturnedWhenExecutingExecuteReaderWithCommandTypeAndTextWithCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbCommand syncCommand = new SqlCommand();
            syncCommand.CommandType = CommandType.Text;
            syncCommand.CommandText = GetCategoryCountQuery;

            int expectedRowCount = (int)db.ExecuteScalar(syncCommand);
            DbAsyncState state;
            DataTable dt = new DataTable();

            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);

            // Get the data Asynchronously.
            state = BeginExecuteReader(db, CommandType.Text, GetAllCategoriesQuery, cb);
            IDataReader reader = (IDataReader)state.State;

            dt.Load((IDataReader)reader);
            reader.Close();

            // Get the data row count synchronously and compare it with the row count returned from the Asynchronous data.
            Assert.AreEqual<int>(expectedRowCount, dt.Rows.Count);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void RecordsAreReturnedWhenExecutingExecuteReaderWithSqlCommandAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbAsyncState state;
            DataTable dt = new DataTable();
            DbCommand command = new SqlCommand();
            command.CommandText = GetAllCategoriesQuery;
            command.CommandType = CommandType.Text;

            DbCommand syncCommand = new SqlCommand();
            syncCommand.CommandType = CommandType.Text;
            syncCommand.CommandText = GetCategoryCountQuery;
            int expectedRowCount = (int)db.ExecuteScalar(syncCommand);

            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);

            // Get the data Asynchronously.
            state = BeginExecuteReader(db, command, cb);
            IDataReader reader = (IDataReader)state.State;
            dt.Load(reader);

            Assert.AreEqual<int>(expectedRowCount, dt.Rows.Count);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void RecordsAreReturnedWhenExecutingExecuteReaderWithStoredProcParamsAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);
            object[] paramsArray = { "10248" };
            DataSet ds = db.ExecuteDataSet("CustOrdersDetail", paramsArray);
            int expectedRowCount = ds.Tables[0].Rows.Count;
            DataTable dt = new DataTable();
            DbAsyncState state = BeginExecuteReader(db, "CustOrdersDetail", cb, paramsArray);
            IDataReader reader = (IDataReader)state.State;
            dt.Load(reader);
            reader.Close();

            Assert.AreEqual<int>(expectedRowCount, dt.Rows.Count);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }


        [TestMethod]
        public void RecordsAreReturnedWhenExecutingExecuteReaderWithTransactionCommandAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            try
            {
                DbCommand command = new SqlCommand();
                AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);
                DbAsyncState state;
                DataTable dt = new DataTable();

                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();

                    // Execute the NonQuery insert statement synchronously.
                    DbCommand command1 = new SqlCommand();
                    command1.CommandType = CommandType.Text;
                    command1.CommandText = InsertCategorySql;
                    db.ExecuteNonQuery(command1, transaction);
                    command.CommandText = GetTestCategoryQuery;
                    command.CommandType = CommandType.Text;
                    state = BeginExecuteReader(db, command, transaction, cb);
                    dt.Load((IDataReader)state.State);
                    transaction.Commit();

                    Assert.AreEqual<int>(1, dt.Rows.Count);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void CategoryIsInsertedWhenExecutingExecuteReaderWithTransactionCommandTextAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbCommand command1 = new SqlCommand();
            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);
            DbAsyncState state;
            DbTransaction transaction;
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    // Execute the NonQuery insert statement synchronously.                    
                    command1.CommandType = CommandType.Text;
                    command1.CommandText = InsertCategorySql;
                    db.ExecuteNonQuery(command1, transaction);

                    // Get the records using overloaded BeginExecuteReader Asynchronously with DBTransaction.                    
                    state = BeginExecuteReader(db, transaction, CommandType.Text, GetTestCategoryQuery, cb);

                    dt.Load((IDataReader)state.State);

                    // If exception is thrown then rollback else commit.
                    if (state.Exception != null)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }

                    Assert.AreEqual<int>(1, dt.Rows.Count);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }

        [TestMethod]
        public void RecordIsInsertedWhenExecutingExecuteReaderWithTransactionStoredProcParamsAndCallBack()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbCommand command1 = new SqlCommand();
            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);
            DbAsyncState state;
            DbTransaction transaction;
            DataTable dt = new DataTable();
            object[] paramsArray = { "BONAP" };

            DataSet ds = db.ExecuteDataSet("CustOrdersOrders", paramsArray);
            int originalCount = ds.Tables[0].Rows.Count;
            try
            {
                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    // Execute the NonQuery insert statement synchronously.                    
                    command1.CommandType = CommandType.Text;
                    command1.CommandText = @"INSERT INTO [Northwind].[dbo].[Orders]
                                                   ([CustomerID]
                                                   ,[EmployeeID]
                                                   ,[OrderDate]
                                                   ,[RequiredDate]
                                                   ,[ShippedDate]
                                                   ,[ShipVia]
                                                   ,[Freight]
                                                   ,[ShipName]
                                                   ,[ShipAddress]
                                                   ,[ShipCity]
                                                   ,[ShipRegion]
                                                   ,[ShipPostalCode]
                                                   ,[ShipCountry])
                                             VALUES
                                                   ('BONAP'
                                                   ,1
                                                   ,GETDATE()
                                                   ,GETDATE()
                                                   ,NULL
                                                   ,2
                                                   ,10
                                                   ,'TestShip'
                                                   ,'TestShip address'
                                                   ,'Marseille'
                                                   ,NULL
                                                   ,'13008'
                                                   ,'France')";
                    db.ExecuteNonQuery(command1, transaction);

                    // Get the records using overloaded BeginExecuteReader with stored proc and params Asynchronously with DBTransaction.                                        
                    state = BeginExecuteReader(db, "CustOrdersOrders", transaction, cb, paramsArray);
                    dt.Load((IDataReader)state.State);

                    transaction.Commit();

                    Assert.AreEqual<int>(originalCount + 1, dt.Rows.Count);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, "DELETE FROM Orders WHERE ShipName = \'TestShip\'");
            }
        }

        [TestMethod]
        public void ReaderIsOpenWhenExecutingExecuteReaderWithParameterizedQuery()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            int expectedRowCount = (int)db.ExecuteScalar(CommandType.Text, "Select count(*) from Categories where CategoryName=\'Seafood\'");
            DbAsyncState state;
            DataTable dt = new DataTable();
            SqlCommand command = new SqlCommand();
            command.CommandText = GetCategoryByNameQuery;
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@Category", SqlDbType.VarChar, 15);
            command.Parameters[0].Value = "Seafood";

            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);

            // Get the data Asynchronously.
            state = BeginExecuteReader(db, command, cb);
            IDataReader reader = (IDataReader)state.State;

            Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
            Assert.AreEqual<bool>(false, reader.IsClosed);
            reader.Close();
        }

        [TestMethod]
        public void ExceptionIsReturnedWhenExecutingExecuteReaderWithParameterizedQueryWithInvalidParameterName()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbAsyncState state;
            DataTable dt = new DataTable();
            SqlCommand command = new SqlCommand();
            command.CommandText = GetCategoryByNameQuery;
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@Category1", SqlDbType.VarChar, 15);
            command.Parameters[0].Value = "Seafood";

            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);

            // Get the data Asynchronously.
            state = BeginExecuteReader(db, command, cb);

            IDataReader reader = (IDataReader)state.State;
            if (state.Exception != null)
            {
                Console.WriteLine(state.Exception.ToString());

            }
            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));

            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void ExceptionIsReturnedWhenExecutingExecuteReaderWithParameterizedQueryWithNoParameter()
        {
            Database db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync");
            DbAsyncState state;
            DataTable dt = new DataTable();
            SqlCommand command = new SqlCommand();
            command.CommandText = GetCategoryByNameQuery;
            command.CommandType = CommandType.Text;

            AsyncCallback cb = new AsyncCallback(EndExecuteReaderCallBack);

            // Get the data Asynchronously.
            state = BeginExecuteReader(db, command, cb);
            IDataReader reader = (IDataReader)state.State;

            if (state.Exception != null)
            {
                Console.WriteLine(state.Exception.ToString());

            }
            Assert.IsInstanceOfType(state.Exception, typeof(SqlException));

            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }
    }
}

