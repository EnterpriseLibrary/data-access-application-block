// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor
{
    public class TopTenProductResultSet : IResultSetMapper<TopTenProduct>
    {
        #region IResultSetMapper<TopTenProduct> Members

        public IEnumerable<TopTenProduct> MapSet(IDataReader reader)
        {
            int i = 0;
            while (reader.Read() && i < 2)
            {
                yield return new TopTenProduct()
                {
                    TenMostExpensiveProducts = (string)reader["TenMostExpensiveProducts"],
                    UnitPrice = (decimal)reader["UnitPrice"]
                };

                i++;
            }
        }

        #endregion
    }
}

