CREATE TYPE [dbo].RunRecordType AS TABLE(
	rrRHID int not null,
    rrTime datetime not null,
	rrTemp decimal(10,2) not null,
	rrTempDelta decimal(10,2) not null,
	rrPressure decimal(10,2) null,
	rrPhase int null,
	rrAmperage decimal(10,2) null
)

--drop procedure dbo.InsertRunRecord
--drop type dbo.runrecordtype
--ALTER TABLE runrecords DROP COLUMN rrID
--ALTER TABLE runrecords ADD rrID INT IDENTITY(1,1)