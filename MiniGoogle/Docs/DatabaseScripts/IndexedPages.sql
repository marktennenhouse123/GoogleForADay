

/****** Object:  Table [dbo].[IndexedPages]    Script Date: 8/13/2017 11:54:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO
Drop table Indexedpages

CREATE TABLE [dbo].[IndexedPages](
	[PageID] [int] IDENTITY(1,1) NOT NULL,
	[PageName] [varchar](250) NULL,
	[PageURL] [varchar](400) NULL,
	[ParentID] [int] NULL,
	[DateCreated] [datetime] NOT NULL DEFAULT (getdate()),
	[ParentDirectory] [varchar](300) NULL,
	[IsIndexed] [bit] NULL DEFAULT ((0)),
	[Title] [nvarchar](50) NULL,
	[IndexedSiteID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[PageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[IndexedPages]  WITH CHECK ADD FOREIGN KEY([IndexedSiteID])
REFERENCES [dbo].[IndexedSites] ([IndexedSiteID])
GO



