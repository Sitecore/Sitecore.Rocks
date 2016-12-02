# Query Analyzer

## Select statements

Simple Select statement
```
select * from /sitecore/content/Home
```

Select child items
```
select * from /sitecore/*
```

Select descendents
```
select * from /sitecore//*
```

Select descendents with filter
```
select * from /sitecore//*[@Title = "Welcome to Sitecore"];
```

Select columns
```
select @Title, @Text from /sitecore/content/Home
```

Select columns with aliases
```
select @Title as Caption, @Text as Body from /sitecore/content/Home
```

Select expression
```
select @Title + ", " + @Text as Value from /sitecore/content/Home
```

Select item ID
```
select @@ID from /sitecore/content/Home
```

Select item name
```
select @@Name from /sitecore/content/Home
```

Select item path
```
select @@Path from /sitecore/content/Home
```

Select item template
```
select @@TemplateID, @@TemplateName, @@TemplateKey from /sitecore/content/Home
```

Select with space escaping
If a field contains a space, the field name can be escaped using hashes #.

```
select @#Body Text# from /sitecore/content/Home
```

Select using Distinct
```
select distinct @Title from /sitecore/content/*
```

Select using Order By
Notice that the result set is order by column names, not field names.

```
select @@Name as Name from /sitecore/content//* order by Name
```

Select using Order By descendent
Notice that the result set is order by column names, not field names.

```
select @@Name as Name from /sitecore/content//* order by Name desc
```

Select using Distinct and Order By
```
select distinct Name as Name from /sitecore/content//* order by Name
```

List of used icons
```
select distinct @__Icon as Icon from //* order by Icon
```

List of items locked by a user
```
select * from //*[contains(@__Lock, 'owner="sitecore\admin"')]
```

## Delete statements

Delete an item
```
delete from /sitecore/content/Home
```

Delete children
```
delete from /sitecore/content/*
```

Delete descendents with a specific template
```
delete from /sitecore/content//*[@@templatename = 'News']
```

Delete items created by a specific user
```
delete from //*[@#__Created By# = 'sitecore\admin' or @#__Updated By# = 'sitecore\admin'];
```

Delete items that were updated recently
```
delete from /sitecore/content//*[@__Updated > '20111231T235959'];
```

Scary 
```
delete from /*
```

## Update statements

Update single field
```
update set 
  @Text = "Welcome to Sitecore"
from /sitecore/content/Home
```

Update multiple fields
```
update set 
  @Title = "Sitecore", 
  @Text = "Hello" 
from /sitecore/content/Home
```

Update multiple fields in multiple items
```
update set 
  @Title = "Sitecore", 
  @Text = "Hello" 
from /sitecore/content/*[@@TemplateName = "Sample Item"];
```

Reset Updated fields to Created fields
```
update set 
  @#__Updated By# = @#__Created By#, 
  @__Updated = @__Created, 
from /sitecore/content/Home
```

Expressions
```
update set 
  @Text = "Title: " + @Title
from /sitecore/content/Home
```

## Import keyword
The ```import``` keyword imports items/data into Sitecore.

The import retrieves items/data from a source. The source is a pluggable component.

The data is inserted at the current context node.

Syntax:
```import [source]```

Example:
```
set contextnode=/sitecore/content/Home;
import csv file "c:\import.csv";
```

By implementing various sources, the ```import``` keyword can import data in many ways.

### CSV import source
The CSV import source inserts comma separated values from either a server-side file or a string literal.

This following imports items from the file c:\import.csv
```import csv file "c:\import.csv"```

The following imports items from a string literal:

```
import csv
" 
<@DefaultTemplate=/Sample/Sample Item@>,
@ItemName, Title, Text
Home, Sitecore, "Welcome to Sitecore"
"
```

The format of the Csv data (file or string literal) consists of 3 parts:
Directives starts and ends with <@ and @>. This is used to specify the default template name, if none other is specified. This directive is optional.

The header line is the first line of data. It specifies the column names. The @ItemName column specifies the name of the item and the @TemplateName column specifies the template to use. The @TemplateName is optional, if the DefaultTemplate directive is used. If the template name does not start with "/sitecore/templates", it is prepended.

Data lines contain data. Each column is separated by a comma. Data containing commas can be enclosed in quotes (" or '). Use "" or '' to escape quotes.

Blank lines are ignored.

### Tree import source
The tree import source makes it possible to quickly build trees of data.

The source can either be a file or a string literal.

The items are create under the current context node.

Import a tree from a file:
```
set contextnode=/sitecore/content/Home;
import tree file "c:\tree.txt"
```

Import a tree from a string literal:
```
set contextnode=/sitecore/content/Home;
import tree
"
* Products|/Sample/SampleItem
** Product|/Sample/SampleItem|5
* News|/Sample/SampleItem
** 2011|/Sample/SampleItem
*** Month|/Sample/SampleItem|12
"
```

The above example creates the following tree structure:
```
+ Products
  | Product 1
  | Product 2
  | Product 3
  | Product 4
  | Product 5
+ News
  + 2011
    | Month 1
    | Month 2
    | Month 3
    | Month 4
    | Month 5
    | Month 6
    | Month 7
    | Month 8
    | Month 9
    | Month 10
    | Month 11
    | Month 12
```

The format of each line is:
* A number of stars (*) that indicate the level of the item.
* The item name.
* The template name. If the template does not start with "/sitecore/templates", it is prepended.
* The number of repeats (optional)
