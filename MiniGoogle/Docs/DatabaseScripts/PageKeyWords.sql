

/****** Object:  Table [dbo].[PageKeyWords]    Script Date: 8/13/2017 11:56:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PageKeyWords](
	[PageKeywordID] [int] IDENTITY(1,1) NOT NULL,
	[PageID] [int] NULL,
	[Keyword] [varchar](50) NULL,
	[KeywordCount] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[PageKeywordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[PageKeyWords]  WITH CHECK ADD FOREIGN KEY([PageID])
REFERENCES [dbo].[IndexedPages] ([PageID])
GO


