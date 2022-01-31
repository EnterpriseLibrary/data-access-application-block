// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
namespace Microsoft.Practices.EnterpriseLibrary.Data.Configuration.Fluent
{
    /// <summary>
    /// Extension point for database providers to connect to the data configuration fluent-api.
    /// </summary>
    /// <seealso cref="DataConfigurationSourceBuilderExtensions"/>
    /// <seealso cref="DatabaseConfigurationExtension"/>
    public interface IDatabaseConfigurationProviders : IFluentInterface
    {
    }
}
