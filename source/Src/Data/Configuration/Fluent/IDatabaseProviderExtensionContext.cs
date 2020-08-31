// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Provides extension context for database provider extensions.
    /// </summary>
    public interface IDatabaseProviderExtensionContext
    {
        ///<summary>
        /// The current connection string under construction in the fluent interface.
        ///</summary>
        ConnectionStringSettings ConnectionString { get; }

        ///<summary>
        /// Context of the current builder for the extension
        ///</summary>
        IConfigurationSourceBuilder Builder { get;  }
    }
}
