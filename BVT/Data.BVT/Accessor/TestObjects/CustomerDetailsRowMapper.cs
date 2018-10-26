// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects
{
    public class CustomerDetailsRowMapper : IRowMapper<CustomerDetails>
    {

        #region IRowMapper<CustomerDetails> Members

        public CustomerDetails MapRow(IDataRecord row)
        {
            return new CustomerDetails()
            {
                City = (string)row["City"],
                CompanyName = (string)row["CompanyName"],
                Country = (string)row["Country"],
                CustomerID = (string)row["CustomerID"]
            };
        }

        #endregion
    }
}

