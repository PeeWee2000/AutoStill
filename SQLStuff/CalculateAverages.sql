USE [StillStats]
GO
/****** Object:  StoredProcedure [dbo].[CalculateAverages]    Script Date: 10/22/2019 6:38:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[CalculateAverages] 

AS
BEGIN
declare @rhid int
declare @avgpressure decimal(10,4)

declare avgpressures cursor for
select distinct rhid from RunHeaders where rhAvgPressure is  null
open avgpressures
fetch next from avgpressures into  @rhid

WHILE @@FETCH_STATUS = 0
BEGIN

set @avgpressure = (select (sum(rrpressure) / count(rrpressure)) from RunRecords  where rrRHID = @rhid)

 if @avgpressure is not null
	update RunHeaders set rhAvgPressure = @avgpressure where rhID = @rhid

FETCH NEXT FROM avgpressures into @rhid
END
CLOSE avgpressures
DEALLOCATE avgpressures

END
