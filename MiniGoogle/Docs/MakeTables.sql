Create table IndexedPages
(
PageID int identity not null primary key,
PageName varchar(50) not  null default 'Index',
PageURL varchar(400) not null,
ParentID int not null,
DateCreated Datetime not null default GetDate(),
ParentDirectory varchar(300) default ''
)
