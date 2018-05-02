[![Build status](https://ci.appveyor.com/api/projects/status/2yn8u67mpbnjfyvt/branch/master?svg=true)](https://ci.appveyor.com/project/EnterpriseLibrary/data-access-application-block/branch/master)
[![License](https://img.shields.io/badge/license-apache%202.0-60C060.svg)](https://github.com/EnterpriseLibrary/data-access-application-block/blob/master/LICENSE)


# DATA ACCESS APPLICATION BLOCK (DAAB) PRE-RELEASE

Summary: The Data Access Application Block abstracts the actual database you are using, and exposes a collection of methods that make it easy to access that database and to perform common tasks. The block is designed to simplify the task of calling stored procedures, but it also provides full support for the use of parameterized SQL statements.

In other words, the Data Access Application Block provides access to the most often used features of ADO.NET in simple-to-use classes and provides a corresponding boost to developer productivity.

In addition to the more common approaches familiar to users of ADO.NET, the Data Access block also exposes techniques for asynchronous data access for databases that support this feature, and provides the ability to return data as a sequence of objects suitable for client-side querying using techniques such as Language Integrated Query (LINQ). However, the block is not intended to be an Object/Relational Mapping (O/RM) solution. Although it uses mappings to relate parameters and relational data with the properties of objects, but does not implement an O/RM modeling solution. 
