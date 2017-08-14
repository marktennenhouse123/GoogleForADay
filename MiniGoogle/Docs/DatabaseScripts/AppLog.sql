USE [CloudDB]
GO

/****** Object:  Table [dbo].[AppLog]    Script Date: 8/14/2017 12:04:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO
drop table AppLog
CREATE TABLE [dbo].[AppLog](
	[MessageID] [int] IDENTITY(1,1) NOT NULL,
	[MessageText] [nvarchar](800) NULL,
	[FullMessage] [nvarchar](1500) NULL,
	[FunctionName] [varchar](100) NULL,
	[PageName] [varchar](200) NULL,
	[AppName] [varchar](50) NULL,
	[DateCreated] [datetime] NOT NULL DEFAULT (getdate()),
	[EntityErrors] [nvarchar](1000) NULL,
	[ObjectData] [nvarchar](2000) NULL,
PRIMARY KEY CLUSTERED 
(
	[MessageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


