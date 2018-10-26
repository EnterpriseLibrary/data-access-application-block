// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects
{
    public class NotInheritedPerson
    {
        public int CustomerID { get; set; }
        public bool IsEmployee { get; set; }
        public string CompanyName { get; set; }
        public DateTime BirthDate { get; set; }
    }
}

