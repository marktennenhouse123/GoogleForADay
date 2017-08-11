

CREATE TABLE [dbo].[AppLog](
	[MessageID] [int] IDENTITY(1,1) NOT NULL,
	[MessageText] [nvarchar](800) NULL,
	[FullMessage] [nvarchar](1500) NULL,
	[FunctionName] [varchar](100) NULL,
	[PageName] [varchar](200) NULL,
	[AppName] [varchar](50) NULL,
	[DateCreated] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[MessageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

ALTER TABLE [dbo].[AppLog] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO


