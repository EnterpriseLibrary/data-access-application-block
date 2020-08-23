# DATA ACCESS APPLICATION BLOCK (DAAB) BVT
http://daab.codeplex.com/

These tests rely on a variety of databases that must be configured prior to running the tests.  
The configuration assumes that for SQL Server the database used will be (localdb)\v11.0 with integrated authentication.

To run the tests the following steps will need to be performed:

A. Build the Data Access Application Block source code before building the BVT solution.

B. Connect to (localdb)\v11.0 and run SQL scripts located in DatabaseSetupScripts:
   1. 1.DataAccessQuickStarts.sql
   2. 2.instnwnd.sql
   3. 3.TestSchemaAdditionsForNorthWind.sql
   4. 4.TestDatabase.sql

C. The Oracle connection string is configured as follows:

   SID: XE<br/>
   PORT:1521<br/>
   User Id: SYSTEM<br/>
   Password: oracle

   These values will need to be modified if the Oracle installation differs. Run this command on your Oracle XE database instance:

    ALTER USER SYSTEM IDENTIFIED BY oracle;


D. Install Oracle XE and then run the SQL scripts located in DatabaseSetupScripts\OracleDBScripts, using the credentials above:
   1. 1.Table\alldb.sql
   2. 2.Packages\ENTLIBTEST.pks
   3. 2.Packages\ENTLIBTEST.pkb
   4. 3.SP\1.cursor.sql<br/>
      All other 18 scripts located in 3.SP directory
   5. 4.Data\alldata.sql

**Tip:** It's best to run the scripts in [Oracle Developer Tools for Visual Studio](https://www.oracle.com/database/technologies/net-downloads.html)
or [SQL Developer](https://www.oracle.com/tools/downloads/sqldev-downloads.html), because some of them contain
multibyte character strings. SQL*Plus, the terminal utility that comes with Oracle, doesn't handle multibyte
characters correctly and will corrupt the data.

**Note:** Orders table contains dates in MM/DD/YYYY format. If your system date format is different, you should
run this command in order to change the date format for the session:

`ALTER SESSION SET NLS_DATE_FORMAT='MM/DD/YYYY';`

Microsoft patterns & practices<br/>
http://microsoft.com/practices