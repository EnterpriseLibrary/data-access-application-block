use "Northwind"


if exists (select * from sysobjects where id = object_id('dbo.GetTestData') and sysstat & 0xf = 4)
	drop procedure "dbo"."GetTestData"
	

if exists (select * from sysobjects where id = object_id('dbo.GetTest') and sysstat & 0xf = 4)
	drop procedure "dbo"."GetTest"
if exists (select * from sysobjects where id = object_id('dbo.Test') and sysstat & 0xf = 3)
	drop table "dbo"."Test"
	
if exists (select * from sysobjects where id = object_id('dbo.ErrorRaisingStoredProc') )
	drop procedure "dbo"."ErrorRaisingStoredProc"
	
if exists (select * from sysobjects where id = object_id('dbo.AddRegion') )
	drop procedure "dbo"."AddRegion"
	
if exists (select * from sysobjects where id = object_id('dbo.RemoveRegion') )
	drop procedure "dbo"."RemoveRegion"
GO

CREATE PROCEDURE [dbo].[RemoveRegion] @RegionID int
AS
delete from Region where RegionID=@RegionID

GO

CREATE PROCEDURE [dbo].[AddRegion] @NewRegionName nchar(50)

AS
Begin
Declare @maxRegion int
select  @maxRegion=max(RegionID) from Region
insert into Region(RegionID,RegionDescription) values(@maxRegion+1,@newRegionName)
End


GO

CREATE PROCEDURE [dbo].[ErrorRaisingStoredProc]
AS
RAISERROR ('Severe Error', 11,1)

GO




CREATE TABLE [dbo].[Test](
	[TestID] [int] IDENTITY(1,1) NOT NULL,
	[TestName] [nvarchar](40) NOT NULL,
	[TestDescription] [nvarchar](40) NULL,
	[BugsCreated] [int] null,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](20) NULL,
	[UpdatedBy] [nvarchar](20) NULL,
	[TestCaseResult] [nvarchar](10) NULL,	
 CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED 
(
	[TestID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE PROCEDURE [dbo].[GetTestData]
AS
SELECT [TestID]
      ,[TestName]
      ,[TestDescription]
      ,[BugsCreated]
      ,[CreatedDate]
      ,[UpdatedDate]
      ,[CreatedBy]
      ,[UpdatedBy]
      ,[TestCaseResult]
  FROM [Northwind].[dbo].[Test] order by TestID
  GO
  
CREATE PROCEDURE [dbo].[GetTest] @TestID INT
AS
SELECT [TestID]
      ,[TestName]
      ,[TestDescription]
      ,[BugsCreated]
      ,[CreatedDate]
      ,[UpdatedDate]
      ,[CreatedBy]
      ,[UpdatedBy]
      ,[TestCaseResult]
  FROM [Northwind].[dbo].[Test]
  WHERE TestID = @TestID
 GO

INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test1','Test1',1,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test2','Test2',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test3','Test3',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test4','Test4',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test5','Test5',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test6','Test6',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test7','Test7',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test8','Test8',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test9','Test9',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')
INSERT INTO [Northwind].[dbo].[Test]([TestName],[TestDescription],[BugsCreated],[CreatedDate],[UpdatedDate],[CreatedBy],[UpdatedBy],[TestCaseResult])VALUES('Test10','Test10',NULL,GETDATE(),null,'v-ravarm',NULL,'Pass')

GO



