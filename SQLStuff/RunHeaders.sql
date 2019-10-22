USE [StillStats]
GO

/****** Object:  Table [dbo].[RunHeaders]    Script Date: 10/22/2019 7:36:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RunHeaders](
	[rhID] [int] IDENTITY(1,1) NOT NULL,
	[rhDate] [date] NOT NULL,
	[rhStart] [datetime] NOT NULL,
	[rhEnd] [datetime] NOT NULL,
	[rhDuration] [time](7) NOT NULL,
	[rhComplete] [bit] NOT NULL,
	[rhUnits] [nchar](10) NOT NULL,
	[rhAvgPressure] [decimal](10, 2) NULL,
 CONSTRAINT [PK_RunHeaders] PRIMARY KEY CLUSTERED 
(
	[rhID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

