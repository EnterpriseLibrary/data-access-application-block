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
After installing the appropriate package(S), you should add the following configuration elements to your app.config or web.config
file.
1. Under the `<configSections>` element, add the following section:
```xml
    <section name="dataConfiguration" type="Microsoft.Practices.EnterpriseLibrary.Data.Configuration.DatabaseSettings, Microsoft.Practices.EnterpriseLibrary.Data" />
```
2. Under the `<connectionStrings>` section, add a normal connection string to your database, with a `name`,
   `connectionString` and `providerName` attributes. DAAB supports the following provider names:
    1. `System.Data.SqlClient`
    2. `Oracle.ManagedDataAccess.Client`
    3. `System.Data.OleDb`
    4. `System.Data.Odbc`
    5. `System.Data.SqlServerCe.4.0`
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

#### Oracle Configuration

### Code Examples

 [1]: https://docs.microsoft.com/en-us/previous-versions/msp-n-p/dn440726(v=pandp.60)
 [2]: https://www.nuget.org/packages/EnterpriseLibrary.Data.NetCore/