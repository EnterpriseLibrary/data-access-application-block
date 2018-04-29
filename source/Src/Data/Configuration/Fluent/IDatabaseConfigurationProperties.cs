// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Database configuration properties that apply to all databases.
    /// </summary>
    /// <remarks>This interface is intended to support a fluent-style configuration interface.</remarks>
    public interface IDatabaseConfigurationProperties : IDatabaseConfigurationProviderEntry, IDatabaseConfiguration
    {
        ///<summary>
        /// Set this database as the default one in the configuration.
        ///</summary>
        ///<returns></returns>
        IDatabaseConfigurationProperties AsDefault();
    }
}
