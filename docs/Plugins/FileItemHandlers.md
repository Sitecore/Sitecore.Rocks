#File/Item Handlers

File/Item handlers manage the relationship between items and files, for instance a layout item and an .aspx
file.

When a developer uses the Add New Item dialog to create a layout file the wizard looks at the extension of 
the file (.aspx) and invokes the matching File/Item handler.

File/Item handlers create a corresponding Sitecore item and maintain the path field in the created item when 
the file is moved.

File/Item handlers must be marked with the IFileItemHandler attribute and inherit from the 
FileItemHandler class.

The FileItemHandler class defines the following members:

Member | Description
--- | ---
TemplateName | The name of the template to create.
Create | Creates the item.
GetItemPath | Get the Sitecore path of the item to create.
NormalizeItemPath | Normalizes the Sitecore Path, e.g. converts \ to /.
SetPath | Sets the path in the Sitecore Item. Is called when a file is moved.

Most File/Item handlers inherit from the PathBasedFileItemHandler class which provides support for 
path based items, like layouts, sublayouts and Xslt renderings