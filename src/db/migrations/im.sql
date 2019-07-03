create table `Category` (
	`Id` integer not null primary key autoincrement,
	`Name` text not null,
	`TopScoreMin` real not null
);
create table `ImageCategory` (
	`Id` integer not null primary key autoincrement,
	`CategoryId` integer not null,
	`ImageId` integer not null,
	`Score` real null,
	foreign key(`CategoryId`) references `Category`(`Id`),
	foreign key(`ImageId`) references `Image`(`Id`)
);
create table `DescriptionCaption` (
	`Id` integer not null primary key autoincrement,
	`Name` text not null
);
create table `ImageDescriptionCaption` (
	`Id` integer not null primary key autoincrement,
	`DescriptionCaptionId` integer not null,
	`ImageId` integer not null,
	`Score` real not null,
	foreign key(`DescriptionCaptionId`) references `DescriptionCaption`(`Id`),
	foreign key(`ImageId`) references `Image`(`Id`)
);
create table `Str` (
	`Id` integer not null primary key autoincrement,
	`Name` text not null,
	`TopScoreMin` real null,
	`TopScoreMinMin` real null
);
create table `ImageStr` (
	`Id` integer not null primary key autoincrement,
	`StrId` integer not null,
	`ImageId` integer not null,
	`IsTag` integer not null,
	`IsDescriptionTag` integer not null,
	`IsDominantColor` integer not null,
	`IsDominantColorBackground` integer not null,
	`IsDominantColorForeground` integer not null,
	`Score` real null,
	foreign key(`StrId`) references `Str`(`Id`),
	foreign key(`ImageId`) references `Image`(`Id`)
);
create table `Image` (
	`Id` integer not null primary key autoincrement,
	`GoogleDriveId` text not null,
	`Title` text not null,
	`SeoId` text not null,
	`GoogleDriveWidth` integer not null,
	`GoogleDriveHeight` integer not null,
	`MSVisionIsBWImg` integer not null,
	`MSVisionAccentColor` text null
);
