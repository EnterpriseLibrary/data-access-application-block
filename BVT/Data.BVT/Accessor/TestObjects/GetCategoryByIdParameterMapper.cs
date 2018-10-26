// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    public class GetCategoryByIdParameterMapper : IParameterMapper
    {
        #region IParameterMapper Members

        public void AssignParameters(DbCommand command, object[] parameterValues)
        {
            command.Parameters.Add(new SqlParameter("@CategoryName", parameterValues[0]));
            command.Parameters.Add(new SqlParameter("@OrdYear", parameterValues[1]));
        }

        #endregion
    }
}

