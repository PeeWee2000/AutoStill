create procedure dbo.InsertRunRecord
 @runrecordtype RunRecordType readonly
 as
 Begin
insert into RunRecords select * from @runrecordtype
 END