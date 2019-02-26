 USE [StillStats]
GO
/****** Object:  StoredProcedure [dbo].[InsertRunRecord]    Script Date: 2/19/2019 11:00:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[InsertRunRecord]
 @runrecordtype RunRecordType readonly,
 @RHID int
 as
 Begin
insert into RunRecords
(rrRHID, rrTime, rrTemp, rrTempDelta, rrPressure, rrPhase, rrAmperage )
 
 select @RHID as rrRHID, rrTime, rrTemp, rrTempDelta, rrPressure, rrPhase, rrAmperage from @runrecordtype
 END
