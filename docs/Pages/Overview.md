## Data Access Application Block User Guide  {#Overview}

[TOC]

This guide shows how to use the Enterprise Library Data Access Application Block (DAAB for short) version 7.0. For a guide
to version 6.0, see the [original documentation by Microsoft][1].

### What is the Data Access Application Block

The Data Access Application Block abstracts the actual database you are using, and exposes a collection of methods that
make it easy to access that database and to perform common tasks. The block is designed to simplify the task of calling
stored procedures, but it also provides full support for the use of parameterized SQL statements.

In other words, the Data Access Application Block provides access to the most often used features of ADO.NET in
simple-to-use classes and provides a corresponding boost to developer productivity.

In addition to the more common approaches familiar to users of ADO.NET, the Data Access block also exposes techniques
for asynchronous data access for databases that support this feature using the Asynchronous Programming Model (TPL
coming in the near future), and provides the ability to return data as a sequence of objects suitable for client-side
querying using techniques such as Language Integrated Query (LINQ).

However, the block is not intended to be an Object/Relational Mapping (O/RM) solution. Although it uses mappings to
relate parameters and relational data with the properties of objects, but does not implement an O/RM modeling solution.

### NuGet Packages
DAAB is composed of a base package, called [EnterpriseLibrary.Data.NetCore][2], and several other packages which support
specific database engines. For databases which are not directly supported by DAAB, only the base package is needed. The
database-specific packages support unique features of those systems.


Supported Database | Package                                  | .NET Framework support
-------------------|------------------------------------------|------------
Generic            | EnterpriseLibrary.Data.NetCore           | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
OLE DB             | EnterpriseLibrary.Data.OleDb.NetCore     | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
SQL Server         | EnterpriseLibrary.Data.SqlServer.NetCore | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
Oracle             | EnterpriseLibrary.Data.Oracle.NetCore    | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
ODBC               | EnterpriseLibrary.Data.Odbc.NetCore      | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
SQL Server CE      | EnterpriseLibrary.Data.SqlCe.NetCore     | .NET 4.5.2, 4.6, 4.7

### Configuration  {#Configuration}
After installing the appropriate package(s), you should add the following configuration elements to your app.config or web.config
file.
1. Under the `<configSections>` element, add the following section:
```xml
    <section name="dataConfiguration" type="Microsoft.Practices.EnterpriseLibrary.Data.Configuration.DatabaseSettings, Microsoft.Practices.EnterpriseLibrary.Data" />
```
2. Under the `<connectionStrings>` section, add a normal connection string to your database, with a `name`,
   `connectionString` and `providerName` attributes. DAAB supports the following provider names:
    1. `System.Data.SqlClient` (.NET built-in provider for SQL Server)
    2. `Oracle.ManagedDataAccess.Client` (Managed ADO.NET provider by Oracle, known as ODP.NET)
    3. `System.Data.OleDb`
    4. `System.Data.Odbc`
    5. `System.Data.SqlServerCe.4.0` (Latest provider for SQL Server CE, available as a NuGet package)
3. Under the root `<configuration>` element, but somewhere bellow the `<configSections>`, add the following section:
```xml
  <dataConfiguration defaultDatabase="MyConnectionString">
  </dataConfiguration>
```
   The `defaultDatabase` attribute points to the `name` of the connection string above. In code, you can direct
   DAAB to use a different connection string.

4. Under the `<dataConfiguration>` add the following section:
```xml   
    <providerMappings>
      <add databaseType="" name="" />
    </providerMappings>
```
  1. `name` is a provider name, as listed above in section 2.
  2. `databaseType` is one of the following types:

   Database      | databaseType    
   --------------|--------------
   Generic       | "Microsoft.Practices.EnterpriseLibrary.Data.GenericDatabase, Microsoft.Practices.EnterpriseLibrary.Data"
   SQL Server CE | "Microsoft.Practices.EnterpriseLibrary.Data.SqlCe.SqlCeDatabase, Microsoft.Practices.EnterpriseLibrary.Data.SqlCe"
   Oracle        | "Microsoft.Practices.EnterpriseLibrary.Data.Oracle.OracleDatabase, Microsoft.Practices.EnterpriseLibrary.Data.Oracle"
   SQL Server    | "Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase, Microsoft.Practices.EnterpriseLibrary.Data.SqlServer"
   OLE DB        | "Microsoft.Practices.EnterpriseLibrary.Data.OleDb.OleDbDatabase, Microsoft.Practices.EnterpriseLibrary.Data.OleDb"
   ODBC          | "Microsoft.Practices.EnterpriseLibrary.Data.Odbc.OdbcDatabase, Microsoft.Practices.EnterpriseLibrary.Data.Odbc"

   You can add multiple mappings if you access more than one database type.

#### Oracle Configuration
For Oracle databases, we also need to configure packages.
1. Under `<configSections>`, in addition to the `dataConfiguration` section, add
```xml
   <section name="oracleConnectionSettings" type="Microsoft.Practices.EnterpriseLibrary.Data.Oracle.Configuration.OracleConnectionSettings, Microsoft.Practices.EnterpriseLibrary.Data.Oracle" />
```
2. Under the root `<configuration>` element, but somewhere bellow the `<configSections>`, add the following section:
```xml
   <oracleConnectionSettings>
     <add name="OracleTest">
       <packages>
         <add name="ENTLIBTEST" prefix="GetProductDetailsById" />
       </packages>
     </add>
   </oracleConnectionSettings>
```
  1. The `name` attribute in the `<add>` element points to the name of a connection string from section 2 above.
  2. in each package, the `name` in the `<add>` element is a package name, which will be prefixed to the
     stored procedure specified in the `prefix` attribute.


### Code Examples

#### Creating Database Instances
The simplest approach for creating a [Database] object or one of its descendants is calling the `CreateDefault`
or `Create` method of the `DatabaseProviderFactory` class, as shown here, and storing these instances in
application-wide variables so that they can be accessed from anywhere in the code.
```cs
// Configure the DatabaseFactory to read its configuration from the .config file
DatabaseProviderFactory factory = new DatabaseProviderFactory();

// Create the default Database object from the factory.
// The actual concrete type is determined by the configuration settings.
Database defaultDB = factory.CreateDefault();

// Create a Database object from the factory using the connection string name.
Database namedDB = factory.Create("ExampleDatabase");
```
where "ExampleDatabase" is a `name` of a connection string. With `CreateDefault`, the connection string specified by
the `defaultDatabase` attribute of the `dataConfiguration` element in web.config is used, as described above.
Using the default database is a useful approach because you can change which of the databases defined in your
configuration is the default simply by editing the configuration file, without requiring recompilation or
redeployment of the application.

Some features are only available in the concrete types for a specific database. For example, the `ExecuteXmlReader`
method is only available in the `SqlDatabase` class. If you want to use such features, you must cast the database
type you instantiate to the appropriate concrete type.

As alternative to using the `DatabaseProviderFactory` class, you could use the static `DatabaseFactory` façade
to create your `Database` instances. You must invoke the `SetDatabaseProviderFactory` method to set the details
of the default database from the configuration file.

```cs
DatabaseFactory.SetDatabaseProviderFactory(factory, false);
defaultDB = DatabaseFactory.CreateDatabase("ExampleDatabase");
// Uses the default database from the configuration file.
sqlServerDB = DatabaseFactory.CreateDatabase() as SqlDatabase;
```

In addition to using configuration to define the databases you will use, the Data Access block allows you to create
instances of concrete types that inherit from the `Database` class directly in your code, as shown here.

```cs
SqlDatabase sqlDatabase = new SqlDatabase(myConnectionString);
```

#### Reading Rows Using a Query with No Parameters
To read rows using a stored procedure with no parameters, retrieve an `IDataReader` and read values from it:
```cs
// Call the ExecuteReader method by specifying just the stored procedure name.
using (IDataReader reader = namedDB.ExecuteReader("MyStoredProcName"))
{
    // Use the values in the rows as required.
    DisplayRowValues(reader);
}
```
`ExecuteReader` also accept a `CommandType`, but the default value is `CommandType.StoredProcedure`.

To use an inline SQL statement, specify a `CommandType.Text`:
```cs
// Call the ExecuteReader method by specifying the command type
// as a SQL statement, and passing in the SQL statement.
using (IDataReader reader = namedDB.ExecuteReader(CommandType.Text, "SELECT TOP 1 * FROM OrderList"))
{
    // Use the values in the rows as required - here we are just displaying them.
    DisplayRowValues(reader);
}
```

You read the values from the `IDataReader` normally:
```cs
private void DisplayRowValues(IDataReader reader)
{
    while (reader.Read())
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            Console.WriteLine("{0} = {1}", reader.GetName(i), reader[i].ToString());
        }
        Console.WriteLine();
    }
}
```
#### Reading Rows Using a Query with unnamed Parameters
If you use only input parameters, you can pass their values to the stored procedure or SQL statement directly
and let the `Database` class wrap them in the appropriate `DbParameter` objects. You must pass them in the same
order as they are expected by the query, because you are not using names for these parameters. The following code
executes a stored procedure that takes a single string parameter.
```cs
// Call the ExecuteReader method with the stored procedure
// name and an Object array containing the parameter values.
using (IDataReader reader = defaultDB.ExecuteReader("ListOrdersByState", "Colorado"))
{
    // Use the values in the rows as required - here we are just displaying them.
    DisplayRowValues(reader);
}
```

#### Reading Rows Using a Query with named Parameters
If you need to specify the data-types of the parameters, e.g. if it can't be inferred from the .NET data type, or you
need to specify the parameter direction (input or output), you can access the provider independent `DbCommand`
object for the query and add parameters using methods on the `Database` object. You can add parameters with a
specific direction using the `AddInParameter` or `AddOutParameter` method, or by using the `AddParameter`
method and providing a value for the `ParameterDirection` parameter. You can change the value of existing parameters
already added to the command using the `GetParameterValue` and `SetParameterValue` methods.
```cs
// Read data with a SQL statement that accepts one parameter prefixed with @.
string sqlStatement = "SELECT TOP 1 * FROM OrderList WHERE State LIKE @state";

// Create a suitable command type and add the required parameter.
using (DbCommand sqlCmd = defaultDB.GetSqlStringCommand(sqlStatement))
{
    // Any required prefix, such as '@' or ':' will be added automatically if missing.
    defaultDB.AddInParameter(sqlCmd, "state", DbType.String, "New York");
    
    // Call the ExecuteReader method with the command.
    using (IDataReader sqlReader = defaultDB.ExecuteReader(sqlCmd))
    {
        DisplayRowValues(sqlReader);
    }
}

// Now read the same data with a stored procedure that accepts one parameter.
string storedProcName = "ListOrdersByState";

// Create a suitable command type and add the required parameter.
using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand(storedProcName))
{
    defaultDB.AddInParameter(sprocCmd, "state", DbType.String, "New York");
    
    // Call the ExecuteReader method with the command.
    using (IDataReader sprocReader = defaultDB.ExecuteReader(sprocCmd))
    {
        DisplayRowValues(sprocReader);
    }
}
```
#### Retrieving Data as Objects
DAAB allows you to extract data using a query, and have the data returned to you as a sequence of objects that
implements the `IEnumerable` interface. This allows you to execute queries, or obtain lists or arrays of objects
that represent the original data in the database.

The block provides two core classes for performing this kind of query: the `SprocAccessor` and the
`SqlStringAccessor`. You can create and execute these accessors in one operation using the `ExecuteSprocAccessor`
and `ExecuteSqlAccessor` extension methods of the `Database` class, or create a new accessor directly and then call its
`Execute` method.

If you do not specify an output mapper, the block uses a default map builder class that maps the column names
of the returned data to properties of the objects it creates. Alternatively, you can create a custom mapping to
specify the relationship between columns in the row set and the properties of the objects. Inferring the details
required to create the correct mappings means that the default output mappers **can have an effect
on performance**. You may prefer to create your own custom mappers and retain a reference to them for reuse when
possible to maximize performance of your data access processes when using accessors.

The following code shows how you can use an accessor to execute a stored procedure. You must specify the object
type that you want the data returned as—in this example it is a simple class named `Product` that has the three
properties: `ID`, `Name`, and `Description`.

The stored procedure takes a single parameter that is a search string, and returns details of all products in
the database that contain this string. Therefore, the code calls the `ExecuteSprocAccessor` extension method
passing the search string as the single parameter. It specifies the `Product` class as the type of object to
return, and passes to the method the name of the stored procedure to execute and the array of parameter values.

```cs
// Create and execute a sproc accessor that uses the default
// parameter and output mappings.
var productData = defaultDB.ExecuteSprocAccessor<Product>("GetProductList", "%bike%");

// Display the results
foreach (var item in results)
{
    Console.WriteLine("Product ID: {0}", item.ID);
    Console.WriteLine("Product Name: {0}", item.Name);
    Console.WriteLine("Description: {0}", item.Description);
    Console.WriteLine();
}
```

#### Creating and Using Mappers

In some cases, you may need to create a custom parameter mapper to pass your parameters to the query that the
accessor will execute. This typically occurs when you need to execute a SQL statement to work with a database
system that does not support parameter resolution, or when a default mapping cannot be inferred due to a mismatch
in the number or types of the parameters. The parameter mapper class must implement the `IParameterMapper` interface
and contain a method named `AssignParameters` that takes a reference to the current `DbCommand` instance and the array
of parameters. The method simply needs to add the required parameters to the `DbCommand` object's `Parameters`
collection.

More often, you will need to create a custom output mapper. To help you do this, the block provides a class called
`MapBuilder` that you can use to create the set of mappings you require between the columns of the data set returned
by the query and the properties of the objects you need.

By default, the accessor will expect to generate a simple sequence of a single type of object (in our earlier
example, this was a sequence of the `Product` class). However, you can use an accessor to return a more complex
graph of objects if you wish. For example, you might execute a query that returns a series of `Order` objects and
the related `OrderLines` objects for all of the selected orders. Simple output mapping cannot cope with this scenario,
and neither can the `MapBuilder` class. In this case, you would create a result set mapper by implementing the
`IResultSetMapper` interface. Your custom row set mapper must contain a method named `MapSet` that receives a
reference to an object that implements the `IDataReader` interface. The method should read all of the data available
through the data reader, processes it to create the sequence of objects you require, and return this sequence.

#### Retrieving XML Data
SQL Server allows you to extract data as a series of XML elements, by executing specially formatted SQL queries.
The Data Access block provides the `ExecuteXmlReader` method for querying data as XML. It takes a SQL statement
that contains the `FOR XML` statement and executes it against the database, returning the result as an `XmlReader`.
You can iterate through the resulting XML elements or work with them in any of the ways supported by the XML
classes in the .NET Framework. However, as the implementations of this type of query differ in other database
systems, it is only available when you specifically use the `SqlDatabase` class (rather than the `Database` class).

The following code shows how you can obtain a `SqlDatabase` instance, specify a suitable XML query, and execute
it using the `ExecuteXmlReader` method.

```cs
// Create a SqlDatabase object from configuration using the default database.
SqlDatabase sqlServerDB = DatabaseFactory.CreateDatabase() as SqlDatabase;

// Specify a SQL query that returns XML data.
string xmlQuery = "SELECT * FROM OrderList WHERE State = @state FOR XML AUTO";

// Create a suitable command type and add the required parameter
// NB: ExecuteXmlReader is only available for SQL Server databases
using (DbCommand xmlCmd = sqlServerDB.GetSqlStringCommand(xmlQuery))
{
    xmlCmd.Parameters.Add(new SqlParameter("state", "Colorado"));
    using (XmlReader reader = sqlServerDB.ExecuteXmlReader(xmlCmd))
    {
        // Iterate through the elements in the XmlReader
        while (!reader.EOF)
        {
            if (reader.IsStartElement())
            {
                Console.WriteLine(reader.ReadOuterXml());
            }
        }
    }
}
```

 **Note:** by default, the result is an XML fragment, and not a valid XML document. It is, effectively, a sequence
of XML elements that represent each row in the results set. Therefore, at minimum, you must wrap the output with a
single root element so that it is well-formed. See [XmlReader][3] for more details.

#### Retrieving Single Scalar Values
The Data Access block provides the `ExecuteScalar` method to extract a single scalar value based on a query that
selects either a single row or a single value. It executes the query you specify, and then returns the value of
the first column of the first row of the result set as an Object type.

The `ExecuteScalar` method has a set of overloads similar to the `ExecuteReader` method we used earlier in this
document. You can specify a `CommandType` (the default is `StoredProcedure`) and either a SQL statement or a stored
procedure name. You can also pass in an array of `Object` instances that represent the parameters for the query.
Alternatively, you can pass to the method a `DbCommand` object that contains any parameters you require.

The following code demonstrates passing a `DbCommand` object to the method to execute both an inline SQL statement
and a stored procedure. It obtains a suitable `DbCommand` instance from the current `Database` instance using the
`GetSqlStringCommand` and `GetStoredProcCommand` methods. You can add parameters to the command before calling
the `ExecuteScalar` method if required. However, to demonstrate the way the method works, the code here simply
extracts the complete row set. The result is a single `Object` that you must cast to the appropriate type before
displaying or consuming it in your code.

```cs
// Create a suitable command type for a SQL statement.
// NB: For efficiency, aim to return only a single value or a single row.
using (DbCommand sqlCmd = defaultDB.GetSqlStringCommand("SELECT [Name] FROM States"))
{
    // Call the ExecuteScalar method of the command.
    Console.WriteLine("Result using a SQL statement: {0}", defaultDB.ExecuteScalar(sqlCmd).ToString());
}

// Create a suitable command type for a stored procedure.
// NB: For efficiency, aim to return only a single value or a single row.
using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand("GetStatesList"))
{
    // Call the ExecuteScalar method of the command.
    Console.WriteLine("Result using a stored procedure: {0}", defaultDB.ExecuteScalar(sprocCmd).ToString());
}
```
The result it produces is shown here.
```
Result using a SQL statement: Alabama
Result using a stored procedure: Alabama
```
#### Retrieving Data Asynchronously
Databases are a major bottleneck in any enterprise application. One way that applications can minimize the
performance hit from data access is to perform it asynchronously. This means that the application code can
continue to execute, and the user interface can remain interactive during the process.  It is also very
useful in server applications where you can avoid blocking threads that could handle other requests, thus
improving utilization. However, keep in mind that asynchronous data access has an effect on connection and
data streaming performance over the wire. Don’t expect a query that returns ten rows to show any improvement
using an asynchronous approach—it is more likely to take longer to return the results!

The Data Access block provides asynchronous `Begin` and `End` versions of many of the standard data access
methods, including `ExecuteReader`, `ExecuteScalar`, `ExecuteXmlReader`, and `ExecuteNonQuery`. It also
provides asynchronous `Begin` and `End` versions of the `Execute` method for accessors that return data as a
sequence of objects. This approach is known as the Asynchronous Programming Model (APM). A future version of
the block will also support the [Task Parallel Library][4] (TPL). In the mean time, you can use
[TaskFactory.FromAsync][5] to wrap a `Begin` and `End` method with a `Task`.

Asynchronous processing in the Data Access block is only available for SQL Server databases. The `Database` class
includes a property named `SupportsAsync` that you can query to see if the current `Database` instance
supports asynchronous operations. The example for this chapter contains a simple check for this.

The `BeginExecuteReader` method does not accept a `CommandBehavior` parameter. By default, the method will
automatically set the `CommandBehavior` property on the underlying reader to `CloseConnection` unless you
specify a transaction when you call the method. If you do specify a transaction, it does not set the
`CommandBehavior` property.

Always ensure you call the appropriate `EndExecute` method when you use asynchronous data access, even if you
do not actually require access to the results, or call the Cancel method on the connection. Failing to do so
can cause memory leaks and consume additional system resources.

Using asynchronous data access with the Multiple Active Results Set (MARS) feature of ADO.NET may produce
unexpected behavior, and should generally be avoided.

The following code creates a `DBCommand` instance and adds two parameters, and then calls the `BeginExecuteReader`
method of the `Database` class to start the process. The code passes to this method a reference to the command
to execute (with its parameters already added), a Lambda expression to execute when the data retrieval process
completes, and a **null** value for the `AsyncState` parameter. The Lambda expression then calls the
`EndExecuteReader` method to obtain the results of the query execution. At this point you can consume the row
set in your application. Notice that the callback expression should handle any errors that may occur during the
asynchronous operation.

```cs
// Create command to execute stored procedure and add parameters.
DbCommand cmd = asyncDB.GetStoredProcCommand("ListOrdersSlowly");
asyncDB.AddInParameter(cmd, "state", DbType.String, "Colorado");
asyncDB.AddInParameter(cmd, "status", DbType.String, "DRAFT");

// Execute the query asynchronously specifying the command and the expression to execute when the data access
// process completes.
asyncDB.BeginExecuteReader(cmd,
  asyncResult =>
  {
    // Lambda expression executed when the data access completes.
    try
    {
      using (IDataReader reader = asyncDB.EndExecuteReader(asyncResult))
      {
        DisplayRowValues(reader);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error after data access completed: {0}", ex.Message);
    }
  }, null);
```

The `AsyncState` parameter can be used to pass any required state information into the callback. For example,
when you use a callback method instead of a lambda expression, you would pass a reference to the current `Database`
instance as the `AsyncState` parameter so that the callback code can call the `EndExecuteReader` (or other
appropriate End method) to obtain the results. When you use a Lambda expression, the current `Database` instance
is available within the expression and, therefore, you do not need to populate the `AsyncState` parameter.

As mentioned above, you can wrap a BeginXXX and EndXXX methods with a Task.
```cs
await Task<IDataReader>.Factory
        .FromAsync<DbCommand>(asyncDB.BeginExecuteReader, .
        asyncDB.EndExecuteReader, cmd, null);
```

#### Updating Data
The Data Access block makes it easy to execute inline SQL statements, or SQL statements within stored procedures,
that use the `UPDATE`, `DELETE`, or `INSERT` keywords. queries against a database. You can execute these kinds of
queries using the `ExecuteNonQuery` method of the `Database` class.

The `ExecuteNonQuery` method has a broad set of overloads. You can specify a `CommandType` (the default is
`StoredProcedure`) and either a SQL statement or a stored procedure name. You can also pass in an array of Object
instances that represent the parameters for the query. Alternatively, you can pass to the method a `DbCommand` object
that contains any parameters you require. There are also `Begin` and `End` versions that allow you to execute update
queries asynchronously. A future version will support a Task based asynchronous method.

The following code shows how you can use the `ExecuteNonQuery` method to update a row in a table in the database.
It updates the Description column of a single row in the Products table, checks that the update succeeded, and then
updates it again to return it to the original value. The first step is to create the command and add the required
parameters, as you've seen in earlier examples, and then call the `ExecuteNonQuery` method with the command as the
single parameter. Next, the code changes the value of the command parameter named `description` to the original
value in the database, and then executes the compensating update.

```cs
string oldDescription = "Carries 4 bikes securely; steel construction, fits 2\" receiver hitch.";
string newDescription = "Bikes tend to fall off after a few miles.";

// Create command to execute the stored procedure and add the parameters.
DbCommand cmd = defaultDB.GetStoredProcCommand("UpdateProductsTable");
defaultDB.AddInParameter(cmd, "productID", DbType.Int32, 84);
defaultDB.AddInParameter(cmd, "description", DbType.String, newDescription);

// Execute the query and check if one row was updated.
if (defaultDB.ExecuteNonQuery(cmd) == 1)
{
  // Update succeeded.
}
else
{
    Console.WriteLine("ERROR: Could not update just one row.");
}

// Change the value of the second parameter
defaultDB.SetParameterValue(cmd, "description", oldDescription);

// Execute query and check if one row was updated
if (defaultDB.ExecuteNonQuery(cmd) == 1)
{
  // Update succeeded.
}
else
{
    Console.WriteLine("ERROR: Could not update just one row.");
}
```

The `ExecuteNonQuery` method returns an integer value that is the number of rows updated (or, to use the more
accurate term, affected) by the query. In this example, we are specifying a single row as the target for the update
by selecting on the unique ID column. Therefore, we expect only one row to be updated—any other value means there
was a problem. If you are expecting to update multiple rows, you would check for a non-zero returned value.

#### Working with DataSets
##### Retrieving data as DataSet

If you need to retrieve data and store it in a way that allows you to push changes back into the database, you
will usually use a `DataSet`. The Data Access block supports simple operations on a normal (non-typed) `DataSet`,
including the capability to fill a `DataSet` and then update the original database table from the `DataSet`.

To fill a `DataSet`, you use the [ExecuteDataSet](@ref Microsoft.Practices.EnterpriseLibrary.Data.Database.ExecuteDataSet)
method, which returns a new instance of the `DataSet` class populated with a table containing the data for each
row set returned by the query (which may be a multiple-statement batch query). The tables in this `DataSet` will
have default names such as Table, Table1, and Table2.

If you want to load data into an existing `DataSet`, you use the `LoadDataSet` method. This allows you to
specify the name(s) of the target table(s) in the `DataSet`, and lets you add additional tables to an existing
`DataSet` or refresh the contents of specific tables in the DataSet.

Both of these methods have a broad set of overloads. You can specify a `CommandType` (the default is `StoredProcedure`)
and either a SQL statement or a stored procedure name. You can also pass the values that represent the input
parameters for the query. Alternatively, you can pass to the method a `DbCommand` object that contains any parameters
you require.

The following code example shows how you can use the `ExecuteDataSet` method with a SQL statement; with a stored
procedure and a parameter array; and with a command pre-populated with parameters. The code assumes you have created
the Data Access block `Database` instance named `db`.

```cs
DataSet productDataSet;

// Using a SQL statement.
string sql = "SELECT CustomerName, CustomerPhone FROM Customers";
productDataSet = db.ExecuteDataSet(CommandType.Text, sql);

// Using a stored procedure and a parameter array.
productDataSet = db.ExecuteDataSet("GetProductsByCategory", "%bike%");

// Using a stored procedure and a named parameter.
DbCommand cmd = db.GetStoredProcCommand("GetProductsByCategory");
db.AddInParameter(cmd, "CategoryID", DbType.Int32, 7);
productDataSet = db.ExecuteDataSet(cmd);
```

##### Updating the Database from a DataSet
To update data in a database from a `DataSet`, you use the [UpdateDataSet](@ref Microsoft.Practices.EnterpriseLibrary.Data.Database.UpdateDataSet)
method, which returns a total count of the number of rows affected by the update, delete, and insert operations.
The overloads of this method allow you to specify the source `DataSet` containing the updated rows, the name of
the table in the database to update, and references to the three [DbCommand] instances that the method will
execute to perform UPDATE, DELETE, and INSERT operations on the specified database table.

In addition, you can specify a value for the [UpdateBehavior](@ref  Microsoft.Practices.EnterpriseLibrary.Data.UpdateBehavior),
which determines how the method will apply the updates to the target table rows. You can specify one of the following
values for this parameter:

* **Standard**. If the underlying ADO.NET update process encounters an error, the update stops and no subsequent
  updates are applied to the target table.
* **Continue**. If the underlying ADO.NET update process encounters an error, the update will continue and attempt
  to apply any subsequent updates.
* **Transactional**. If the underlying ADO.NET update process encounters an error, all the updates made to all rows
  will be rolled back.

Finally, you can optionally provide a value for the `updateBatchSize` parameter of the `UpdateDataSet`
method. This forces the method to attempt to perform updates in batches instead of sending each one to the database
individually. This is more efficient, but the return value for the method will show only the number of updates made
in the final batch, and not the total number for all batches. Typically, you are likely to use a batch size value
between 10 and 100. You should experiment to find the most appropriate batch size; it depends on the type of
database you are using, the query you are executing, and the number of parameters for the query.

The following examples demonstrates the `ExecuteDataSet` and `UpdateDataSet` methods. It uses the simple overloads
of the `ExecuteDataSet` and `LoadDataSet` methods to fill two `DataSet` instances, using a separate routine named
`DisplayTableNames` (not shown here) to display the table names and a count of the number of rows in these tables.
This shows one of the differences between these two methods. Note that the `LoadDataSet` method requires a reference
to an existing `DataSet` instance, and an array containing the names of the tables to populate.

```cs
string selectSQL = "SELECT Id, Name, Description FROM Products WHERE Id > 90";

// Fill a DataSet from the Products table using the simple approach.
DataSet simpleDS = defaultDB.ExecuteDataSet(CommandType.Text, selectSQL);
DisplayTableNames(simpleDS, "ExecuteDataSet");

// Fill a DataSet from the Products table using the LoadDataSet method.
// This allows you to specify the name(s) for the table(s) in the DataSet.
DataSet loadedDS = new DataSet("ProductsDataSet");
defaultDB.LoadDataSet(CommandType.Text, selectSQL, loadedDS, new string[] { "Products" });
DisplayTableNames(loadedDS, "LoadDataSet");
```
This produces the following result:
```
Tables in the DataSet obtained using the ExecuteDataSet method:
 - Table named 'Table' contains 6 rows.

Tables in the DataSet obtained using the LoadDataSet method:
 - Table named 'Products' contains 6 rows.
```

Let's say you updated, added and deleted some row in the Products table. The next stage is to create the
commands that the `UpdateDataSet` method will use to update the target table in the database. The code declares
three suitable SQL statements, and then builds the commands and adds the requisite parameters to them. Note that
each parameter may be applied to multiple rows in the target table, so the actual value must be dynamically set
based on the contents of the DataSet row whose updates are currently being applied to the target table.

This means that you must specify, in addition to the parameter name and data type, the name and the version
(Current or Original) of the row in the DataSet to take the value from. For an INSERT command, you need the
current version of the row that contains the new values. For a DELETE command, you need the original value of
the ID to locate the row in the table that will be deleted. For an UPDATE command, you need the original value
of the ID to locate the row in the table that will be updated, and the current version of the values with which
to update the remaining columns in the target table row.

```cs

string addSQL = "INSERT INTO Products (Name, Description) VALUES (@name, @description)";
string updateSQL = "UPDATE Products SET Name = @name, Description = @description WHERE Id = @id";
string deleteSQL = "DELETE FROM Products WHERE Id = @id";

// Create the commands to update the original table in the database
DbCommand insertCommand = defaultDB.GetSqlStringCommand(addSQL);
defaultDB.AddInParameter(insertCommand, "name", DbType.String, "Name", DataRowVersion.Current);
defaultDB.AddInParameter(insertCommand, "description", DbType.String, "Description", DataRowVersion.Current);

DbCommand updateCommand = defaultDB.GetSqlStringCommand(updateSQL);
defaultDB.AddInParameter(updateCommand, "name", DbType.String, "Name", DataRowVersion.Current);
defaultDB.AddInParameter(updateCommand, "description", DbType.String, "Description", DataRowVersion.Current);
defaultDB.AddInParameter(updateCommand, "id", DbType.String, "Id", DataRowVersion.Original);

DbCommand deleteCommand = defaultDB.GetSqlStringCommand(deleteSQL);
defaultDB.AddInParameter(deleteCommand, "id", DbType.Int32, "Id", DataRowVersion.Original);
```

Finally, you can apply the changes by calling the UpdateDataSet method, as shown here:

```cs
// Apply the updates in the DataSet to the original table in the database.
int rowsAffected = defaultDB.UpdateDataSet(loadedDS, "Products", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Standard);
Console.WriteLine("Updated a total of {0} rows in the database.", rowsAffected);
```
The code captures and displays the number of rows affected by the updates. As expected, this is three, as shown
in the final section of the output from the example.

```
Updated a total of 3 rows in the database.
```

#### Managing Connections
Connections are scarce, expensive in terms of resource usage, and can cause a big performance hit if not managed
correctly. You must obviously open a connection before you can access data, and you should make sure it is closed
after you have finished with it. However, if the operating system does actually create a new connection, and then
closes and destroys it every time, execution in your applications would flow like molasses.

Instead, ADO.NET holds a pool of open connections that it hands out to applications that require them. Data access
code must still go through the motions of calling the methods to create, open, and close connections, but ADO.NET
automatically retrieves connections from the connection pool when possible, and decides when and whether to actually
close the underlying connection and dispose it. The main issues arise when you have to decide when and how your code
should call the `Close` method. The Data Access block helps to resolve these issues by automatically managing
connections as far as is reasonably possible.

When you use the Data Access block to retrieve a `DataSet`, the `ExecuteDataSet` method automatically opens and
closes the connection to the database. If an error occurs, it will ensure that the connection is closed. If you
want to keep a connection open, perhaps to perform multiple operations over that connection, you can access the
`ActiveConnection` property of your `DbCommand` object and open it before calling the ExecuteDataSet method. The
`ExecuteDataSet` method will leave the connection open when it completes, so you must ensure that your code
closes it afterwards.

In contrast, when you retrieve a `DataReader` or an `XmlReader`, the [ExecuteReader](@ref Microsoft.Practices.EnterpriseLibrary.Data.Database.ExecuteReader)
method (or, in the case of the `XmlReader`, the [ExecuteXmlReader](@ref Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase.ExecuteXmlReader)
method) must leave the connection open so that you can read the data. The `ExecuteReader` method sets the
`CommandBehavior` property of the reader to `CloseConnection` so that the connection is closed when you dispose the
reader. Commonly, you will use a using construct to ensure that the reader is disposed, as shown here:

```cs
using (IDataReader reader = db.ExecuteReader(cmd))
{
    // use the reader here
}
```

Typically, when you use the `ExecuteXmlReader` method, you will explicitly close the connection after you dispose
the reader. This is because the underlying `XmlReader` class does not expose a `CommandBehavior` property. However,
you should still use the same approach as with a `DataReader` (a using statement) to ensure that the `XmlReader`
is correctly closed and disposed.

```cs
using (XmlReader reader = db.ExecuteXmlReader(cmd))
{
    // use the reader here
}
```

Finally, if you want to be able to access the connection your code is using, perhaps to create connection-based
transactions in your code, you can use the Data Access block methods to explicitly create a connection for your
data access methods to use. This means that you must manage the connection yourself, usually through a using
statement as shown below, which automatically closes and disposes the connection:

```cs
using (DbConnection conn = db.CreateConnection())
{
    conn.Open();
    try
    {
        // perform data access here
    }
    catch
    {
        // handle any errors here
    }
}
```

#### Working with Connection-Based Transactions
A common requirement in many applications is to perform multiple updates to data so that they all succeed,
or can all be undone (rolled back) to leave the databases in a valid state that is consistent with the original
content. The traditional example is when your bank carries out a monetary transaction that requires them to subtract
a payment from one account and add the same amount to another account (or perhaps slightly less, with the commission
going into their own account).

Transactions should follow the four **ACID** principles. These are **Atomicity** (all of the tasks of a transaction
are performed or none of them are), **Consistency** (the database remains in a consistent state before and after the
transaction), **Isolation** (other operations cannot access or see the data in an intermediate state during a
transaction), and **Durability** (the results of a successful transaction are persisted and will survive system
failure).

You can execute transactions when all of the updates occur in a single database by using the features of your
database system (by including the relevant commands such as `BEGIN TRANSACTION` and `ROLLBACK TRANSACTION` in
your stored procedures). ADO.NET also provides features that allow you to perform connection-based transactions
over a single connection. This allows you to perform multiple actions on different tables in the same database,
and manage the commit or rollback in your data access code.

All of the methods of the Data Access block that retrieve or update data have overloads that accept a reference
to an existing transaction as a DbTransaction type. As an example of their use, the following code explicitly
creates a transaction over a connection. It assumes you have created the Data Access block [Database] instance named
`db` and two [DbCommand] instances named `cmdA` and `cmdB`.

```cs
using (DbConnection conn = db.CreateConnection())
{
    conn.Open();
    DbTransaction trans = conn.BeginTransaction();

    try
    {
        // execute commands, passing in the current transaction to each one
        db.ExecuteNonQuery(cmdA, trans);
        db.ExecuteNonQuery(cmdB, trans);
        trans.Commit();    // commit the transaction
    }
    catch
    {
        trans.Rollback();  // rollback the transaction
    }
}
```

 [1]: https://docs.microsoft.com/en-us/previous-versions/msp-n-p/dn440726(v=pandp.60)
 [2]: https://www.nuget.org/packages/EnterpriseLibrary.Data.NetCore/
 [3]: https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlreader?view=netframework-4.8
 [4]: https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl
 [5]: https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory.fromasync
 [DbCommand]: https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand
 [Database]: @ref Microsoft.Practices.EnterpriseLibrary.Data.Database