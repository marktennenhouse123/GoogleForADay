

/****** Object:  Table [dbo].[IndexedSites]    Script Date: 8/13/2017 11:57:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[IndexedSites](
	[IndexedSiteID] [int] IDENTITY(1,1) NOT NULL,
	[Domain] [varchar](300) NULL,
	[InitialPage] [varchar](300) NULL,
	[DateCreated] [datetime] NULL DEFAULT (getdate()),
PRIMARY KEY CLUSTERED 
(
	[IndexedSiteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


