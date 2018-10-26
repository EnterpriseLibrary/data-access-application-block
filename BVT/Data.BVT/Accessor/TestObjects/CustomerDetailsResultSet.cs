// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects
{
    public class CustomerDetailsResultSet : IResultSetMapper<CustomerDetails>
    {
        #region IResultSetMapper<CustomerDetails> Members

        public IEnumerable<CustomerDetails> MapSet(IDataReader reader)
        {
            int i = 0;
            while (reader.Read() && i < 2)
            {
                yield return new CustomerDetails()
                {
                    City = (string)reader["City"],
                    CompanyName = (string)reader["CompanyName"],
                    Country = (string)reader["Country"],
                    CustomerID = (string)reader["CustomerID"]
                };

                i++;
            }
        }

        #endregion
    }
}

