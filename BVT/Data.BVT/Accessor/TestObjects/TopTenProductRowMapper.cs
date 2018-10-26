// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    public class TopTenProductRowMapper : IRowMapper<TopTenProduct>
    {
        #region IRowMapper<TopTenProduct> Members

        public TopTenProduct MapRow(IDataRecord row)
        {
            return new TopTenProduct()
            {
                TenMostExpensiveProducts = (string)row["TenMostExpensiveProducts"],
                UnitPrice = (decimal)row["UnitPrice"]
            };
        }

        #endregion
    }
}

