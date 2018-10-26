// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.Data.TestSupport
{
    public abstract class UpdateDataSetBehaviorsFixture : UpdateDataSetStoredProcedureBase
    {
        protected override void AddTestData() { }

        protected override DataSet GetDataSetFromTable()
        {
            DbCommand selectCommand = db.GetStoredProcCommand("RegionSelect");
            return db.ExecuteDataSet(selectCommand);
        }

        protected override void PrepareDatabaseSetup() { }

        protected override void ResetDatabase()
        {
            RestoreRegionTable();
        }

        void RestoreRegionTable()
        {
            string sql = "delete from Region where RegionID >= 99";
            DbCommand cleanupCommand = db.GetSqlStringCommand(sql);
            db.ExecuteNonQuery(cleanupCommand);
        }

        public void UpdateWithStandardBehavior()
        {
            AddRowsToDataTable(startingData.Tables[0]);

            db.UpdateDataSet(startingData, "Table", insertCommand, updateCommand, deleteCommand,
                             UpdateBehavior.Standard);

            DataSet resultDataSet = GetDataSetFromTable();
            DataTable resultTable = resultDataSet.Tables[0];

            Assert.AreEqual(8, resultTable.Rows.Count);
            Assert.AreEqual(502, Convert.ToInt32(resultTable.Rows[6]["RegionID"]));
            Assert.AreEqual("Washington", resultTable.Rows[6]["RegionDescription"].ToString().Trim());
        }

        public void UpdateWithTransactionalBehavior()
        {
            AddRowsToDataTable(startingData.Tables[0]);

            db.UpdateDataSet(startingData, "Table", insertCommand, updateCommand, deleteCommand,
                             UpdateBehavior.Transactional);

            DataSet resultDataSet = GetDataSetFromTable();
            DataTable resultTable = resultDataSet.Tables[0];

            Assert.AreEqual(8, resultTable.Rows.Count);
            Assert.AreEqual(502, Convert.ToInt32(resultTable.Rows[6]["RegionID"]));
            Assert.AreEqual("Washington", resultTable.Rows[6]["RegionDescription"].ToString().Trim());
        }
    }
}
