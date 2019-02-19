insert into RunHeaders (rhDate, rhStart, rhEnd, rhDuration, rhComplete) values ('2/18/2019', '2/18/2019 9:09:31 PM', '2/18/2019 9:09:31 PM', '00:00:00', 1)

select * from RunHeaders

delete from RunHeaders

DBCC CHECKIDENT (RunHeaders, RESEED, 0)
