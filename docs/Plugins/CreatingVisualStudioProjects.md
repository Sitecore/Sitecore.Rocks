# Creating Visual Studio Projects

## Plugin (client-side)
* Create a standard Class Library project in Visual Studio.
* Add a reference to the Sitecore.Rocks assembly and optionally a reference to the Sitecore.VisualStudio assembly, if you want to integrate more deeply with Visual Studio. You can also install the Sitecore.Rocks.Client or Sitecore Rocks Server NuGet packages. 
* Set the "Copy Local" setting to false for each of these assemblies.
* Set the Start Action in the Debug tab to External Program and point the path to something like (Windows 7, 64-bit): ```C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe ```
* Set the Output path of the assembly in the Build tab to something like:
```C:\Users\<You>\AppData\Local\Sitecore\Sitecore.Rocks\Plugins\ ```

## Server Component (server-side)
* Create another standard Class Library project in Visual Studio.
* By convention, the server component assembly name must start with "Sitecore.Rocks.Server.", for instance "Sitecore.Rocks.Server.MyPlugin.dll"
* Add references to the Sitecore.Rocks.Server, Sitecore.Rocks.Server.Core and Sitecore.Kernel assemblies.
* Set the "Copy Local" setting to false for each of these assemblies.
* Set the Output path of the assembly in the Build tab to the /bin folder of your website.

## Calling Server Component code
To call server-side code, use the ```ExecuteAsync``` method on the Hard Rock Data Service.

This is a typical example:

```
item.ItemUri.Site.DataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.MyPlugin.DoWork,Sitecore.Rocks.Server.MyPlugin", completed, item.ItemUri.DatabaseName.Name, itemList.ToString());
```

The first parameter is the type name of the class to call. Please notice that the type must be located under the ```Sitecore.Rocks.Server.Requests``` namespace for security reasons. If the type name does not start with ```Sitecore.Rocks.Server.Requests```, it is prepended automatically.

The second completed parameter is the callback method to call when the request completes. Please notice that ```ExecuteAsync``` is asynchroneous, so that the UI thread is not blocked. The callback should always call ```DataService.HandleExecute``` to handle any errors. If ```HandleExecute``` returns false, an error occured.

```
ExecuteCompleted completed = delegate(string response, ExecuteResult result)
{
  if (!DataService.HandleExecute(response, result))
  {
    return;
  }
  // Do work
};
```

The rest of the parameters are optional and passed to the server method. By convention, they should all be strings.

## Server Component Code
The server-side code is always a class under the ```Sitecore.Rocks.Server.Requests``` with a single method named ```Execute```. The Execute method always returns a string. It can take any number of string parameters.

```
namespace Sitecore.Rocks.Server.Requests.MyPlugin
{
  using System;

  public class DoWork
  {
    public string Execute(string databaseName, string itemList)
    {
      return string.Empty;
    }
  }
}
```