// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace EnterpriseLibrary.Data.TestSupport
{
    public abstract class StoredProcedureCreationBase
    {
        protected Database db;
        protected StoredProcedureCreatingFixture baseFixture;

        protected abstract void CreateStoredProcedure();
        protected abstract void DeleteStoredProcedure();

        protected void CompleteSetup(Database db)
        {
            this.db = db;
            baseFixture = new StoredProcedureCreatingFixture(db);

            Database.ClearParameterCache();
            CreateStoredProcedure();
        }

        protected void Cleanup()
        {
            DeleteStoredProcedure();
            Database.ClearParameterCache();
        }
    }
}
