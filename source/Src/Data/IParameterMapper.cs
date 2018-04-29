// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace EnterpriseLibrary.Data
{
    /// <summary>
    /// Interface used to interpret parameters passed to an <see cref="DataAccessor{TResult}.Execute(object[])"/> method
    /// and assign them to the <see cref="DbCommand"/> that will be executed.
    /// </summary>
    public interface IParameterMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterValues"></param>
        void AssignParameters(DbCommand command, object[] parameterValues);
    }
}
