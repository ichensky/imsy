create table `PixelUser` (
	`PixelUserId` blob not null primary key unique,
	`NickName` text not null unique,
	`UserName` text
);

create table `PixelImage` (
	`PixelImageId` blob not null primary key unique,
	`PixelUserId` blob null,
	`Url` text not null unique,
	`MetaUrl` text not null unique,
	`Title` text,
	`Keywords` text,
	`GoogleId` text unique,
	`Width` integer,
	`Height` integer,
	`GrayscalePerceptualHash` text,
	`ProcessState` integer null,
	`MSVision` integer null,
	foreign key (`PixelUserId`) references `PixelUser` (`PixelUserId`)    
);
create unique index `IX_PixelImage_PixelImageId_PixelUserId` on `PixelImage` (
	`PixelImageId`,
	`PixelUserId`
);
