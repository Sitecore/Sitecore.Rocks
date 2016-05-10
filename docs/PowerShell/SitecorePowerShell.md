#Sitecore PowerShell

PowerShell support for Sitecore can automate a number of daily, repetitive tasks for administrators and developers and it provides an advanced scripting interface to Sitecore.

The initial version of Sitecore.PowerShell supports basic CRUD operations (navigation, create item, set field value, delete item, copy an item to a file and back etc.) through the PowerShell Provider interface and a number of Commandlets: 

* Clear-SCArchive
* Clear-SCCache
* Clear-SCCaches
* Export-SCPackage
* Clear-SCPublishingQueue
* Compress-SCPublishingQueue
* Publish-SCDatabase
* Publish-SCItem
* Compress-SCIndex
* Search-SCItems
* Update-SCIndex
* Update-SCSearchIndex
* Update-SCLinkDatabase

Sitecore PowerShell supports the old web service (which has been available since Sitecore v5) and the new Sitecore Rocks web service. 

Sample:
```
# Map Rocks web service drive
new-psdrive -name rocks   -psp SitecoreRocks   -root "" -host "http://localhost" -usr "sitecore\admin" -pwd "b" -databasename "master";
Function rocks: { Set-Location rocks:\ }

# Map Good Old web service drive
new-psdrive -name goodold -psp SitecoreGoodOld -root "" -host "http://localhost" -usr "sitecore\admin" -pwd "b" -databasename "master";
Function goodold: { Set-Location goodold:\ }

rocks:;

cd \sitecore\content\Home;

# Create a new item
new-item . -type 'Sample\Sample Item' -Value MyItem;

# Set and get fields
set-itemproperty MyItem -name Title -value "It works!";
get-itemproperty MyItem -name Title;

# Item CRUD
Rename-Item MyItem YourItem;

Copy-Item YourItem OurItem;

Move-Item OurItem YourItem;

Remove-Item YourItem\OurItem;

# Copy item Xml to a file and back
get-content YourItem > c:\item.xml
get-content c:\item.xml | set-content YourItem

# Delete children
remove-scchildren;

# Update an index
Update-SCIndex master;

# Update link database
Update-SCLinkDatabase;

# Update search index
Update-SCSearchIndex;
```