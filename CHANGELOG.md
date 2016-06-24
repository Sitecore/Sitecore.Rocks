Version 2.0.0-beta1
===================
* Add: Added Ribbon to Template Designer (2016-01-08)
* Fix: Template Field Sorted is not theme correctly (2016-01-08)
* Add: Added Sort Template Fields to context menus (2016-01-08)
* Fix: Duplicating menu items when hover over Include in Library (2016-01-08) - Steve Jennings
* Add: Filter Commands in Management | Commands (2016-01-08) - Robbert Hock
* Fix: Duplicate Rendering in Layout Designer (2016-01-08) - Max Reimer-Nielsen
* Add: Added Create Standard Values and Base Templates to New Template Dialog (2016-01-17)
* Add: Added keyboard schemes (2017-01-25) - Richard Cabral
* Fix: Background on Office White icons (2017-01-26) - Robbert Hock
* Fix: Exception in Template Hierarchy (2017-03-02) - Saif Mazhar
* Removed Sitecore.Rocks.GridDesigner and Sitecore.Rocks.Server.GridDesigner
* Removed Sitecore.Rocks.Server.Cms7 assembly
* Removed Sitecore.Rocks.Server.Lucene.* assemblies
* Removed "Start PowerShell command"
* Fix: Version numbers in .vsix manifest (2016-06-24) - Catherine Guey Ruang Lim

Version 1.5.1
=============
* Fix: Upgraded to Visual Studio 2015 and new code style (2015-06-14)
* Add: Check for SPEAK Core Version on renderings in Layout Designer (2015-06-15)
* Add: Added Copy Rendering ID to Layout Designer (2015-06-16) - Robbert Hock
* Add: Added support for TriState fields in SPEAK Rendering Parameters (2015-06-17) 
* Add: Grid Designer for SPEAK (contributed by Andrea Bellagamba) (2015-06-26)
* Fix: MaxItems setting now carries over to subexpressions (2015-06-26) - Pradeep Shukla
* Fix: Added a new MVC view in VS 2015 no longer throws an exception (2015-08-11) - John Penfold
* Fix: Theme is now correctly detected in VS 2015 (2015-08-11)
* Fix: Better compatibility for VS 2015 (2015-08-14)
* Fix: Autogeneration of control IDs in Layout Designer is now better (2015-08-14) - Max Reimer-Nielsen
* Add: Support for param(...) in Task Lists (2015-08-26) - Rasmus Rasmussen

Version 1.5.0
=============
* Fix: Changed Item Editor labels to hyperlinks (2015-03-16)
* Fix: DropTree now shows [No item] with nothing is selected (2015-03-16)
* Add: It is now possible to switch database in the Select item dialog (2015-03-16)
* Add: Added Clipboard | Copy Icon Path (2015-03-16)
* Fix: Item Editor Field help texts are now always shown (2015-03-17)
* Add: Added UnexportAttribute for programatically removing extensiblity exports (2015-03-17) - Pierre Sapinault
* Add: Added @@shortid to Query Analyzer expressions (2015-03-20) - Pierre Sapinault
* Fix: When updating fields in the Query Analyzer grid, the language is now respected (2015-03-20) - Pierre Sapinault
* Fix: Now show an error message if an image field fails to load (2015-03-25) - Damani Musgrave
* Add: Added ribbon to Item Editor (2015-03-25)
* Fix: Image fields now work in CMS 8.0 rev 150223 (2015-03-25) - Damani Musgrave
* Fix: Changed the visual cue on image fields, if there are hidden values (2015-03-25) - Damani Musgrave
* Add: Set Base Templates command available from the Sitecore Explorer (2015-04-15) - Michelle White, Damani Musgrave
* Fix: Deleting a Standard Values item will now enable the Create Standard Values command without a refresh (2015-04-15) - Michelle White
* Fix: Context menu items for creating Standard Values are now in sync between Sitecore Explorer and Template Designer (2015-04-19) - Michael Robicheaux
* Add: Added support for DateTime/Date fields in parameter templates in Layout Designer (2015-04-10) - Andrea Bellagamba
* Add: Added support for custom editors in the Properties pane. Set the 'editor' on a field 'source' to point to a .NET class implementing UITypeEditor (2015-04-10) - Andrea Bellagamba
* Add: Set Allowed Renderings command on placeholders in Layout Designer (2015-04-10) - Robbert Hock
* Fix: Slight reorganization of the commands in context menus in the Item Editor (2015-04-10)
                                                                                                                  
Version 1.4.0
=============
* Fix: Layout Files does not save correctly (2015-02-05) - Lars Roed
* Fix: Layout Files Schema (IntelliSense) now ignores duplicate field names (2015-02-05)
* Add: File Pattern Masks for Folder Synchronization (2015-02-18) - Thomas Dedeyne
* Fix: Collapsed Link fields now show a visual cue, if any values are hidden (2015-02-18) - Carlos Rodriguez
* Fix: When designing a layout on standard values, the layout is saved to the correct item (2015-02-18) - Raul Jimenez
* Fix: Reinstated original Resolve using Local or Resolve using Website to resolve conflicts when deploying
       files to websites - however it is cumbersome/bad workflow - not sure it is a good idea.
       Somebody should write a plugin (2015-02-23)
* Fix: Reinstated original Update Local Files from Website - same comment as above (2015-02-23)
* Fix: Renamed Deploy Marked Files to Deploy Local Files to Website (2015-02-23)
* Add: Commandy can now be dragged to a different position. The position is not persisted across sessions (2015-02-23)
* Add: Added rendering icons to Layout Designer (2015-02-26)
* Add: Added rendering icons to Select Rendering dialog (2015-02-26)
* Add: Added Ctrl+N keyboard shortcut for added new renderings to Layout Designer (2015-02-26)
* Add: Select columns to show in Layout Designer (right-click the grid header) (2015-03-02)
* Add: Changed Folders pane into Libraries in the Sitecore Explorer (think Windows Explorer Libraries) (2015-03-03)
* Add: Added the current template name to the Change Template dialog (2015-03-04) - Robbert Hock
* Add: Added Move Up and Down buttons to Set Insert Options dialog (2015-03-04) - Robbert Hock
* Add: Added help texts to various dialog boxes (2015-03-04)
* Add: Added ribbon to Layout Designer (2015-03-05)
* Fix: SPEAK Component JavaScript new follows newest SPEAK standard (2015-03-05) - Andrea Bellagamba
* Fix: DropTree Content Editor field now supports the DatabaseName/Database parameter (2015-03-07)
* Fix: Folder Synchronization no longer copies all assemblies to the /bin folder - only the project output file (2015-03-09)
* Fix: TreeList fields now works even if data source is not set (2015-03-10)

Version 1.3.5
=============
* Fix: Page Editor renamed to Experience Editor for CMS 8 and above (2015-01-05) - Ariel McKnight
* Fix: Layout Designer Layout field now works in Japanese (2015-01-05)
* Add: Context menu is now available in the Set Base Template dialog (2015-01-05) - Robbert Hock
* Add: Sorting commands in Layout Designer (Move Up, Down, First, Last) (2015-01-05) - Robbert Hock
* Fix: Browsing for a data source in the Layout Designer now expands correctly (2015-01-06) - Qing Zhang
* Add: Upload Media commands on Media Folders, Attachments and Images (2015-01-13) - Ariel McKnight
* Fix: Images are now visible in Sitecore 8 (2015-01-13) - Ariel McKnight
* Rem: Removed recently used templates from the Add submenu (2015-01-13) - by popular demand
* Fix: Browse website and items now logs in correctly (2015-01-13)
* Add: Added a Test Connection button to Project Properties (2015-01-19) - Carlos Rodriguez
* Fix: Site elements in the .sitecore files are now wrapped in <PropertyGroup> elements for backwards compatibility (2015-01-19) - Jason Cox
* Fix: Droptree fields are now marked as modified when browsing (2015-01-21) - Carlos Rodriguez
* Fix: Image now load correctly in CMS 8, even when the session times out (2015-01-21) - Andrew Vieau
* Fix: Added horizontal scrollbars to various windows for supporting small screens (2015-01-20) - Andrew Vieau

Version 1.3.1
=============
* Fix: Edit Layout as File is visible again  (2014-12-16) - Qing Zhang
* Fix: Placeholder is set correctly when adding renderings in tree view mode in Layout Designer (2014-12-16) - Ivan Korshun
* Fix: Sitecore Nuget now finds the correct connection - (2014-12-18) - Kevin Obee
* Add: Filter to Select Icon dialog (2014-12-18) - Robbert Hock

Version 1.3.0
=============
* Fix: Enter now respects default action in Sitecore Explorer (2014-05-01)
* Add: Bind item to files by dragging files from Solution Explorer to Sitecore Explorer (2014-05-15)
* Fix: Refactored Projects to support multiple projects pointing to the same Sitecore web site (2014-05-15)
* Fix: Missing "using Sitecore;" was added to Template Classes code generation (2014-05-28) - @mrmathos 
* Fix: Site names are no longer evaluated case-sensitively (2014-06-13) - @mrmathos 
* Add: Icons in Content Editor sections including context menus (2014-06-13) - Robbert Hock
* Fix: Insert rendering at the currently selected position (2014-07-01) - Johannes Zijlstra                                                                   - 
* Fix: Add Placeholder button now works in Design Layout  (2014-07-04)
* Fix: Copying items now adds the postfix " - Copy" instead of the prefix "Copy of " 
       to be consistent with Visual Studio (2014-09-23) - Dave Morrison
* Fix: Copying an item only adds the postfix, it an item with the same name already exists (2014-09-23) - Dave Morrison
* Fix: Refreshing items in the Solution Explorer always expands the tree node (2014-09-23)
* Add: Recently used templates are now in the Add New Item drop down (2014-09-23)
* Add: Added new Sitecore 8 icons to the Select Icon dialog (2014-10-10) - Kerry Bellerose
* Add: Browse for Type and Assembly in fields (2014-10-21) - John West
* Add: Added new VS Item Templates for MVC Models, Controllers and layouts (2014-10-21) - John West
* Add: Sitecore Validation Runner for VS Build or CI (2014-10-23)
* Fix: Support for CMS 7 Rules Editor (2014-10-26)
* Fix: Support for rendering multivariate testing in Layout Designer (2014-10-27)
* Rem: Removed Conditional Renderings field from renderings in Layout Designer (2014-10-27)
* Add: Support for personalization in Layout Designer (2014-10-27)
* Fix: Heavily refactored ProjectItems to support multiple kinds of project items (2014-10-28)
* Fix: Support for UTC date/time format in CMS 7.5 (2014-10-28)
* Rem: Removed Command Cheatsheet (2014-10-28)
* Add: Support for CMS 7.2 Publish Related Items in Advanced Publish (2014-10-30)
* Fix: Removed some false positives from Validation/SitecoreCop (2014-10-30)
* Add: Folder Synchronization inspired by CopySauce and Project2Sitecore (2014-11-04)
* Fix: Support for ExcludeItemsForDisplay, ExcludeTemplatesForDisplay, ExcludeTemplatesForSelection,
       IncludeItemsForDisplay, IncludeTemplatesForDisplay and EncludeTemplatesForSelection 
       in TreeList field (2014-11-04) - Kevin Obee
* Add: Internal Link field (2014-11-05)
* Add: Custom filters on Select Items and Select Template dialogs (2014-11-05)
* Add: Support for Versioned Layouts (2014-11-05)
* Add: Rendering Chunks (2014-11-14)
* Change: Please notice that "Edit Layout as File" does not support Layout Deltas (2014-11-14)
* Add: Insert Rendering mode added to Commandy in Layout Designer (2014-11-14)
* Add: Name Lookup Value List field (2014-11-20) - Shriroop Parikh
* Fix: Boolean data bindable properties are now set to 1 or 0 instead of True and False (2014-12-01) - Sean Holmesby
* Fix: Show tooltip in Item Editor labels (2014-12-02) - Shriroop Parikh
                                                                                         
Version 1.2.6
=============
* Fix: Layout attribute in Layout Files (2014-04-07)
* Fix: Binding booleans in Layout Files (2014-04-08)
* Fix: Install Template Wizard makes Visual Studio hang (2014-04-15) - Mike Sprague
* Fix: Sitecore Rocks overwrites rendering parameters, if there are no parameters template (2014-04-15) - Thom Puiman
* Fix: Icons no longer disappear in the Sitecore Explorer (2014-04-20)
* Fix: Lots of fixes to the Layout Files schema (2014-04-23)
* Add: Added path information to Link fields (2014-04-23)
* Add: Toggle details on Link fields (2014-04-23)
* Add: Added path information to Image fields (2014-04-23)
* Add: Toggle details on Image fields (2014-04-23)
* Add: Browse for image on Image fields (2014-04-23)
* Fix: CheckBox field alignment (2014-03-23)
* Fix: Save Layout File schema file correct in non-standard installation of Visual Studio (2014-04-15)
* Add: Added Browse button to XPath Builder context node (2014-04-15) - Robbert Hock
* Fix: Switch Language now also switches the language on the server (2014-04-15) - Andrey Bobrov

Version 1.2.5
=============
* Add: Update Place Holder Field (2014-03-27)
* Add: Added help text to Layout File xsd schema (2014-04-02)
* Fix: Copy Template Wizard with elevated rights in Windows 8.1 (2014-04-02)
* Fix: Copy Layout File schema xsd with elevated rights in Windows 8.1 (2014-04-02)
* Fix: Reinstated Sort Template Field tool (2014-04-02) - Robbert Hock
* Add: Create PageCode TypeScript file (2014-04-03)
* Fix: Item Editor failed to open (2014-04-03) - Zicheng Dong
* Fix: Give a warning, if the developer is using Design Layout while the item is linked to a Layout File (2014-04-04)

Version 1.2.0
=============
* Add: Relink item tree (2013-12-17) 
* Fix: Server-side drop list properties in SPEAK Layout Designer can now be set correctly (2013-12-19) - Nick Wesselman
* Fix: Edit File in Content Editor now considers Visual Studio Project (2014-01-10)
* Fix: When adding a control with an ID in the Layout Designer, the ID is converted to a safe identifier (2014-01-10)
* Fix: Removed _ from connection string when installing Sitecore (2014-01-15) - Nick Hills
* Add: Create SPEAK TypeScript file (2014-01-17)
* Add: Import/Export SPEAK Documentation (2014-01-19)
* Add: Convert Backslashes to Slashes in Text Field (2014-01-30)
* Add: Html Encode Text field (2014-01-30)
* Add: Url Encode Text field (2014-01-30)
* Add: JavaScript Encode Text field (2014-01-30)
* Add: PropertyType parameters for SPEAK renderings (2014-01-31)
* Add: RulesPath data source parameter (2014-02-02) - Robbert Hock
* Add: HideActions data source parameter (2014-02-02) - Robbert Hock
* Fix: Add for Sitecore connection when using Add New Item in Visual Studio (2014-02-02)
* Fix: Set modified in Layout Designer, when using Context Menu to delete a rendering (2014-02-04) - Brandon Hubbard
* Fix: Refactored Plugins significantly to support Sitecore.Rocks.Contrib project (2014-02-18)
       Plugins in NuGet packages are now stored in the Packages folder instead of the Plugins folder.
* Fix: Edit Externally was completely broken (2014-02-19)
* Fix: Layout Designer in Dark Theme - no more unreadable text (2014-02-21)
* Fix: Changed sections in Content Editor to fit with themes (2013-02-21)
* Add: Create Plugin as NuGet Package in Plugins Manager (2014-02-26)
* Add: Edit SPEAK Layout as Xml (2014-03-01)
* Add: Upload and Install Package in one go in Package Manager (2014-03-01)
* Fix: Upgrade Actipro components to the latest version (2014-03-04)
* Fix: Removed SitecoreApplication API class to support IOC (2014-03-05)
* Fix: AppHost.OpenDocumentWindow now supports WinForms controls (2014-03-05) - Rasmus Rasmussen
* Add: Create Sitecore Rocks plugin VS projects from Start Page (2014-03-06)
* Fix: Upgraded Sitecore.Rocks.Resharper to Resharper 8.2 (2014-03-07)
* Add: Reset Layout to Standard Value in context menu Tasks (2014-03-07) - Raul Jimenez
* Add: Field Painter (2014-03-07) - Raul Jimenez
* Add: 10 new validations for SPEAK (2014-04-10) - Chris Bushnell and Henning Bertram
* Add: Infrastructure for support various versions of the CMS (2014-04-11)
* Add: Reduce UI bloat by turning Features on or off (2014-04-11)
* Add: Menu button in Sitecore Explorer, Item Editor, Search, Media Library, Layout Designer, Job Viewer, Log Viewer (2014-04-12)
* Fix: Updated some icons to fit better with VS theme (2014-04-12)
* Fix: Notification.TemplateChanged has wrong set of parameters (2014-04-12)
* Fix: Cleaned up duplicate menu items in Item Editor context menu (2014-04-12)
* Add: Set Base Template in Commandy (2014-04-21) - Jens Mikkelsen
* Add: Tasklist Rockify Website (2014-04-22)
* Add: Execute All button in Task Lists (2014-04-22)

Version 1.1.0
=============
* Add: Ability to disable IIS Integration (2013-12-04)
* Fix: Drag/drop in Design Layout (2013-12-05) - Alan Coates
* Fix: Multiple selection in Design Layout (2013-12-05)
* Fix: Multiple selection in Query Analyzer (2013-12-05)
* Add: Multiple selection in Query Analyzer (2013-12-05)
* Add: Create SPEAK Parameters template (2013-12-06)
* Fix: Shortcut keys for Design Layout / Design layout on Standard Values (2013-12-08) - Sean Holmesby
* Add: Set keyboard shortcuts (2013-12-08)