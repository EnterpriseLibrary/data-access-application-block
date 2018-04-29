DATA ACCESS APPLICATION BLOCK (DAAB) BVT
http://daab.codeplex.com/

These tests rely on a variety of databases that must be configured prior to running the tests.  
The configuration assumes that for SQL Server the database used will be (localdb)\v11.0 with integrated authentication.

To run the tests the following steps will need to be performed:

A. Build the Data Access Application Block source code before building the BVT solution.

B. Connect to (localdb)\v11.0 and run SQL scripts located in DatabaseSetupScripts:
	1.DataAccessQuickStarts.sql
	2.instnwnd.sql
	3.TestSchemaAdditionsForNorthWind.sql
	4.TestDatabase.sql

C. Install Oracle and then run the SQL scripts located in DatabaseSetupScripts\OracleDBScripts:
	1.Table\alldb.sql
	2.Packages\ENTLIBTEST.pks
	2.Packages\ENTLIBTEST.pkb
	3.SP\1.cursor.sql
	All other 18 scripts located in 3.SP directory
	4.Data\alldata.sql

The Oracle connection string is configured as follows:

SID: XE 
PORT:1521
User Id: SYSTEM
Password: oracle

These values will need to be modified of the Oracle installation differs.


Microsoft patterns & practices
http://microsoft.com/practices