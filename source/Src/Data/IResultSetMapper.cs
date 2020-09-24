// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// Represents the operation of mapping a <see cref="IDataReader"/> to an enumerable of <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The element type this result set mapper will be mapping to.</typeparam>
    public interface IResultSetMapper<TResult>
    {

        /// <summary>
        /// When implemented by a class, returns an enumerable of <typeparamref name="TResult"/> based on <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> to map.</param>
        /// <returns>The enumerable of <typeparamref name="TResult"/> that is based on <paramref name="reader"/>.</returns>
        IEnumerable<TResult> MapSet(IDataReader reader);
    
    }
}
