# Commands 
A command is the most common extension point in Sitecore Rocks. The context menus are dynamically built from commands.

Commands are managed by the static ```CommandManager``` class.

All commands implement the ```Sitecore.Commands.ICommand``` interface which extends the WPF ```ICommand``` interface. 

Usually commands inherit from the ```CommandBase``` class which implements the ```ICommand``` interface.
All commands must be marked with the ```Command``` attribute.

## Execution
All commands work in a context (see also [Command Contexts](CommandContexts.md)). For example, when user right-clicks in the Sitecore Explorer, the context is the Sitecore Explorer (represented by the ```ContentTreeContext``` class).

When Sitecore Rocks builds a context menu, it creates a new context object and asks the ```CommandManager``` for all commands that can execute in this context.

The ```CommandManager``` iterates through all registered commands and calls the CanExecute method with the context as parameter. If the ```CanExecute``` method returns true, the commands is displayed in the context menu, otherwise not.

When the user selects an item from the context menu, the ```Execute``` method is called with the context as parameter.

## Appearance
The ```Command``` class contains several properties that control how the command appears in the context menu.

The most important properties are (these should be specified for all commands):
* ```Text``` - the name of the command.
* ```Group``` - the command group, e.g. the Paste commands belongs to the Clipboard group.
* ```SortingValue``` - all visible commands are sorted by this value before being displayed.

When Sitecore Rocks builds the context menu, it first sorts the visible commands by the sorting value. Then it iterates the commands and creates a menu item for each. If the Group property of the current command is different from the Group property of the previous command, a menu separator is inserted before the command.

### Tip
You can turn on debug information in the Visual Studio Options dialog. In the Sitecore | Content Editor section, set the Plugin Developers | Show Groups and Sorting Values to true. This will display the groups and sorting values of commands in the context menus which makes it easy to position a new command.

## Hello World Sample
To create a new command (the annoying HelloWorld command) create a new Class Library project in Visual Studio. Add a reference to the Sitecore.Rocks assembly and setup the project as described in the [Debugging a Visual Studio project](DebuggingVisualStudioProject.md) section.

Create a new class that derives from the ```CommandBase``` class.

Mark the class with the ```Command``` attribute:

```
[Command]
public class HelloWorld : CommandBase
```

Implement a constructor that assigns the ```Text```, ```Group``` and ```SortingValue``` properties:

```
public HelloWorld()
{
  this.Text = "Hello World";
  this.Group = "Sample";
  this.SortingValue = 1;
}
```

Implement the ```CanExecute``` method (returning true ensures that the command is only available in the Sitecore Explorer context):

```
public override bool CanExecute(object parameter)
{
  return parameter is ContentTreeContext;
}
```

Implement the ```Execute``` method:

```
public override void Execute(object parameter)
{
  MessageBox.Show("Hello World");
}
```

Now start debugging the project (press F5) - this should start a second instance of Visual Studio. Open the Sitecore Explorer and right-click on a site node.

At the top of the context menu, you should see the "Hello World" menu item.