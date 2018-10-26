// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{
    ///<summary>
    /// This interface support the database configuration fluent interface.
    ///</summary>
    public interface IDatabaseConfigurationProviderEntry : IFluentInterface
    {
        ///<summary>
        /// Specify the type of database.
        ///</summary>
        IDatabaseConfigurationProviders ThatIs { get; }
    }
}
