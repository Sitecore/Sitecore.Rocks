# Plugin Architecture

Sitecore Rocks uses type discovery to support extensibility. This is heavily inspired by Microsoft MEF.

When Sitecore Rocks starts, it scans predefined folders for assemblies. Each assembly is loaded and each type in the assembly is examined.

If the type is marked with an Extensibility attribute, the type is processed as part of the extensibility framework. Examples of extensibility attributes are ```Command```, ```Pipeline```, ```Skin``` and ```FieldControl```.

All extensibility attributes inherit from the ExtensibilityAttribute class which has 3 methods: ```PreInitialize```, ```Initialize``` and ```PostInitialize```. 

When all assemblies have been loaded and the extensibility types found, Sitecore Rocks iterates through all types and calls first all the PreInitialize methods, then all the Initialize methods, and last all the PostInitialize methods.

For instance the ```CommandAttribute``` class overwrites the Initialize method and calls the ```CommandManager.LoadType``` method to register the command with the command manager.

The CommandManager is also marked with the ```ExtensibilityInitialization``` attribute. The ```ExtensibilityInitialization``` invokes  methods in the type on PreInit, Init and PostInit.  The CommandManager uses the ExtensibilityInitialization to call the ```Clear``` method during PreInitialization, which ensures that the list of commands is empty when the commands start registering.
