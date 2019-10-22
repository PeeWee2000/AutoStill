USE [StillStats]
GO

/****** Object:  Table [dbo].[RunRecords]    Script Date: 10/22/2019 7:36:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RunRecords](
	[rrID] [int] IDENTITY(1,1) NOT NULL,
	[rrRHID] [int] NOT NULL,
	[rrTime] [datetime] NOT NULL,
	[rrTemp] [decimal](10, 2) NOT NULL,
	[rrTempDelta] [decimal](10, 2) NOT NULL,
	[rrPressure] [decimal](10, 2) NOT NULL,
	[rrPhase] [int] NOT NULL,
	[rrAmperage] [decimal](10, 2) NULL,
	[rrRefluxTemperature] [decimal](10, 2) NULL,
	[rrCondensorTemperature] [decimal](10, 2) NULL,
 CONSTRAINT [PK_RunRecords] PRIMARY KEY CLUSTERED 
(
	[rrID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RunRecords]  WITH CHECK ADD  CONSTRAINT [FK_RunRecords_RunHeaders] FOREIGN KEY([rrRHID])
REFERENCES [dbo].[RunHeaders] ([rhID])
GO

ALTER TABLE [dbo].[RunRecords] CHECK CONSTRAINT [FK_RunRecords_RunHeaders]
GO

