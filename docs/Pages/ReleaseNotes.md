# Data Access Application Block Release Notes   {#ReleaseNotes}
## Version 7.0 RC1
* The block was split to several NuGet packages, one for each database provider. See [Upgrade](@ref Upgrade) and
  [User Guide](@ref Overview) for details.
* Support for some older .NET frameworks was dropped, in favor of Long Term Support versions:
  * Dropped support for .NET Framework 4.0, 4.5, .NET Core 2.0, 3.0.
  * Added support for .NET Framework 4.5.2, .NET Core 2.1, 3.1
* Fixed [bug #23 Oracle RAW data type is not cast to byte[] correctly][1]
* Fixed [bug #31 SprocAccessor<TResult>.Execute(params object[]) throws ObjectDisposedException under Oracle][2]
* Fixed [bug #32 DatabaseProviderFactory.Create(String) fails for an ODBC provider under .NET Core][3]
* Add support for `IN OUT` parameters in Oracle database provider ([issue #14][4])


[1]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/23
[2]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/31
[3]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/32
[4]: https://github.com/EnterpriseLibrary/data-access-application-block/issues/14