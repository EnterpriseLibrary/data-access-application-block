// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.Data.BVT.Accessor.TestObjects
{
    public class ClassWithIndexerProperty
    {
        private int[] numbers = new int[] { 1, 2, 3 };

        public int this[int index]
        {
            get { return numbers[index]; }
            set { numbers[index] = value; }
        }
    }
}

