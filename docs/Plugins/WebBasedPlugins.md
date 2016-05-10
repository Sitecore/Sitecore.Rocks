# Web Based Plugins

Sitecore Rocks is very extendable and has rich support for plugins.

Usually plugins are implemented in .NET assemblies which are installed into Sitecore Rocks. This requires knowledge of how to write applications using WPF and similar desktop technologies. This can be a steep learning curve and a different mindset for Sitecore Rocks developers who are usually Web Developers.

Sitecore Rocks also supports web based plugins which are implemented on the Sitecore server using the normal Sitecore API and development process. Basically they are just webpages which are defined in a manifest file that Sitecore Rocks finds and processes.

Here is an example of such a manifest file:

```
<?xml version="1.0" encoding="utf-8" ?>
<commands>
  <command>
    <context>Sites</context>
    <text>Security Editor</text>
    <sorting>1100</sorting>
    <group>WebCommands</group>
    <request>/en/sitecore/shell/Applications/Security/Security Editor.aspx</request>
    <window>Window</window>
  </command>
</commands>
```

This particular manifest defines the Security Editor command which opens in the Security Editor in a window.

## How it works
When Sitecore Rocks connects to a Sitecore website, it scans the /sitecore and /sitecore modules folders for files with the extension .sitecorerocks.xml. The contents of these files are returned to Sitecore Rocks client which processes the files and creates suitable commands.

When a user clicks such a command, Sitecore Rocks makes a request to the specified Url. The request can be showed in a window, dialog or with no UI.

After the request completes, Sitecore Rocks will execute any commands specified in the &lt;afterexecute> section.

## How to create a manifest file
Simply create a file with the extension .sitecorerocks.xml and put it under either the /sitecore or /sitecore modules folder in the website. When Sitecore Rocks connects to the website, it will find the file and parse it.

There are no requirements on the Url.

## Manifest file
A manifest file many contain any number of commands or submenus.

### Command section

Element | Description
--- | --- 
context | The context determines where the command is available. This is a Sitecore Rocks `IContext` type name like `Sitecore.VisualStudio.ContentEditors.ContentEditorContext` or an alias: Items, Daatbases, Sites, SitecoreExplorer or ItemEditor.
text | The display name. This is the text that is shown in the context menu.
sorting | An integer that defines the sorting order.
submenu | The name of the submenu. 
group | A group name. Commands are ordered in groups. Groups are delimited in the context menu by a separator.
request | The request Url. The Url may contain macros - see later.
window | Determines how the request is show. It can be one of the following values: Dialog, Window or empty. If the value is Dialog, it is possible to specify the attributes width, height and title to control the appearance of the dialog.
afterexecute | Specifies a list of command to execute after the request comples. This does not apply of the window is set to Window.

### Submenu section

Element | Description
--- | --- 
context | The context determines where the command is available. This is a Sitecore Rocks `IContext` type name like `Sitecore.VisualStudio.ContentEditors.ContentEditorContext` or an alias: Items, Daatbases, Sites, SitecoreExplorer or ItemEditor.
text | The display name. This is the text that is shown in the context menu.
sorting | An integer that defines the sorting order.
submenu | The name of the submenu. 
group | A group name. Commands are ordered in groups. Groups are delimited in the context menu by a separator.

### Request macros

Macro | Description
--- | ---
$sitename | The name of the current site.
$databasename | The name of the current database.
$itemid | The Guid of the current item.
$items | A pipe-separated list of Guid representing the currently selected items.
$languagename | The name of the current language.
$username | The name of the current user.
$password  | Well, this is pretty obvious.    

Here is an example of how to use macros:

```
    <request>/sitecore/shell/Applications/Dialogs/CloneItem.aspx?fo=$itemid&amp;sc_content=$databasename&amp;la=$languagename</request>
```

### afterexecute section
The afterexecute section is processed after the request completes. Please notice that the section is not supported when the window is set to Window, since the Window never really completes.

The section consist of a list of Sitecore Rocks command type names. Here is an example: 

```
<commands>
  <command>
     ...
    <afterexecute>
      <command type="Sitecore.VisualStudio.ContentTrees.Commands.Editing.EditItems" />
    </afterexecute>
  </command>
</commands>
```

This example opens the Item Editor on the current item.

