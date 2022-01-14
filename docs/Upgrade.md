## Upgrading from version 6 to version 7   {#Upgrade}
In previous versions, all databases were supported by one package, EnterpriseLibrary.Data.NetCore (or, if you are
using the original package from Microsoft, EnterpriseLibrary.Data). To enable support of new databases, and use the
best packages to support each database, this has changed in version 7. In this version, only a generic database
is supported by EnterpriseLibrary.Data.NetCore, while other databases are supported by specific packages, which
in turn depend on specific database ADO.NET provider packages.

One consequence of this new architecture, is that you can no longer rely on the Data Access Application Block to
automatically detect your database type from the `providerName` property of the connection string. You will have
to specify in the Web.config or App.config what database you are using.

To update your package from version 6.* to version 7.0, follow these steps:
1. Upgrade EnterpriseLibrary.Data.NetCore to version 7.0
2. Install the database-specific package for your database, using the following table:
   
   Supported Database | Package                                  | .NET Framework support
   -------------------|------------------------------------------|------------
   OLE DB             | EnterpriseLibrary.Data.OleDb.NetCore     | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
   SQL Server         | EnterpriseLibrary.Data.SqlServer.NetCore | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
   Oracle             | EnterpriseLibrary.Data.Oracle.NetCore    | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
   ODBC               | EnterpriseLibrary.Data.Odbc.NetCore      | .NET 4.5.2, 4.6, 4.7; .NET Core 2.1, 3.1; .NET Standard 2.0
   SQL Server CE      | EnterpriseLibrary.Data.SqlCe.NetCore     | .NET 4.5.2, 4.6, 4.7

3. Follow the instructions at the [Configuration](@ref Configuration) section to configure your database.
4. The base namespaces for the database providers other than the Generic provider have changed. If you reference
   database-specific types, such as `SqlDatabase` or `OracleDatabase`, you should change the `using` statements
   (`Imports` in VB.NET) as follows:

   Database provider  | Namespace
   -------------------|-----------------------------------------------------
   OLE DB             | Microsoft.Practices.EnterpriseLibrary.Data.OleDb
   SQL Server         | Microsoft.Practices.EnterpriseLibrary.Data.Sql
   Oracle             | Microsoft.Practices.EnterpriseLibrary.Data.Oracle
   ODBC               | Microsoft.Practices.EnterpriseLibrary.Data.Odbc
   SQL Server CE      | Microsoft.Practices.EnterpriseLibrary.Data.SqlCe
