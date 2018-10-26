// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    //This class is defined as private in the SProcAcessor.cs
    public class CustomParameterMapper : IParameterMapper
    {
        readonly Database database;
        public CustomParameterMapper(Database database)
        {
            this.database = database;
        }

        public void AssignParameters(DbCommand command, object[] parameterValues)
        {
            if (parameterValues.Length > 0)
            {
                GuardParameterDiscoverySupported();
                database.AssignParameters(command, parameterValues);
            }
        }

        private void GuardParameterDiscoverySupported()
        {
            if (!database.SupportsParemeterDiscovery)
            {
                //throw new InvalidOperationException(
                //    string.Format(Resources.Culture,
                //                  Resources.ExceptionParameterDiscoveryNotSupported,
                //                  database.GetType().FullName));
            }
        }
    }
}

