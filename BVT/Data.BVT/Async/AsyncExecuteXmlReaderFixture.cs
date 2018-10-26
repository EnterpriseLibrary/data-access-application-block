// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT
{
    [TestClass]
    public class AsyncExecuteXmlReaderFixture : AsyncFixtureBase
    {
        public AsyncExecuteXmlReaderFixture()
            : base("ConfigFiles.AsyncExecuteXmlReaderFixture.config")
        {
        }

        #region Helpers

        private DbAsyncState BeginExecuteXmlReader(SqlDatabase db, DbCommand command, AsyncCallback cb)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteXmlReader(command, cb, (object)stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private DbAsyncState BeginExecuteXmlReader(SqlDatabase db, DbCommand command, DbTransaction transaction, AsyncCallback cb)
        {
            DbAsyncState stateObject = GetNewStateObject(db);
            stateObject.AsyncResult = (DaabAsyncResult)db.BeginExecuteXmlReader(command, transaction, cb, (object)stateObject);
            bool stateSignalled = stateObject.AutoResetEvent.WaitOne(new TimeSpan(0, 0, 20));

            if (stateSignalled == false && cb != null) throw new Exception("Callback thread did not raise the event, test failed");
            return stateObject;
        }

        private void EndExecuteXmlReaderCallBack(IAsyncResult result)
        {
            DbAsyncState state = null;
            try
            {
                // Retrieve the original command object, passed
                // to this procedure in the AsyncState property
                // of the IAsyncResult parameter.
                DaabAsyncResult blockResult = (DaabAsyncResult)result;
                state = (DbAsyncState)blockResult.AsyncState;
                SqlDatabase db = (SqlDatabase)state.Database;

                state.State = (object)db.EndExecuteXmlReader(blockResult);
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
        public void AllRecordsAreReturnedWhenExecutingExecuteXmlReaderWithSqlCommandAndWithCallBack()
        {
            DbAsyncState state;
            DataTable dt = new DataTable();
            DbCommand command = new SqlCommand();
            DbCommand command1 = new SqlCommand();
            AsyncCallback cb = new AsyncCallback(EndExecuteXmlReaderCallBack);
            int rowCount = 0;
            SqlDatabase db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync") as SqlDatabase;

            //Get the row count synchronously.
            command1.CommandType = CommandType.Text;
            command1.CommandText = "SELECT count(*) FROM Employees";
            int originalCount = (int)db.ExecuteScalar(command1);

            // Get the data Asynchronously.
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT * FROM Employees FOR XML AUTO, XMLDATA";
            state = BeginExecuteXmlReader(db, command, cb);

            using (System.Xml.XmlReader xmlr = (System.Xml.XmlReader)state.State)
            {
                while (!xmlr.EOF)
                {
                    xmlr.Read();
                    xmlr.MoveToElement();
                    if (xmlr.Name.Contains("Employees"))
                        rowCount++;
                }
            }

            // Compare the row count that is fetched both by synchronously and Asynchronously.                
            Assert.AreEqual<int>(originalCount, rowCount);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, state.AsyncResult.Connection.State);
        }

        [TestMethod]
        public void AllRecordsAreReturnedWhenExecutingExecuteXmlReaderWithSqlCommandTransactionAndWithCallBack()
        {
            SqlDatabase db = new DatabaseProviderFactory(base.ConfigurationSource).Create("DefaultSqlAsync") as SqlDatabase;
            try
            {
                DbAsyncState state;
                DataTable dt = new DataTable();
                DbCommand command = new SqlCommand();
                DbCommand command1 = new SqlCommand();
                AsyncCallback cb = new AsyncCallback(EndExecuteXmlReaderCallBack);
                int originalCount = 1;
                int rowCount = 0;

                using (SqlConnection connection = new SqlConnection(db.ConnectionString))
                {
                    connection.Open();
                    DbTransaction transaction = connection.BeginTransaction();

                    // Execute the NonQuery insert statement synchronously.                    
                    command1.CommandType = CommandType.Text;
                    command1.CommandText = InsertCategorySql;
                    db.ExecuteNonQuery(command1, transaction);
                    // Since one row is inserted.
                    originalCount = 1;

                    // Get the data Asynchronously.
                    command.CommandType = CommandType.Text;
                    command.CommandText = GetTestCategoryQuery + " FOR XML AUTO, XMLDATA";

                    state = BeginExecuteXmlReader(db, command, transaction, cb);
                    using (System.Xml.XmlReader xmlr = (System.Xml.XmlReader)state.State)
                    {
                        while (!xmlr.EOF)
                        {
                            xmlr.Read();
                            xmlr.MoveToElement();
                            if (xmlr.Name.Contains("Categories"))
                                rowCount++;
                        }
                    }

                    transaction.Commit();
                    Assert.AreEqual<int>(originalCount, rowCount);
                    Assert.AreEqual<ConnectionState>(ConnectionState.Open, state.AsyncResult.Connection.State);
                }
            }
            finally
            {
                db.ExecuteNonQuery(CommandType.Text, DeleteCategoriesSql);
            }
        }
    }
}

