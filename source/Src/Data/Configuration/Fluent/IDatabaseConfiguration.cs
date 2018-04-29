// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using EnterpriseLibrary.Common;

namespace EnterpriseLibrary.Data.Configuration.Fluent
{
    ///<summary>
    /// Supports configuring the data connections via fluent-style interface.
    ///</summary>
    public interface IDatabaseConfiguration : IFluentInterface
    {
        ///<summary>
        /// Configure a named database.
        ///</summary>
        ///<param name="databaseName">Name of database to configure</param>
        ///<returns></returns>
        IDatabaseConfigurationProperties ForDatabaseNamed(string databaseName);
    }
}
