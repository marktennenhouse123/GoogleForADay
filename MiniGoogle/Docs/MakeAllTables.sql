create table IndexedPages
(
PageID	int not null identity Primary key,
PageName	varchar	(250),
PageURL	varchar	(400),
ParentID	int	NULL,
DateCreated	datetime not null default getdate(),
ParentDirectory	varchar	(300),
IsIndexed	bit	default 0,
Title	nvarchar(50)
)

Create Table PageKeyWords
(
PageKeywordID int identity primary key,
PageID int Foreign key references Indexedpages(PageID),
Keyword varchar(50),
KeywordCount int
)

Create Table AppLog
(

MessageID	int	not null identity primary Key,
MessageText		nvarchar(800),
FullMessage		nvarchar(1500),
FunctionName	varchar(100),
PageName	varchar (200),
AppName	varchar(50),
DateCreated		datetime not null default	getdate(),
EntityErrors	nvarchar(1000),
ObjectData	nvarchar(2000)
)

