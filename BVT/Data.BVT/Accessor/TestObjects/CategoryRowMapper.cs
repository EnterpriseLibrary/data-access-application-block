// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    public class CategoryRowMapper : IRowMapper<CategoryBySale>
    {
        #region IRowMapper<CategoryBySale> Members

        public CategoryBySale MapRow(IDataRecord row)
        {
            return new CategoryBySale()
            {
                ProductName = (string)row["ProductName"],
                TotalPurchase = (decimal)row["TotalPurchase"]
            };
        }

        #endregion
    }
}

