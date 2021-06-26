// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Practices.EnterpriseLibrary.Data
{
    /// <summary>
    /// Used with the Database.UpdateDataSet method. Provides control over behavior when the Data
    /// Adapter's update command encounters an error.
    /// </summary>
    public enum UpdateBehavior
    {
        /// <summary>
        /// No interference with the DataAdapter's Update command. If Update encounters
        /// an error, the update stops. Additional rows in the Datatable are unaffected.
        /// </summary>
        Standard,
        /// <summary>
        /// If the DataAdapter's Update command encounters an error, the update will
        /// continue. The Update command will try to update the remaining rows. 
        /// </summary>
        Continue,
        /// <summary>
        /// If the DataAdapter encounters an error, all updated rows will be rolled back.
        /// </summary>
        Transactional
    }
}
