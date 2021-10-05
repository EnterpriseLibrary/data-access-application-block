## Data Access Application Block User Guide

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

### Configuration
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
The simplest approach for creating a `Database` object or one of its descendants is calling the `CreateDefault`
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
instances of concrete types that inherit from the Database class directly in your code, as shown here.

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

 [1]: https://docs.microsoft.com/en-us/previous-versions/msp-n-p/dn440726(v=pandp.60)
 [2]: https://www.nuget.org/packages/EnterpriseLibrary.Data.NetCore/