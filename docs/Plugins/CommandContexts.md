# Command Contexts
In Sitecore Rocks a command is an executable action.

All commands implement the WPF ```ICommand``` interface and usually inherits from ```Sitecore.VisualStudio.Commands.CommandBase```.

The ```ICommand``` interface defines two methods:

```
bool CanExecute(object parameter);
void Execute(object parameter);
```

In Sitecore Rocks the parameter is a context object that defines in which context the command can execute.

Usually a ```CanExecute``` method looks like this:

```
public override bool CanExecute(object parameter)
{
  var context = parameter as ContentTreeContext;
  if (context == null)
  {
    return false;
  }

  // Additional checks...

  return true;
}
```

In the above sample, the command is only valid in the ```ContentTreeContext``` (which is the Sitecore Explorer, just to confuse you).

The context object usually specifies additional parameters, such as selected items.

To implement a new command, it is important to know which context the command can execute in - or in other words the type of the context object.

All Sitecore Rocks context objects implement the ```ICommandContext``` interface, which is just a marker interface (no properties or methods defined).

Furthermore many command contexts implement the ```IItemSelectionContext``` which defines a list of selected items. Commands that work on items usually use the ```IItemSelectionContext``` context object. That way the command is available in the Sitecore Explorer, Item Editor, Search and other panes.

Below is a list of context objects (0.7.4):

ICommandContext
* ArchiveContext (in Sitecore.VisualStudio.UI.Archives)
* ContentEditorContext (in Sitecore.VisualStudio.ContentEditors)
* ContentEditorFieldContext (in Sitecore.VisualStudio.ContentEditors)
* ContentEditorSectionContext (in Sitecore.VisualStudio.ContentEditors)
* ContentTreeContext (in Sitecore.VisualStudio.ContentTrees)
* ContentTreeSecurityContext (in Sitecore.VisualStudio.ContentTrees)
* DebugSessionListViewerContext (in Sitecore.VisualStudio.UI.DebugTraces)
* GutterContext (in Sitecore.VisualStudio.ContentTrees.Gutters)
* JobViewerContext (in Sitecore.VisualStudio.UI.JobViewer)
* LayoutDesignerContext (in Sitecore.VisualStudio.UI.LayoutDesigner)
* LinksContext (in Sitecore.VisualStudio.UI.Links)
* LogViewerContext (in Sitecore.VisualStudio.UI.LogViewer)
* MediaContext (in Sitecore.VisualStudio.Media)
* MultiSessionContext (in Sitecore.VisualStudio.UI.DebugTraces.MultiSession)
* PanelContext (in Sitecore.VisualStudio.ContentEditors.Panels)
* PublishingQueueContext (in Sitecore.VisualStudio.UI.Publishing)
* QueryAnalyzerContext (in Sitecore.VisualStudio.UI.QueryAnalyzers)
* RuleDesignerContext (in Sitecore.VisualStudio.UI.Rules)
* SanityCheckContext (in Sitecore.VisualStudio.UI.Management.ManagementItems.SanityChecking)
* SearchContext (in Sitecore.VisualStudio.Searching)
* SessionContext (in Sitecore.VisualStudio.UI.DebugTraces.SingleSession)
* ShellContext (in Sitecore.VisualStudio.Shell)
* SiteManagementContext (in Sitecore.VisualStudio.UI.Management)
* TemplateDesignerContext (in Sitecore.VisualStudio.UI.TemplateDesigner)
* TemplateFieldSorterContext (in Sitecore.VisualStudio.UI.TemplateFieldSorter)
* ValidationIssuesContext (in Sitecore.VisualStudio.UI.ValidationIssues)
* ValidatorContext (in Sitecore.VisualStudio.ContentEditors.Validators)
