// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data.Properties;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// Represents an abstract database that commands can be run against.
    /// </summary>
    /// <remarks>
    /// The <see cref="Database"/> class leverages the provider factory model from ADO.NET. A database instance holds
    /// a reference to a concrete <see cref="DbProviderFactory"/> object to which it forwards the creation of ADO.NET objects.
    /// </remarks>
    public abstract class Database
    {
        static readonly ParameterCache parameterCache = new ParameterCache();
        static readonly string VALID_PASSWORD_TOKENS = Resources.Password;
        static readonly string VALID_USER_ID_TOKENS = Resources.UserName;

        readonly ConnectionString connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class with a connection string and a <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the database.</param>
        /// <param name="dbProviderFactory">A <see cref="DbProviderFactory"/> object.</param>
        protected Database(string connectionString, DbProviderFactory dbProviderFactory)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(connectionString));
            if (dbProviderFactory == null) throw new ArgumentNullException(nameof(dbProviderFactory));

            this.connectionString = new ConnectionString(connectionString, VALID_USER_ID_TOKENS, VALID_PASSWORD_TOKENS);
            DbProviderFactory = dbProviderFactory;
        }

        /// <summary>
        /// Gets the string used to open a database.
        /// </summary>
        /// <value>
        /// <para>The string used to open a database.</para>
        /// </value>
        /// <seealso cref="DbConnection.ConnectionString"/>
        public string ConnectionString
        {
            get { return connectionString.ToString(); }
        }

        /// <summary>
        /// Gets the connection string without the username and password.
        /// </summary>
        /// <value>
        /// <para>The connection string without the username and password.</para>
        /// </value>
        /// <seealso cref="ConnectionString"/>
        protected string ConnectionStringNoCredentials
        {
            get { return connectionString.ToStringNoCredentials(); }
        }

        /// <summary>
        /// Gets the connection string without credentials.
        /// </summary>
        /// <value>
        /// The connection string without credentials.
        /// </value>
        public string ConnectionStringWithoutCredentials
        {
            get { return ConnectionStringNoCredentials; }
        }

        /// <summary>
        /// Gets the DbProviderFactory used by the database instance.
        /// </summary>
        /// <seealso cref="DbProviderFactory"/>
        public DbProviderFactory DbProviderFactory { get; }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the in parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <remarks>
        /// <para>This version of the method is used when you can have the same parameter object multiple times with different values.</para>
        /// </remarks>
        public void AddInParameter(DbCommand command,
                                   string name,
                                   DbType dbType)
        {
            AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, null);
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>
        public void AddInParameter(DbCommand command,
                                   string name,
                                   DbType dbType,
                                   object value)
        {
            AddParameter(command, name, dbType, ParameterDirection.Input, String.Empty, DataRowVersion.Default, value);
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the value.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        public void AddInParameter(DbCommand command,
                                   string name,
                                   DbType dbType,
                                   string sourceColumn,
                                   DataRowVersion sourceVersion)
        {
            AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null);
        }

        /// <summary>
        /// Adds a new Out <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the out parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        public void AddOutParameter(DbCommand command,
                                    string name,
                                    DbType dbType,
                                    int size)
        {
            AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
        }

        /// <summary>
        /// Adds new In/Out <see cref="DbParameter"/> object to given <paramref name="command"/>
        /// </summary>
        /// <param name="command">The command to add the in/out parameter.</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
        /// <param name="value">The value of the parameter for In direction.</param>
        public void AddInOutParameter(DbCommand command, string name, DbType dbType, object value)
        {
            AddParameter(command, name, dbType, 0, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
        }

        /// <summary>
        /// Adds new In/Out <see cref="DbParameter"/> object to given <paramref name="command"/>
        /// </summary>
        /// <param name="command">The command to add the in/out parameter.</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="dbType">One of the <see cref="DbType"/> values.</param>
        /// <param name="size">The maximum size of the data within the column.</param>
        /// <param name="value">The value of the parameter for In direction.</param>
        public void AddInOutParameter(DbCommand command, string name, DbType dbType, int size, object value)
        {
            AddParameter(command, name, dbType, size, ParameterDirection.InputOutput, true, 0, 0, String.Empty, DataRowVersion.Default, value);
        }

        /// <summary>
        /// Adds a new In <see cref="DbParameter"/> object to the given <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="nullable"><para>A value indicating whether the parameter accepts <b>null</b> (<b>Nothing</b> in Visual Basic) values.</para></param>
        /// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
        /// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <b>null</b>.</exception>
        public virtual void AddParameter(DbCommand command,
                                         string name,
                                         DbType dbType,
                                         int size,
                                         ParameterDirection direction,
                                         bool nullable,
                                         byte precision,
                                         byte scale,
                                         string sourceColumn,
                                         DataRowVersion sourceVersion,
                                         object value)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            DbParameter parameter = CreateParameter(name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Adds a new instance of a <see cref="DbParameter"/> object to the command.
        /// </summary>
        /// <param name="command">The command to add the parameter.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <b>null</b>.</exception>
        public void AddParameter(DbCommand command,
                                 string name,
                                 DbType dbType,
                                 ParameterDirection direction,
                                 string sourceColumn,
                                 DataRowVersion sourceVersion,
                                 object value)
        {
            AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);
        }

        private void AssignParameterValues(DbCommand command, object[] values)
        {
            int parameterIndexShift = UserParametersStartIndex(); // DONE magic number, depends on the database
            for (int i = 0; i < values.Length; i++)
            {
                IDataParameter parameter = command.Parameters[i + parameterIndexShift];

                // There used to be code here that checked to see if the parameter was input or input/output
                // before assigning the value to it. We took it out because of an operational bug with
                // deriving parameters for a stored procedure. It turns out that output parameters are set
                // to input/output after discovery, so any direction checking was unneeded. Should it ever
                // be needed, it should go here, and check that a parameter is input or input/output before
                // assigning a value to it.
                SetParameterValue(command, parameter.ParameterName, values[i]);
            }
        }

        static DbTransaction BeginTransaction(DbConnection connection)
        {
            DbTransaction tran = connection.BeginTransaction();
            return tran;
        }

        /// <summary>
        /// Builds a value parameter name for the current database.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>A correctly formated parameter name.</returns>
        public virtual string BuildParameterName(string name)
        {
            return name;
        }

        /// <summary>
        /// Clears the parameter cache. Since there is only one parameter cache that is shared by all instances
        /// of this class, this clears all parameters cached for all databases.
        /// </summary>
        public static void ClearParameterCache()
        {
            parameterCache.Clear();
        }

        static void CommitTransaction(IDbTransaction tran)
        {
            tran.Commit();
        }

        /// <summary>
        /// Configures a given <see cref="DbParameter"/>.
        /// </summary>
        /// <param name="param">The <see cref="DbParameter"/> to configure.</param>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="nullable"><para>A value indicating whether the parameter accepts <b>null</b> (<b>Nothing</b> in Visual Basic) values.</para></param>
        /// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
        /// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>
        protected virtual void ConfigureParameter(DbParameter param,
                                                  string name,
                                                  DbType dbType,
                                                  int size,
                                                  ParameterDirection direction,
                                                  bool nullable,
                                                  byte precision,
                                                  byte scale,
                                                  string sourceColumn,
                                                  DataRowVersion sourceVersion,
                                                  object value)
        {
            param.DbType = dbType;
            param.Size = size;
            param.Value = value ?? DBNull.Value;
            param.Direction = direction;
            param.IsNullable = nullable;
            param.SourceColumn = sourceColumn;
            param.SourceVersion = sourceVersion;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "The purpose of the block is to execute arbitrary SQL on behalf of the user. It is known that the users must review the use of the Database for security vulnerabilities.")]
        DbCommand CreateCommandByCommandType(CommandType commandType,
                                             string commandText)
        {
            DbCommand command = DbProviderFactory.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;

            return command;
        }

        /// <summary>
        /// Creates a connection for this database.
        /// </summary>
        /// <returns>
        /// <para>The <see cref="DbConnection"/> for this database.</para>
        /// </returns>
        /// <seealso cref="DbConnection"/>
        public virtual DbConnection CreateConnection()
        {
            DbConnection newConnection = DbProviderFactory.CreateConnection();
            newConnection.ConnectionString = ConnectionString;

            return newConnection;
        }

        /// <summary>
        /// Adds a new instance of a <see cref="DbParameter"/> object.
        /// </summary>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <param name="dbType"><para>One of the <see cref="DbType"/> values.</para></param>
        /// <param name="size"><para>The maximum size of the data within the column.</para></param>
        /// <param name="direction"><para>One of the <see cref="ParameterDirection"/> values.</para></param>
        /// <param name="nullable"><para>A value indicating whether the parameter accepts <b>null</b> (<b>Nothing</b> in Visual Basic) values.</para></param>
        /// <param name="precision"><para>The maximum number of digits used to represent the <paramref name="value"/>.</para></param>
        /// <param name="scale"><para>The number of decimal places to which <paramref name="value"/> is resolved.</para></param>
        /// <param name="sourceColumn"><para>The name of the source column mapped to the DataSet and used for loading or returning the <paramref name="value"/>.</para></param>
        /// <param name="sourceVersion"><para>One of the <see cref="DataRowVersion"/> values.</para></param>
        /// <param name="value"><para>The value of the parameter.</para></param>
        /// <returns>A newly created <see cref="DbParameter"/> fully initialized with given parameters.</returns>
        protected DbParameter CreateParameter(string name,
                                              DbType dbType,
                                              int size,
                                              ParameterDirection direction,
                                              bool nullable,
                                              byte precision,
                                              byte scale,
                                              string sourceColumn,
                                              DataRowVersion sourceVersion,
                                              object value)
        {
            DbParameter param = CreateParameter(name);
            ConfigureParameter(param, name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
            return param;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DbParameter"/> object.
        /// </summary>
        /// <param name="name"><para>The name of the parameter.</para></param>
        /// <returns><para>An unconfigured parameter.</para></returns>
        protected DbParameter CreateParameter(string name)
        {
            DbParameter param = DbProviderFactory.CreateParameter();
            param.ParameterName = BuildParameterName(name);

            return param;
        }



        /// <summary>
        /// Does this <see cref='Database'/> object support parameter discovery?
        /// </summary>
        /// <value>Base class always returns false.</value>
        public virtual bool SupportsParemeterDiscovery => false;

        /// <summary>
        /// Retrieves parameter information from the stored procedure specified in the <see cref="DbCommand"/> and populates the
        /// Parameters collection of the specified <see cref="DbCommand"/> object.
        /// </summary>
        /// <param name="discoveryCommand">The <see cref="DbCommand"/> to do the discovery.</param>
        protected abstract void DeriveParameters(DbCommand discoveryCommand);

        /// <summary>
        /// Discovers the parameters for a <see cref="DbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to discover the parameters.</param>
        /// <remarks>You should get the value of <see cref="SupportsParemeterDiscovery"/> to detect
        /// if parameter discovery is supported for your database provider type.</remarks>
        public void DiscoverParameters(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using (var wrapper = GetOpenConnection())
            {
                using (DbCommand discoveryCommand = CreateCommandByCommandType(command.CommandType, command.CommandText))
                {
                    discoveryCommand.Connection = wrapper.Connection;
                    DeriveParameters(discoveryCommand);

                    foreach (IDataParameter parameter in discoveryCommand.Parameters)
                    {
                        IDataParameter cloneParameter = (IDataParameter)((ICloneable)parameter).Clone();
                        command.Parameters.Add(cloneParameter);
                    }
                }
            }
        }

        /// <summary>
        /// Executes the query for <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> representing the query to execute.</param>
        /// <returns>The quantity of rows affected.</returns>
        protected int DoExecuteNonQuery(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected;
        }

        IDataReader DoExecuteReader(DbCommand command, CommandBehavior cmdBehavior)
        {
            IDataReader reader = command.ExecuteReader(cmdBehavior);

            return reader;
        }

        object DoExecuteScalar(IDbCommand command)
        {
            object returnValue = command.ExecuteScalar();
            return returnValue;
        }

        void DoLoadDataSet(IDbCommand command,
                           DataSet dataSet,
                           string[] tableNames)
        {
            if (tableNames == null) throw new ArgumentNullException(nameof(tableNames));
            if (tableNames.Length == 0)
            {
                throw new ArgumentException(Resources.ExceptionTableNameArrayEmpty, nameof(tableNames));
            }
            for (int i = 0; i < tableNames.Length; i++)
            {
                if (string.IsNullOrEmpty(tableNames[i])) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, string.Concat("tableNames[", i, "]"));
            }

            using (DbDataAdapter adapter = GetDataAdapter(UpdateBehavior.Standard))
            {
                ((IDbDataAdapter)adapter).SelectCommand = command;

                string systemCreatedTableNameRoot = "Table";
                for (int i = 0; i < tableNames.Length; i++)
                {
                    string systemCreatedTableName = (i == 0)
                                                        ? systemCreatedTableNameRoot
                                                        : systemCreatedTableNameRoot + i;

                    adapter.TableMappings.Add(systemCreatedTableName, tableNames[i]);
                }

                adapter.Fill(dataSet);
            }
        }

        int DoUpdateDataSet(UpdateBehavior behavior,
                            DataSet dataSet,
                            string tableName,
                            IDbCommand insertCommand,
                            IDbCommand updateCommand,
                            IDbCommand deleteCommand,
                            int? updateBatchSize)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(tableName));
            if (dataSet == null) throw new ArgumentNullException(nameof(dataSet));

            if (insertCommand == null && updateCommand == null && deleteCommand == null)
            {
                throw new ArgumentException(Resources.ExceptionMessageUpdateDataSetArgumentFailure);
            }

            using (DbDataAdapter adapter = GetDataAdapter(behavior))
            {
                IDbDataAdapter explicitAdapter = adapter;
                if (insertCommand != null)
                {
                    explicitAdapter.InsertCommand = insertCommand;
                }
                if (updateCommand != null)
                {
                    explicitAdapter.UpdateCommand = updateCommand;
                }
                if (deleteCommand != null)
                {
                    explicitAdapter.DeleteCommand = deleteCommand;
                }

                if (updateBatchSize != null)
                {
                    adapter.UpdateBatchSize = (int)updateBatchSize;
                    if (insertCommand != null)
                        adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                    if (updateCommand != null)
                        adapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                    if (deleteCommand != null)
                        adapter.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                }

                int rows = adapter.Update(dataSet.Tables[tableName]);
                return rows;
            }
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns the results in a new <see cref="DataSet"/>.
        /// </summary>
        /// <param name="command"><para>The <see cref="DbCommand"/> to execute.</para></param>
        /// <returns>A <see cref="DataSet"/> with the results of the <paramref name="command"/>.</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command)
        {
            DataSet dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            LoadDataSet(command, dataSet, "Table");
            return dataSet;
        }

        /// <summary>
        /// Executes the <paramref name="command"/> as part of the <paramref name="transaction" /> and returns the results in a new <see cref="DataSet"/>.
        /// </summary>
        /// <param name="command"><para>The <see cref="DbCommand"/> to execute.</para></param>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <returns>A <see cref="DataSet"/> with the results of the <paramref name="command"/>.</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command,
                                              DbTransaction transaction)
        {
            var dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            LoadDataSet(command, dataSet, "Table", transaction);
            return dataSet;
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> with <paramref name="parameterValues" /> and returns the results in a new <see cref="DataSet"/>.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>A <see cref="DataSet"/> with the results of the <paramref name="storedProcedureName"/>.</para>
        /// </returns>
        public virtual DataSet ExecuteDataSet(string storedProcedureName,
                                              params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteDataSet(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> with <paramref name="parameterValues" /> as part of the <paramref name="transaction" /> and returns the results in a new <see cref="DataSet"/> within a transaction.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="storedProcedureName">
        /// <para>The stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>A <see cref="DataSet"/> with the results of the <paramref name="storedProcedureName"/>.</para>
        /// </returns>
        public virtual DataSet ExecuteDataSet(DbTransaction transaction,
                                              string storedProcedureName,
                                              params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteDataSet(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> and returns the results in a new <see cref="DataSet"/>.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>A <see cref="DataSet"/> with the results of the <paramref name="commandText"/>.</para>
        /// </returns>
        public virtual DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteDataSet(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> as part of the given <paramref name="transaction" /> and returns the results in a new <see cref="DataSet"/>.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>A <see cref="DataSet"/> with the results of the <paramref name="commandText"/>.</para>
        /// </returns>
        public virtual DataSet ExecuteDataSet(DbTransaction transaction,
                                              CommandType commandType,
                                              string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteDataSet(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns the number of rows affected.
        /// </summary>
        /// <param name="command">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual int ExecuteNonQuery(DbCommand command)
        {
            using (var wrapper = GetOpenConnection())
            {
                PrepareCommand(command, wrapper.Connection);
                return DoExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="command"/> within the given <paramref name="transaction" />, and returns the number of rows affected.
        /// </summary>
        /// <param name="command">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
        {
            PrepareCommand(command, transaction);
            return DoExecuteNonQuery(command);
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> and returns the number of rows affected.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The name of the stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>The number of rows affected</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual int ExecuteNonQuery(string storedProcedureName, params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> within a transaction and returns the number of rows affected.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="storedProcedureName">
        /// <para>The name of the stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>The number of rows affected.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual int ExecuteNonQuery(DbTransaction transaction,
                                           string storedProcedureName,
                                           params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteNonQuery(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>The number of rows affected.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> as part of the given <paramref name="transaction" /> and returns the number of rows affected.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>The number of rows affected</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual int ExecuteNonQuery(DbTransaction transaction,
                                           CommandType commandType,
                                           string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteNonQuery(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns an <see cref="IDataReader"></see> through which the result can be read.
        /// It is the responsibility of the caller to close the reader when finished.
        /// </summary>
        /// <param name="command">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object.</para>
        /// </returns>
        public virtual IDataReader ExecuteReader(DbCommand command)
        {
            using (DatabaseConnectionWrapper wrapper = GetOpenConnection())
            {
                PrepareCommand(command, wrapper.Connection);
                IDataReader realReader = DoExecuteReader(command, CommandBehavior.Default);
                return CreateWrappedReader(wrapper, realReader);
            }
        }

        /// <summary>
        /// All data readers get wrapped in objects so that they properly manage connections.
        /// Some derived Database classes will need to create a different wrapper, so this
        /// method is provided so that they can do this.
        /// </summary>
        /// <param name="connection">Connection + refcount.</param>
        /// <param name="innerReader">The reader to wrap.</param>
        /// <returns>The new reader.</returns>
        protected virtual IDataReader CreateWrappedReader(DatabaseConnectionWrapper connection, IDataReader innerReader)
        {
            return new RefCountingDataReader(connection, innerReader);
        }

        /// <summary>
        /// Executes the <paramref name="command"/> within a transaction and returns an <see cref="IDataReader"></see> through which the result can be read.
        /// It is the responsibility of the caller to close the connection and reader when finished.
        /// </summary>
        /// <param name="command">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object.</para>
        /// </returns>
        public virtual IDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
        {
            PrepareCommand(command, transaction);
            return DoExecuteReader(command, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
        /// It is the responsibility of the caller to close the connection and reader when finished.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object.</para>
        /// </returns>
        public IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteReader(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> within the given <paramref name="transaction" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
        /// It is the responsibility of the caller to close the connection and reader when finished.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="storedProcedureName">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object.</para>
        /// </returns>
        public IDataReader ExecuteReader(DbTransaction transaction,
                                         string storedProcedureName,
                                         params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteReader(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> and returns an <see cref="IDataReader"></see> through which the result can be read.
        /// It is the responsibility of the caller to close the connection and reader when finished.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object.</para>
        /// </returns>
        public IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteReader(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by
        /// <paramref name="commandType" /> within the given <paramref name="transaction" /> and returns an
        /// <see cref="IDataReader"></see> through which the result can be read.
        /// It is the responsibility of the caller to close the connection and reader when finished.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IDataReader"/> object.</para>
        /// </returns>
        public IDataReader ExecuteReader(DbTransaction transaction,
                                         CommandType commandType,
                                         string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteReader(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="command">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <returns>
        /// <para>The first column of the first row in the result set.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using (var wrapper = GetOpenConnection())
            {
                PrepareCommand(command, wrapper.Connection);
                return DoExecuteScalar(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="command"/> within a <paramref name="transaction" />, and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="command">
        /// <para>The command that contains the query to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <returns>
        /// <para>The first column of the first row in the result set.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(DbCommand command, DbTransaction transaction)
        {
            PrepareCommand(command, transaction);
            return DoExecuteScalar(command);
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>The first column of the first row in the result set.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(string storedProcedureName, params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteScalar(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="storedProcedureName"/> with the given <paramref name="parameterValues" /> within a
        /// <paramref name="transaction" /> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="storedProcedureName">
        /// <para>The stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>The first column of the first row in the result set.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(DbTransaction transaction,
                                            string storedProcedureName,
                                            params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteScalar(command, transaction);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" />  and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>The first column of the first row in the result set.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(CommandType commandType, string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteScalar(command);
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" />
        /// within the given <paramref name="transaction" /> and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <returns>
        /// <para>The first column of the first row in the result set.</para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(DbTransaction transaction,
                                            CommandType commandType,
                                            string commandText)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                return ExecuteScalar(command, transaction);
            }
        }

        /// <summary>
        /// Gets a DbDataAdapter with Standard update behavior.
        /// </summary>
        /// <remarks>
        /// Created this new, public method instead of modifying the protected, abstract one so that there will be no
        /// breaking changes for any currently derived Database class.
        /// </remarks>
        /// <returns>A <see cref="DbDataAdapter"/>.</returns>
        /// <seealso cref="DbDataAdapter"/>
        public DbDataAdapter GetDataAdapter()
        {
            return GetDataAdapter(UpdateBehavior.Standard);
        }

        /// <summary>
        /// Gets the DbDataAdapter with the given update behavior and connection from the proper derived class.
        /// </summary>
        /// <param name="updateBehavior">
        /// <para>One of the <see cref="UpdateBehavior"/> values.</para>
        /// </param>
        /// <returns>A <see cref="DbDataAdapter"/>.</returns>
        /// <seealso cref="DbDataAdapter"/>
        protected DbDataAdapter GetDataAdapter(UpdateBehavior updateBehavior)
        {
            DbDataAdapter adapter = DbProviderFactory.CreateDataAdapter();

            if (updateBehavior == UpdateBehavior.Continue)
            {
                SetUpRowUpdatedEvent(adapter);
            }
            return adapter;
        }

        /// <summary>
        /// Gets a new <see cref="DbConnection"/> in an open state.
        /// </summary>
        /// <returns>An open <see cref="DbConnection"/>.</returns>
        protected internal DbConnection GetNewOpenConnection()
        {
            DbConnection connection = null;
            try
            {
                    connection = CreateConnection();
                    connection.Open();
            }
            catch
            {
                if (connection != null)
                    connection.Close();

                throw;
            }

            return connection;
        }

        /// <summary>
        /// Gets a "wrapped" connection that will be not be disposed if a transaction is
        /// active (created by creating a <see cref="TransactionScope"/> instance). The
        /// connection will be disposed when no transaction is active.
        /// </summary>
        /// <returns></returns>
        protected DatabaseConnectionWrapper GetOpenConnection()
        {
            DatabaseConnectionWrapper connection = TransactionScopeConnections.GetConnection(this);
            return connection ?? GetWrappedConnection();
        }

        /// <summary>
        /// Gets a "wrapped" connection for use outside a transaction.
        /// </summary>
        /// <returns>The wrapped connection.</returns>
        protected virtual DatabaseConnectionWrapper GetWrappedConnection()
        {
            return new DatabaseConnectionWrapper(GetNewOpenConnection());
        }

        /// <summary>
        /// Gets a parameter value.
        /// </summary>
        /// <param name="command">The command that contains the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The value of the parameter.</returns>
        public virtual object GetParameterValue(DbCommand command, string name)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return command.Parameters[BuildParameterName(name)].Value;
        }

        /// <summary>
        /// Creates a <see cref="DbCommand"/> for a SQL query.
        /// </summary>
        /// <param name="query"><para>The text of the query.</para></param>
        /// <returns><para>The <see cref="DbCommand"/> for the SQL query.</para></returns>
        /// <exception cref="ArgumentException"><paramref name="query"/> is <b>null</b> or empty.</exception>
        public DbCommand GetSqlStringCommand(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(query));

            return CreateCommandByCommandType(CommandType.Text, query);
        }

        /// <summary>
        /// Creates a <see cref="DbCommand"/> for a stored procedure.
        /// </summary>
        /// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
        /// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>
        /// <exception cref="ArgumentException"><paramref name="storedProcedureName"/>is <b>null</b> or empty.</exception>
        public virtual DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(storedProcedureName));

            return CreateCommandByCommandType(CommandType.StoredProcedure, storedProcedureName);
        }

        /// <summary>
        /// Creates a <see cref="DbCommand"/> for a stored procedure.
        /// </summary>
        /// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
        /// <param name="parameterValues"><para>The list of parameters for the procedure.</para></param>
        /// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>
        /// <remarks>
        /// <para>The parameters for the stored procedure will be discovered and the values are assigned in
        /// positional order.</para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="storedProcedureName"/>is <b>null</b> or empty.</exception>
        public virtual DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(storedProcedureName));

            DbCommand command = CreateCommandByCommandType(CommandType.StoredProcedure, storedProcedureName);

            AssignParameters(command, parameterValues);

            return command;
        }

        /// <summary>
        /// Discovers parameters on the <paramref name="command"/> and assigns the values from <paramref name="parameterValues"/> to the <paramref name="command"/>s Parameters list.
        /// </summary>
        /// <param name="command">The command the parameter values will be assigned to</param>
        /// <param name="parameterValues">The parameter values that will be assigned to the command.</param>
        /// <exception cref="InvalidOperationException">The number of parameters in <paramref name="parameterValues"/>
        /// does not match the number of parameters in <paramref name="command"/>.</exception>
        public virtual void AssignParameters(DbCommand command, object[] parameterValues)
        {
            parameterCache.SetParameters(command, this);

            if (SameNumberOfParametersAndValues(command, parameterValues) == false)
            {
                throw new InvalidOperationException(Resources.ExceptionMessageParameterMatchFailure);
            }

            AssignParameterValues(command, parameterValues);
        }

        /// <summary>
        /// Wraps around a derived class's implementation of the GetStoredProcCommandWrapper method and adds functionality for
        /// using this method with UpdateDataSet. The GetStoredProcCommandWrapper method (above) that takes a params array
        /// expects the array to be filled with VALUES for the parameters. This method differs from the GetStoredProcCommandWrapper
        /// method in that it allows a user to pass in a string array. It will also dynamically discover the parameters for the
        /// stored procedure and set the parameter's SourceColumns to the strings that are passed in. It does this by mapping
        /// the parameters to the strings IN ORDER. Thus, order is very important.
        /// </summary>
        /// <param name="storedProcedureName"><para>The name of the stored procedure.</para></param>
        /// <param name="sourceColumns"><para>The list of DataFields for the procedure.</para></param>
        /// <returns><para>The <see cref="DbCommand"/> for the stored procedure.</para></returns>
        public DbCommand GetStoredProcCommandWithSourceColumns(string storedProcedureName,
                                                               params string[] sourceColumns)
        {
            if (string.IsNullOrEmpty(storedProcedureName)) throw new ArgumentException(Resources.ExceptionNullOrEmptyString, nameof(storedProcedureName));
            if (sourceColumns == null) throw new ArgumentNullException(nameof(sourceColumns));

            DbCommand dbCommand = GetStoredProcCommand(storedProcedureName);

            //we do not actually set the connection until the Fill or Update, so we need to temporarily do it here so we can set the sourcecolumns
            using (DbConnection connection = CreateConnection())
            {
                dbCommand.Connection = connection;
                DiscoverParameters(dbCommand);
            }

            int iSourceIndex = 0;
            foreach (IDataParameter dbParam in dbCommand.Parameters)
            {
                //BUG: should probably be ||. needs testing.
                if ((dbParam.Direction == ParameterDirection.Input) | (dbParam.Direction == ParameterDirection.InputOutput))
                {
                    dbParam.SourceColumn = sourceColumns[iSourceIndex];
                    iSourceIndex++;
                }
            }

            return dbCommand;
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and adds a new <see cref="DataTable"></see> to the existing <see cref="DataSet"></see>.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to load.</para>
        /// </param>
        /// <param name="tableName">
        /// <para>The name for the new <see cref="DataTable"/> to add to the <see cref="DataSet"/>.</para>
        /// </param>
        /// <exception cref="System.ArgumentNullException">Any input parameter was <b>null</b> (<b>Nothing</b> in Visual Basic)</exception>
        /// <exception cref="System.ArgumentException">tableName was an empty string</exception>
        public virtual void LoadDataSet(DbCommand command,
                                        DataSet dataSet,
                                        string tableName)
        {
            LoadDataSet(command, dataSet, new[] { tableName });
        }

        /// <summary>
        /// Executes the <paramref name="command"/> within the given <paramref name="transaction" /> and adds a new <see cref="DataTable"></see> to the existing <see cref="DataSet"></see>.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to load.</para>
        /// </param>
        /// <param name="tableName">
        /// <para>The name for the new <see cref="DataTable"/> to add to the <see cref="DataSet"/>.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <exception cref="System.ArgumentNullException">Any input parameter was <b>null</b> (<b>Nothing</b> in Visual Basic).</exception>
        /// <exception cref="System.ArgumentException">tableName was an empty string.</exception>
        public virtual void LoadDataSet(DbCommand command,
                                        DataSet dataSet,
                                        string tableName,
                                        DbTransaction transaction)
        {
            LoadDataSet(command, dataSet, new[] { tableName }, transaction);
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> from a <see cref="DbCommand"/>.
        /// </summary>
        /// <param name="command">
        /// <para>The command to execute to fill the <see cref="DataSet"/>.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to fill.</para>
        /// </param>
        /// <param name="tableNames">
        /// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
        /// </param>
        public virtual void LoadDataSet(DbCommand command,
                                        DataSet dataSet,
                                        string[] tableNames)
        {
            using (var wrapper = GetOpenConnection())
            {
                PrepareCommand(command, wrapper.Connection);
                DoLoadDataSet(command, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> from a <see cref="DbCommand"/> in  a transaction.
        /// </summary>
        /// <param name="command">
        /// <para>The command to execute to fill the <see cref="DataSet"/>.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to fill.</para>
        /// </param>
        /// <param name="tableNames">
        /// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command in.</para>
        /// </param>
        public virtual void LoadDataSet(DbCommand command,
                                        DataSet dataSet,
                                        string[] tableNames,
                                        DbTransaction transaction)
        {
            PrepareCommand(command, transaction);
            DoLoadDataSet(command, dataSet, tableNames);
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> with the results returned from a stored procedure.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The stored procedure name to execute.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to fill.</para>
        /// </param>
        /// <param name="tableNames">
        /// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        public virtual void LoadDataSet(string storedProcedureName,
                                        DataSet dataSet,
                                        string[] tableNames,
                                        params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                LoadDataSet(command, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> with the results returned from a stored procedure executed in a transaction.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the stored procedure in.</para>
        /// </param>
        /// <param name="storedProcedureName">
        /// <para>The stored procedure name to execute.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to fill.</para>
        /// </param>
        /// <param name="tableNames">
        /// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        public virtual void LoadDataSet(DbTransaction transaction,
                                        string storedProcedureName,
                                        DataSet dataSet,
                                        string[] tableNames,
                                        params object[] parameterValues)
        {
            using (DbCommand command = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                LoadDataSet(command, dataSet, tableNames, transaction);
            }
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> from command text.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to fill.</para>
        /// </param>
        /// <param name="tableNames">
        /// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
        /// </param>
        public virtual void LoadDataSet(CommandType commandType,
                                        string commandText,
                                        DataSet dataSet,
                                        string[] tableNames)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                LoadDataSet(command, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> from command text in a transaction.
        /// </summary>
        /// <param name="transaction">
        /// <para>The <see cref="IDbTransaction"/> to execute the command in.</para>
        /// </param>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <param name="dataSet">
        /// <para>The <see cref="DataSet"/> to fill.</para>
        /// </param>
        /// <param name="tableNames">
        /// <para>An array of table name mappings for the <see cref="DataSet"/>.</para>
        /// </param>
        public void LoadDataSet(DbTransaction transaction,
                                CommandType commandType,
                                string commandText,
                                DataSet dataSet,
                                string[] tableNames)
        {
            using (DbCommand command = CreateCommandByCommandType(commandType, commandText))
            {
                LoadDataSet(command, dataSet, tableNames, transaction);
            }
        }

        /// <summary>
        /// Assigns a <paramref name="connection"/> to the <paramref name="command"/> and discovers parameters if needed.
        /// </summary>
        /// <param name="command"><para>The command that contains the query to prepare.</para></param>
        /// <param name="connection">The connection to assign to the command.</param>
        protected static void PrepareCommand(DbCommand command,
                                             DbConnection connection)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            command.Connection = connection;
        }

        /// <summary>
        /// Assigns a <paramref name="transaction"/> to the <paramref name="command"/> and discovers parameters if needed.
        /// </summary>
        /// <param name="command"><para>The command that contains the query to prepare.</para></param>
        /// <param name="transaction">The transaction to assign to the command.</param>
        protected static void PrepareCommand(DbCommand command,
                                             DbTransaction transaction)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            PrepareCommand(command, transaction.Connection);
            command.Transaction = transaction;
        }

        static void RollbackTransaction(IDbTransaction tran)
        {
            tran.Rollback();
        }

        /// <summary>
        /// Determines if the number of parameters in the command matches the array of parameter values.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> containing the parameters.</param>
        /// <param name="values">The array of parameter values.</param>
        /// <returns><see langword="true"/> if the number of parameters and values match; otherwise, <see langword="false"/>.</returns>
        protected virtual bool SameNumberOfParametersAndValues(DbCommand command, object[] values)
        {
            int numberOfParametersToStoredProcedure = command.Parameters.Count;
            int numberOfValuesProvidedForStoredProcedure = values.Length;
            return numberOfParametersToStoredProcedure == numberOfValuesProvidedForStoredProcedure;
        }

        /// <summary>
        /// Sets a parameter value.
        /// </summary>
        /// <param name="command">The command with the parameter.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public virtual void SetParameterValue(DbCommand command,
                                              string parameterName,
                                              object value)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            command.Parameters[BuildParameterName(parameterName)].Value = value ?? DBNull.Value;
        }

        /// <summary>
        /// Sets the RowUpdated event for the data adapter.
        /// </summary>
        /// <param name="adapter">The <see cref="DbDataAdapter"/> to set the event.</param>
        protected virtual void SetUpRowUpdatedEvent(DbDataAdapter adapter) { }

        /// <summary>
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="dataSet"><para>The <see cref="DataSet"/> used to update the data source.</para></param>
        /// <param name="tableName"><para>The name of the source table to use for table mapping.</para></param>
        /// <param name="insertCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Added"/></para></param>
        /// <param name="updateCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Modified"/></para></param>
        /// <param name="deleteCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Deleted"/></para></param>
        /// <param name="updateBehavior"><para>One of the <see cref="UpdateBehavior"/> values.</para></param>
        /// <param name="updateBatchSize">The number of database commands to execute in a batch.</param>
        /// <returns>number of records affected</returns>
        public int UpdateDataSet(DataSet dataSet,
                                 string tableName,
                                 DbCommand insertCommand,
                                 DbCommand updateCommand,
                                 DbCommand deleteCommand,
                                 UpdateBehavior updateBehavior,
                                 int? updateBatchSize)
        {
            using (var wrapper = GetOpenConnection())
            {
                if (updateBehavior == UpdateBehavior.Transactional && Transaction.Current == null)
                {
                    DbTransaction transaction = BeginTransaction(wrapper.Connection);
                    try
                    {
                        int rowsAffected = UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, transaction, updateBatchSize);
                        CommitTransaction(transaction);
                        return rowsAffected;
                    }
                    catch
                    {
                        RollbackTransaction(transaction);
                        throw;
                    }
                }

                if (insertCommand != null)
                {
                    PrepareCommand(insertCommand, wrapper.Connection);
                }
                if (updateCommand != null)
                {
                    PrepareCommand(updateCommand, wrapper.Connection);
                }
                if (deleteCommand != null)
                {
                    PrepareCommand(deleteCommand, wrapper.Connection);
                }

                return DoUpdateDataSet(updateBehavior, dataSet, tableName,
                                       insertCommand, updateCommand, deleteCommand, updateBatchSize);
            }
        }

        /// <summary>
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the <see cref="DataSet"/>.
        /// </summary>
        /// <param name="dataSet"><para>The <see cref="DataSet"/> used to update the data source.</para></param>
        /// <param name="tableName"><para>The name of the source table to use for table mapping.</para></param>
        /// <param name="insertCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Added"/></para></param>
        /// <param name="updateCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Modified"/></para></param>
        /// <param name="deleteCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Deleted"/></para></param>
        /// <param name="updateBehavior"><para>One of the <see cref="UpdateBehavior"/> values.</para></param>
        /// <returns>number of records affected</returns>
        public int UpdateDataSet(DataSet dataSet,
                                 string tableName,
                                 DbCommand insertCommand,
                                 DbCommand updateCommand,
                                 DbCommand deleteCommand,
                                 UpdateBehavior updateBehavior)
        {
            return UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBehavior, null);
        }

        /// <summary>
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the <see cref="DataSet"/> within a transaction.
        /// </summary>
        /// <param name="dataSet"><para>The <see cref="DataSet"/> used to update the data source.</para></param>
        /// <param name="tableName"><para>The name of the source table to use for table mapping.</para></param>
        /// <param name="insertCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Added"/>.</para></param>
        /// <param name="updateCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Modified"/>.</para></param>
        /// <param name="deleteCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Deleted"/>.</para></param>
        /// <param name="transaction"><para>The <see cref="IDbTransaction"/> to use.</para></param>
        /// <param name="updateBatchSize">The number of commands that can be executed in a single call to the database. Set to 0 to
        /// use the largest size the server can handle, 1 to disable batch updates, and anything else to set the number of rows.
        /// </param>
        /// <returns>Number of records affected.</returns>
        public int UpdateDataSet(DataSet dataSet,
                                 string tableName,
                                 DbCommand insertCommand,
                                 DbCommand updateCommand,
                                 DbCommand deleteCommand,
                                 DbTransaction transaction,
                                 int? updateBatchSize)
        {
            if (insertCommand != null)
            {
                PrepareCommand(insertCommand, transaction);
            }
            if (updateCommand != null)
            {
                PrepareCommand(updateCommand, transaction);
            }
            if (deleteCommand != null)
            {
                PrepareCommand(deleteCommand, transaction);
            }

            return DoUpdateDataSet(UpdateBehavior.Transactional,
                                   dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBatchSize);
        }

        /// <summary>
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the <see cref="DataSet"/> within a transaction.
        /// </summary>
        /// <param name="dataSet"><para>The <see cref="DataSet"/> used to update the data source.</para></param>
        /// <param name="tableName"><para>The name of the source table to use for table mapping.</para></param>
        /// <param name="insertCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Added"/>.</para></param>
        /// <param name="updateCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Modified"/>.</para></param>
        /// <param name="deleteCommand"><para>The <see cref="DbCommand"/> executed when <see cref="DataRowState"/> is <see cref="DataRowState.Deleted"/>.</para></param>
        /// <param name="transaction"><para>The <see cref="IDbTransaction"/> to use.</para></param>
        /// <returns>Number of records affected.</returns>
        public int UpdateDataSet(DataSet dataSet,
                                 string tableName,
                                 DbCommand insertCommand,
                                 DbCommand updateCommand,
                                 DbCommand deleteCommand,
                                 DbTransaction transaction)
        {
            return UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, transaction, null);
        }

        #region Async methods

        /// <summary>
        /// Does this <see cref='Database'/> object support asynchronous execution?
        /// </summary>
        /// <value>Base class always returns false.</value>
        public virtual bool SupportsAsync => false;

        /// <summary>
        /// Initiates the asynchronous execution of the <see cref="DbCommand"/> which will return the number of affected records.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <seealso cref="Database.ExecuteNonQuery(DbCommand)"/>
        /// <seealso cref="EndExecuteNonQuery(IAsyncResult)"/>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
        /// which returns the number of affected records.</para>
        /// </returns>
        public virtual IAsyncResult BeginExecuteNonQuery(DbCommand command, AsyncCallback callback, object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <see cref="DbCommand"/> inside a transaction which will return the number of affected records.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="DbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <seealso cref="Database.ExecuteNonQuery(DbCommand)"/>
        /// <seealso cref="EndExecuteNonQuery(IAsyncResult)"/>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
        /// which returns the number of affected records.</para>
        /// </returns>
        public virtual IAsyncResult BeginExecuteNonQuery(DbCommand command,
                                                         DbTransaction transaction,
                                                         AsyncCallback callback,
                                                         object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> which will return the number of rows affected.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The name of the stored procedure to execute.</para>
        /// </param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
        /// which returns the number of affected records.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteNonQuery(string,object[])"/>
        /// <seealso cref="EndExecuteNonQuery(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteNonQuery(string storedProcedureName,
                                                         AsyncCallback callback,
                                                         object state,
                                                         params object[] parameterValues)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> inside a transaction which will return the number of rows affected.
        /// </summary>
        /// <param name="storedProcedureName">
        /// <para>The name of the stored procedure to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="DbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <param name="parameterValues">
        /// <para>An array of parameters to pass to the stored procedure. The parameter values must be in call order as they appear in the stored procedure.</para>
        /// </param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
        /// which returns the number of affected records.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteNonQuery(string,object[])"/>
        /// <seealso cref="EndExecuteNonQuery(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteNonQuery(DbTransaction transaction,
            string storedProcedureName,
            AsyncCallback callback,
            object state,
            params object[] parameterValues)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> which will return the number of rows affected.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
        /// which returns the number of affected records.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteNonQuery(CommandType,string)"/>
        /// <seealso cref="EndExecuteNonQuery(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteNonQuery(CommandType commandType,
                                                         string commandText,
                                                         AsyncCallback callback,
                                                         object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> inside a transaction which will return the number of rows affected.
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
        /// this value is also needed when invoking <see cref="EndExecuteNonQuery"/>,
        /// which returns the number of affected records.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteNonQuery(CommandType,string)"/>
        /// <seealso cref="EndExecuteNonQuery(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteNonQuery(DbTransaction transaction,
                                                         CommandType commandType,
                                                         string commandText,
                                                         AsyncCallback callback,
                                                         object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Finishes asynchronous execution of a SQL statement, returning the number of affected records.
        /// </summary>
        /// <param name="asyncResult">
        /// <para>The <see cref="IAsyncResult"/> returned by a call to any overload of <see cref="BeginExecuteNonQuery(DbCommand, AsyncCallback, object)"/>.</para>
        /// </param>
        /// <seealso cref="Database.ExecuteNonQuery(DbCommand)"/>
        /// <seealso cref="BeginExecuteNonQuery(DbCommand, AsyncCallback, object)"/>
        /// <seealso cref="BeginExecuteNonQuery(DbCommand, DbTransaction, AsyncCallback, object)"/>
        /// <returns>
        /// <para>The number of affected records.</para>
        /// </returns>
        public virtual int EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            AsyncNotSupported();
            return 0;
        }

        /// <summary>
        /// Initiates the asynchronous execution of a <paramref name="command"/> which will return a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
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
        public virtual IAsyncResult BeginExecuteReader(DbCommand command, AsyncCallback callback, object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of a <paramref name="command"/> inside a transaction which will return a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
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
        public virtual IAsyncResult BeginExecuteReader(DbCommand command,
                                                       DbTransaction transaction,
                                                       AsyncCallback callback,
                                                       object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> which will return a <see cref="IDataReader"/>.
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
        public virtual IAsyncResult BeginExecuteReader(string storedProcedureName,
                                                       AsyncCallback callback,
                                                       object state,
                                                       params object[] parameterValues)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> inside a transaction which will return a <see cref="IDataReader"/>.
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
        public virtual IAsyncResult BeginExecuteReader(DbTransaction transaction,
                                                       string storedProcedureName,
                                                       AsyncCallback callback,
                                                       object state,
                                                       params object[] parameterValues)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="commandText"/>
        /// interpreted as specified by the <paramref name="commandType" /> which will return
        /// a <see cref="IDataReader"/>. When the async operation completes, the
        /// <paramref name="callback"/> will be invoked on another thread to process the
        /// result.
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
        public virtual IAsyncResult BeginExecuteReader(CommandType commandType,
                                                       string commandText,
                                                       AsyncCallback callback,
                                                       object state)
        {
            AsyncNotSupported();
            return null;
        }


        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> inside an transaction which will return a <see cref="IDataReader"/>.
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
        public virtual IAsyncResult BeginExecuteReader(DbTransaction transaction,
                                                       CommandType commandType,
                                                       string commandText,
                                                       AsyncCallback callback,
                                                       object state)
        {
            AsyncNotSupported();
            return null;
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
        public virtual IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of a <paramref name="command"/> which will return a single value.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteScalar"/>,
        /// which returns the actual result.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteScalar(DbCommand)"/>
        /// <seealso cref="EndExecuteScalar(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteScalar(DbCommand command, AsyncCallback callback, object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of a <paramref name="command"/> inside a transaction which will return a single value.
        /// </summary>
        /// <param name="command">
        /// <para>The <see cref="DbCommand"/> to execute.</para>
        /// </param>
        /// <param name="transaction">
        /// <para>The <see cref="DbTransaction"/> to execute the command within.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteScalar"/>,
        /// which returns the actual result.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteScalar(DbCommand, DbTransaction)"/>
        /// <seealso cref="EndExecuteScalar(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteScalar(DbCommand command,
                                                       DbTransaction transaction,
                                                       AsyncCallback callback,
                                                       object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> which will return a single value.
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
        /// this value is also needed when invoking <see cref="EndExecuteScalar"/>,
        /// which returns the actual result.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteScalar(string, object[])"/>
        /// <seealso cref="EndExecuteScalar(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteScalar(string storedProcedureName,
                                                       AsyncCallback callback,
                                                       object state,
                                                       params object[] parameterValues)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of <paramref name="storedProcedureName"/> using the given <paramref name="parameterValues" /> inside a transaction which will return a single value.
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
        /// this value is also needed when invoking <see cref="EndExecuteScalar"/>,
        /// which returns the actual result.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteScalar(DbTransaction, string, object[])"/>
        /// <seealso cref="EndExecuteScalar(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteScalar(DbTransaction transaction,
                                                       string storedProcedureName,
                                                       AsyncCallback callback,
                                                       object state,
                                                       params object[] parameterValues)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> which will return a single value.
        /// </summary>
        /// <param name="commandType">
        /// <para>One of the <see cref="CommandType"/> values.</para>
        /// </param>
        /// <param name="commandText">
        /// <para>The command text to execute.</para>
        /// </param>
        /// <param name="callback">The async callback to execute when the result of the operation is available. Pass <langword>null</langword>
        /// if you don't want to use a callback.</param>
        /// <param name="state">Additional state object to pass to the callback.</param>
        /// <returns>
        /// <para>An <see cref="IAsyncResult"/> that can be used to poll or wait for results, or both;
        /// this value is also needed when invoking <see cref="EndExecuteScalar"/>,
        /// which returns the actual result.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteScalar(CommandType, string)"/>
        /// <seealso cref="EndExecuteScalar(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteScalar(CommandType commandType,
                                                       string commandText,
                                                       AsyncCallback callback,
                                                       object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Initiates the asynchronous execution of the <paramref name="commandText"/> interpreted as specified by the <paramref name="commandType" /> inside an transaction which will return a single value.
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
        /// this value is also needed when invoking <see cref="EndExecuteScalar"/>,
        /// which returns the actual result.</para>
        /// </returns>
        /// <seealso cref="Database.ExecuteScalar(DbTransaction, CommandType, string)"/>
        /// <seealso cref="EndExecuteScalar(IAsyncResult)"/>
        public virtual IAsyncResult BeginExecuteScalar(DbTransaction transaction,
                                                       CommandType commandType,
                                                       string commandText,
                                                       AsyncCallback callback,
                                                       object state)
        {
            AsyncNotSupported();
            return null;
        }

        /// <summary>
        /// Finishes asynchronous execution of a Transact-SQL statement, returning the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="asyncResult">
        /// <para>The <see cref="IAsyncResult"/> returned by a call to any overload of BeginExecuteScalar.</para>
        /// </param>
        /// <seealso cref="Database.ExecuteScalar(DbCommand)"/>
        /// <seealso cref="BeginExecuteScalar(DbCommand,AsyncCallback,object)"/>
        /// <seealso cref="BeginExecuteScalar(DbCommand,DbTransaction,AsyncCallback,object)"/>
        /// <returns>
        /// <para>The value of the first column of the first row in the result set returned by the query.
        /// If the result didn't have any columns or rows <b>null</b> (<b>Nothing</b> in Visual Basic).</para>
        /// </returns>
        public virtual object EndExecuteScalar(IAsyncResult asyncResult)
        {
            AsyncNotSupported();
            return null;
        }

        private void AsyncNotSupported()
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                                              Resources.AsyncOperationsNotSupported,
                                                              GetType().Name));
        }

        #endregion

        /// <summary>
        /// Returns the starting index for parameters in a command.
        /// </summary>
        /// <returns>The starting index for parameters in a command.</returns>
        protected virtual int UserParametersStartIndex()
        {
            return 0;
        }

        /// <summary>
        /// This is a helper class that is used to manage the lifetime of a connection for various
        /// Execute methods. We needed this class to support implicit transactions created with
        /// the <see cref="TransactionScope"/> class. In this case, the various Execute methods
        /// need to use a shared connection instead of a new connection for each request in order
        /// to prevent a distributed transaction.
        /// </summary>
        protected sealed class OldConnectionWrapper : IDisposable
        {
            readonly bool disposeConnection;

            /// <summary>
            /// Create a new "lifetime" container for a <see cref="DbConnection"/> instance.
            /// </summary>
            /// <param name="connection">The connection</param>
            /// <param name="disposeConnection">
            /// Whether or not to dispose of the connection when this class is disposed.
            /// </param>
            public OldConnectionWrapper(DbConnection connection, bool disposeConnection)
            {
                Connection = connection;
                this.disposeConnection = disposeConnection;
            }

            /// <summary>
            /// Gets the actual connection.
            /// </summary>
            public DbConnection Connection { get; }

            /// <summary>
            /// Dispose the wrapped connection, if appropriate.
            /// </summary>
            public void Dispose()
            {
                if (disposeConnection)
                {
                    Connection.Dispose();
                }

                GC.SuppressFinalize(this);
            }
        }
    }
}
