create table `MdsAuthor` (
	`Id` integer not null primary key autoincrement unique,
	`Name` text not null unique
);
create index `IX_MdsAuthor_Name` on `MdsAuthor` (
	`Name`
);

create table `MdsMusic` (
	`GoogleDriveId` text not null primary key unique,
	`MdsAuthorId` integer null,
	`Year` integer null,
	`Category` text null,
	`Title` text not null,
	`Length` integer not null,
	foreign key(`MdsAuthorId`) references `AuthorId`(`Id`)
);
create index `IX_MdsMusic_Title` on `MdsMusic` (
	`Title`
);
create index `IX_MdsMusic_Year` on `MdsMusic` (
	`Year`
);
create index `IX_MdsMusic_Category` on `MdsMusic` (
	`Year`
);
