// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Design;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;

#if !NET10_0
[assembly: ReliabilityContract(Consistency.WillNotCorruptState, Cer.None)]
#endif
[assembly: SecurityTransparent]

[assembly: ComVisible(false)]

[assembly: HandlesSection(DataAccessDesignTime.ConnectionStringSettingsSectionName)]
[assembly: HandlesSection(DatabaseSettings.SectionName, ClearOnly = true)]

[assembly: AddApplicationBlockCommand(
            DataAccessDesignTime.ConnectionStringSettingsSectionName,
            typeof(ConnectionStringsSection),
            TitleResourceName = "AddDataSettings",
            TitleResourceType = typeof(DesignResources),
            CommandModelTypeName = DataAccessDesignTime.CommandTypeNames.AddDataAccessBlockCommand)]
