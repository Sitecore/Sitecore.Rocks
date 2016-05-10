# Debugging a Visual Studio project

In the Visual Studio project properties, set the Start Action in the Debug tab to External Program and point the path to something like (Windows 7, 64-bit):

```C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe```

This will start a second instance of Visual Studio when you start debugging.
In the Visual Studio project properties, set the Output path of the assembly in the Build tab to something like:

```C:\Users\<You>\AppData\Local\Sitecore\Sitecore.Rocks\Plugins\<MyPlugin> ```

This will ensure that Sitecore Rocks loads the assembly when starting up.

## Visual Studio Experimental Instance
Visual Studio can be run in an [experimental instance mode](http://msdn.microsoft.com/en-us/library/bb166560.aspx), so that any configuration changes do not interfere with the normal configuration.

To use experimental mode, add the string:

```/rootsuffix Exp```

to the Command line arguments field.

Rocks supports experimental mode by using the alternative path:

```C:\Users\<You>\AppData\Local\Sitecore\Sitecore.RocksExp\Plugins\<MyPlugin> ```

## Loading Sitecore Rocks plugins
Sitecore Rocks plugins can also be loaded from the Visual Studio command line by specifying: 

```/rocksplugin <folder>```

The "folder" parameter points to a folder where the plugin assemblies are located. 