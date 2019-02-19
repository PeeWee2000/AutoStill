CREATE TYPE [dbo].RunRecordType AS TABLE(
    rrid int NOT NULL,
	rrRHID int not null,
    rrTime datetime not null,
	rrTemp decimal(10,2) not null,
	rrTempDelta decimal(10,2) not null,
	rrPressure decimal(10,2) null,
	rrPhase int not null,
	rrAmperage decimal(10,2) null
)

--drop procedure dbo.InsertRunRecord
--drop type dbo.runrecordtype