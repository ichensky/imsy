create index `IX_ImageCategory_ImageCategory` on `ImageCategory` (
	`CategoryId`,	
	`Score`,		
	`ImageId`
);
create index `IX_ImageStr_ImageStr` on `ImageStr` (
	`StrId`,
	`Score`,
	`ImageId`	
);
create index `IX_Image_SeoId` on `Image` (
	`SeoId`
);



create index `IX_ImageDescriptionCaption_ImageDescriptionCaption` on `ImageDescriptionCaption` (
	`ImageId`,
	`DescriptionCaptionId`
);
create index `IX_Category_Category` on `Category` (
	`Name`
);
create index `IX_Image_MSVisionIsBWImg` on `Image` (
	`MSVisionIsBWImg`
);

create index `IX_Str_Str` on `Str` (
	`Name`
);


