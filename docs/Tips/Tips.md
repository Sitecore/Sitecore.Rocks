# Tips

## Sitecore Explorer

* You can edit the fields of an item directly in the Sitecore Explorer without opening the Content Editor. Click the Quick View button to open a pane at the bottom of the Sitecore Explorer. This pane shows the raw values of the item, and they can be edited. You can specify additional options by right-clicking inside the pane.
* You can narrow the scope of the Sitecore Explorer. For instance if you have multiple websites, but currently only work with one website, you can scope the tree view to show that website. Right-click the connection and select Scope to This.
* You can hide the "my items" virtual item in the tree view by opening the Turn Features On or Off dialog from the main menu and the uncheck the Sitecore Explorer My Items checkbox.
* You can hide the web site file folders Web Site and Data Folder in the tree view by opening the Turn Features On or Off dialog from the main menu and the uncheck the Sitecore Explorer Files checkbox.
* If you have selected an item in the Master database and want to find the same item in the Web database, you can select Navigate | Same Item in Other Database | Web.
* You can set the SortOrder field to blank for a number of items by selected all the items and select Tasks | Sorting | Reset Sort Order.
* You can update the SortOrder field in subitems of an items, by right-clicking the item and selecting Tasks | Sorting | Renumber Subitems. The will assign a number in leaps of 100 to each subitem in the current order. This is useful for spreading out sortorder numbers.
* You can "paint" field values between items - think Format Painter in Microsoft Word. Select an item and open Tasks | Field Painter. This open a dialog which allows you to select the fields to paint. When done, you can use the Tasks | Apply Field Painter.
* When deleting, you can bypass the Link Checking dialog by holding Shift down.
* You can change Modes in Commandy. You can right the dropdown arrow in to the right or you can type in a prefix and then space. You can see the available prefixes in the dropdown.
* You can see various properties - including the item ID - for the selected items by opening the Properties pane.
* You can favorite commands in the context menus by holding Shift+Control down while opening the context menu. Each command has a star next to it. Click the star to make it a favorite.
* You can assign keyboard shortcuts to commands in the context menus. Hold Shift+Control down while opening the context menu and click on a keyboard icon. 
* You can bypass the confirmation dialog while dragging and dropping by holding Shift down.
* You can reuse Item Editor tabs if they are not modified. Open the Options dialog from the main menu and enable Reuse Item Editor Window, if saved.
* You can enable the gutter which shows item warnings by right-clicking a website and selecting Tools | Gutters.
* You can open the expanded web.config by right-clicking a website and selecting Tools | Open Expanded web.config. The expanded web.config contains all include files.   
* You can switch the current language by opening a context menu and selecting Tools | Switch Language. This will change the current language globally.  

## Content Editor

* You can toogle between a small menu and a ribbon by right-clicking and selecting View | Ribbon.
* You can change how fields are displayed by selecting a view in the View context submenu. There are 5 views: Default, DB Browser, Tabbed, Two Column and Wide.
* You can show additional information about fields like Shared/Unversioned and if the field contains the standard value by enabling Field Information in the the View context submenu.
* You can click the label of a field to get commands that relate to that field.
* You can sort the fields of a template (including inherited fields) by selecting Tasks | Sort Template Fields.
* You can quickly find a field by using the Field Filter box in the top right corner.
* You can configure Rocks to reuse a Item Editor tab, if it is not modified by enabling the "Reuse Item Editor window, if saved" option in the Options. This will reduce the number of open tabs and allow you to use the Back and Forward buttons in the Item Editor.
* To edit field values directly, you can choose the Edit Externally command. This will launch an editor with the field value and automatically update the field value when the editor saves.
* You can hide a field when not showing Standard Fields by clicking its label and selecting "Hide in Non Standard Values View". The field will only be shown when viewing Standard Values.
* You can choose to show a Standard Field even if not viewing Standard Field by clicking its label and choosing "Always Show this Field".
* In a Rich Text field you can enable additional view options by right-clicking inside the field (not on the label) and selecting the View submenu: Indicator, Line Numbers, Outlining, Gutter, Word Wrap and Whitspace.
* In Image fields you can find other items that use this image by clicking the label and selecting "Image Links".
* In a Multilist field you can right-click the options to locate the item in the Sitecore Explorer.

## Layout Designer
* You can switch between a small menu and a full-blown Microsoft ribbon by selecting View | Enable Ribbon in the Context Menu.
* You can view the renderings in a list view or in a tree view. Switch between the view modes by selecting View | List View or Tree View.
* In the Tree View Property Editor you can directly open related files and items from the Resources section.
* When designing a SPEAK page in Tree View mode, it is possible to create property bindings by dragging the small square in the Property Editor to a rendering. It will show a small list of available properties to choose from. Think the XCode designer.
* Right-clicking a Placeholder in the Treeview mode will list the available renderings from the Placeholder Settings in the Add submenu.
* If you hold Ctrl down while selecting Locate in Sitecore Explorer in the Context Menu, the rendering will open in the Content Editor instead.
* You can select which columns to show in the List View mode by right-clicking the header row and choosing either Hide Column or Select Columns.
* Tou can addjust the width of the columns in the List View mode by right-clicking the header row and choosing either Size Column to Fit or Size all Columns to fit.
* You can copy the layout of one device to another by selecting Tasks | Copy to Other Device. 
* When adding renderings, you can setup filters. Click the Add button at the bottom of the filters list.
* You can press Ctrl+N to add a new rendering.
* You can see the properties of a rendering by double-clicking it. You can also open the Property Pane (in Visual Studio and Sitecore Rocks Windows) - the currently selected rendering will be tracked.
* You can multiselect renderings.
* If you assign a Datasource Location and a Datasource Template to the rendering, you can create data source items directly in the Layout Designer.
* If you assign a Parameters Template to a rendering, you will see the fields in the template as properties.
* You can drag and drop renderings from the Sitecore Explorer directly into the Layout Designer.
* You can generate SPEAK parameter templates in the Sitecore Explorer by right-clicking a View Rendering item and selecting SPEAK | Create Parameters Template. This will create a new template and assign the item ID to the Parameters Renderings field in the rendering.
* You can generate a SPEAK Model class for a rendering based on the Parameters Template by right-clicking a View Rendering item and selecting SPEAK | Create View Model.
* By filling out the Place Holders field on a rendering, you can specify which placeholders the rendering offers. Otherwise Sitecore Rocks will try to parse source file, which may not always be accurate.
* You can create new Rendering items from the Visual Studio Solution Explorer by right-clicking a .cs, .cshtml, .xml, .aspx or .ascx file and selecting Sitecore | Register... (remember to connect the VS project with a Sitecore website first).

## Query Analyzer

* The result grids are editable. You can click on a cell to edit the value. When you leave the cell, it is updated on the server.
* You can execute more than one statement in the same request. Separate the statements with a semi-colon. 
* If you highlight some of the text in the editor and click Execute, only the selected part is evaluated. This is useful when having more the one statement in the editor.
* You can export the result grids as Html, Csv and Xml. Right-click the grid and select the appropriate option.

