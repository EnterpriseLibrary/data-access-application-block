// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Sql
{
    public partial class SqlDatabase : Database
    {
        private IAsyncResult DoBeginExecuteReader(SqlCommand command, bool disposeCommand, AsyncCallback callback, object state)
        {
            CommandBehavior commandBehavior =
                command.Transaction == null ? CommandBehavior.CloseConnection : CommandBehavior.Default;

            return WrappedAsyncOperation.BeginAsyncOperation(
                callback,
                cb => command.BeginExecuteReader(cb, state, commandBehavior),
                ar => new DaabAsyncResult(ar, command, disposeCommand, false, DateTime.Now));
        }

        /// <summary>
        /// <para>Initiates the asynchronous execution of a <paramref name="command"/> which will return a <see cref="IDataReader"/>.</para>
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="SqlCommand"/> to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both; 
        /// this value is also needed when invoking <see cref="EndExecuteReader"/>, 
        /// which returns the <see cref="IDataReader"/>.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteReader(DbCommand)"/>
        /// <seealso cref="EndExecuteReader(IAsyncResult)"/>
        public override IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, object state)
        {
            SqlCommand sqlCommand = CheckIfSqlCommand(command);

            DbConnection connection = this.GetNewOpenConnection();
            try
            {
                PrepareCommand(sqlCommand, connection);
                return DoBeginExecuteReader(sqlCommand, false, callback, state);
            }
            catch
            {
                connection.Close();
                throw;
            }
        }

        /// <summary>
        /// <para>Initiates the asynchronous execution of a <paramref name="command"/> inside a transaction which will return a <see cref="IDataReader"/>.</para>
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="SqlCommand"/> to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="DbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both; 
        /// this value is also needed when invoking <see cref="EndExecuteReader"/>, 
        /// which returns the <see cref="IDataReader"/>.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteReader(DbCommand)"/>
        /// <seealso cref="EndExecuteReader(IAsyncResult)"/>
        public override IAsyncResult BeginExecuteReader(DbCommand command, DbTransaction transaction, AsyncCallback callback, object state)
        {
            SqlCommand sqlCommand = CheckIfSqlCommand(command);

            PrepareCommand(sqlCommand, transaction);
            return DoBeginExecuteReader(sqlCommand, false, callback, state);
        }

        /// <summary>
        /// <para>Initiates the asynchronous execution of <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> which will return a <see cref="IDataReader"/>.</para>
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The name of the stored procedure to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both; 
        /// this value is also needed when invoking <see cref="EndExecuteReader"/>, 
        /// which returns the <see cref="IDataReader"/>.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteReader(string, object[])"/>
        /// <seealso cref="EndExecuteReader(IAsyncResult)"/>
        public override IAsyncResult BeginExecuteReader(string storedProcedureName, AsyncCallback callback, object state, params object[] parameterValues)
        {
            SqlCommand sqlCommand = CheckIfSqlCommand(GetStoredProcCommand(storedProcedureName, parameterValues));

            DbConnection connection = this.GetNewOpenConnection();
            try
            {
                PrepareCommand(sqlCommand, connection);
                return DoBeginExecuteReader(sqlCommand, true, callback, state);
            }
            catch
            {
                connection.Close();
                throw;
            }
        }

        /// <summary>
        /// <para>Initiates the asynchronous execution of <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> inside a transaction which will return a <see cref="IDataReader"/>.</para>
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="DbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="storedProcedureName">
        /// <para>The name of the stored procedure to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both; 
        /// this value is also needed when invoking <see cref="EndExecuteReader"/>, 
        /// which returns the <see cref="IDataReader"/>.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteReader(DbTransaction, string, object[])"/>
        /// <seealso cref="EndExecuteReader(IAsyncResult)"/>
        public override IAsyncResult BeginExecuteReader(DbTransaction transaction, string storedProcedureName, AsyncCallback callback, object state, params object[] parameterValues)
        {
            SqlCommand sqlCommand = CheckIfSqlCommand(GetStoredProcCommand(storedProcedureName, parameterValues));

            PrepareCommand(sqlCommand, transaction);
            return DoBeginExecuteReader(sqlCommand, true, callback, state);
        }

        /// <summary>
        /// <para>Initiates the asynchronous execution of the <paramref name="commandText"/> 
        /// interpreted as specified by the <paramref name="commandType" /> which will return 
        /// a <see cref="IDataReader"/>. When the async operation completes, the
        /// <paramref name="callback"/> will be invoked on another thread to process the
        /// result.</para>
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <param name="callback"><see cref="AsyncCallback"/> to execute when the async operation
        /// completes.</param>
        /// <param name="state">State object passed to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both; 
        /// this value is also needed when invoking <see cref="EndExecuteReader"/>, 
        /// which returns the <see cref="IDataReader"/>.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteReader(CommandType, string)"/>
        /// <seealso cref="EndExecuteReader(IAsyncResult)"/>
        public override IAsyncResult BeginExecuteReader(CommandType commandType, string commandText, AsyncCallback callback, object state)
        {
            SqlCommand sqlCommand = CreateSqlCommandByCommandType(commandType, commandText);

            DbConnection connection = this.GetNewOpenConnection();
            try
            {
                PrepareCommand(sqlCommand, connection);
                return DoBeginExecuteReader(sqlCommand, true, callback, state);
            }
            catch
            {
                connection.Close();
                throw;
            }
        }

        /// <summary>
        /// <para>Initiates the asynchronous execution of the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> inside an transaction which will return a <see cref="IDataReader"/>.</para>
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="DbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both; 
        /// this value is also needed when invoking <see cref="EndExecuteReader"/>, 
        /// which returns the <see cref="IDataReader"/>.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteReader(CommandType, string)"/>
        /// <seealso cref="EndExecuteReader(IAsyncResult)"/>
        public override IAsyncResult BeginExecuteReader(DbTransaction transaction, CommandType commandType, string commandText,
            AsyncCallback callback, object state)
        {
            SqlCommand sqlCommand = CreateSqlCommandByCommandType(commandType, commandText);

            PrepareCommand(sqlCommand, transaction);
            return DoBeginExecuteReader(sqlCommand, true, callback, state);
        }

        /// <summary>
        /// Finishes asynchronous execution of a Transact-SQL statement, returning an <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="asyncResult">
        /// <para>The <see cref="IAsyncResult"/> returned by a call to any overload of BeginExecuteReader.</para>
        /// </param>
        /// <seealso cref="Database.ExecuteReader(DbCommand)"/>
        /// <seealso cref="BeginExecuteReader(DbCommand,AsyncCallback,object)"/>
        /// <seealso cref="BeginExecuteReader(DbCommand, DbTransaction,AsyncCallback,object)"/>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object that can be used to consume the queried information.</para>
        /// </returns>     
        public override IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            DaabAsyncResult daabAsyncResult = (DaabAsyncResult)asyncResult;
            SqlCommand command = (SqlCommand)daabAsyncResult.Command;
            try
            {
                IDataReader reader = command.EndExecuteReader(daabAsyncResult.InnerAsyncResult);

                return reader;
            }
            catch (Exception)
            {
                if (command.Transaction == null)
                {
                    // for a reader, the standard cleanup will not close the connection, so it needs to be closed
                    // in the catch block if necessary
                    command.Connection.Close();
                }
                throw;
            }
            finally
            {
                CleanupConnectionFromAsyncOperation(daabAsyncResult);
            }
        }
    }
}
