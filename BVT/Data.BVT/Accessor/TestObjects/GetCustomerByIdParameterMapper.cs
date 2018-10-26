// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects
{
    public class GetCustomerByIdParameterMapper : IParameterMapper
    {
        #region IParameterMapper Members

        public void AssignParameters(DbCommand command, object[] parameterValues)
        {
            command.Parameters.Add(new SqlParameter("ID", parameterValues[0]));
        }

        #endregion
    }
}

