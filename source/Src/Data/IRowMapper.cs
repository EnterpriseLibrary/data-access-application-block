// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace EnterpriseLibrary.Data
{
    /// <summary>
    /// Represents the operation of mapping a <see cref="IDataRecord"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type this row mapper will be mapping to.</typeparam>
    /// <seealso cref="ReflectionRowMapper&lt;TResult&gt;"/>
    public interface IRowMapper<TResult>
    {
        /// <summary>
        /// When implemented by a class, returns a new <typeparamref name="TResult"/> based on <paramref name="row"/>.
        /// </summary>
        /// <param name="row">The <see cref="IDataRecord"/> to map.</param>
        /// <returns>The instance of <typeparamref name="TResult"/> that is based on <paramref name="row"/>.</returns>
        TResult MapRow(IDataRecord row);

    }
}
