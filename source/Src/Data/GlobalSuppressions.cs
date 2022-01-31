// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.Oracle.OracleDataReaderWrapper.#System.Collections.IEnumerable.GetEnumerator()", Justification = "As designed")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.DataReaderWrapper.#System.Data.IDataRecord.Item[System.Int32]", Justification = "As designed")]
[assembly: SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.DataReaderWrapper.#System.Data.IDataRecord.Item[System.String]", Justification = "As designed")]

[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.ConnectionString.#UserName", Justification = "normalized to parse tokens")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.ConnectionString.#UserName", Justification = "normalized to parse tokens")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.ConnectionString.#Password", Justification = "normalized to parse tokens")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.ConnectionString.#Password", Justification = "normalized to parse tokens")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.ConnectionString.#GetTokenPositions(System.String,System.Int32&,System.Int32&)", Justification = "normalized to parse tokens")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Scope = "member", Target = "Microsoft.Practices.EnterpriseLibrary.Data.ConnectionString.#RemoveCredentials(System.String)", Justification = "normalized to parse tokens")]
[assembly: SuppressMessage("Usage", "RCS1155:Use StringComparison when comparing strings.", Justification = "Not going to optimize code now", Scope = "member", Target = "~M:System.Data.Common.DbProviderFactories.IncludeFrameworkFactoryClasses(System.Data.DataTable)~System.Data.DataTable")]
