// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// Interface used to interpret parameters passed to an <see cref="DataAccessor{TResult}.Execute(object[])"/> method
    /// and assign them to the <see cref="DbCommand"/> that will be executed.
    /// </summary>
    public interface IParameterMapper
    {
        /// <summary>
        /// Assigns the values from <paramref name="parameterValues"/> to the <paramref name="command"/>'s Parameters list.
        /// </summary>
        /// <param name="command">The command the parameter values will be assigned to</param>
        /// <param name="parameterValues">The parameter values that will be assigned to the command.</param>
        void AssignParameters(DbCommand command, object[] parameterValues);
    }
}
