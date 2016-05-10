// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;
using Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Plugins;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Extending
{
    [Command, StartPageCommand("Add a new Sitecore Rocks plugin project to the current solution", StartPageSitecoreRocksPluginsGroup.Name, 2000)]
    public class CreateProject : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.MessageBox("In the Add New Project dialog, choose the Visual C# / Sitecore Rocks folder and pick a project template.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                SitecorePackage.Instance.Dte.ExecuteCommand("File.AddNewProject");
            }
            catch (COMException)
            {
                AppHost.MessageBox("Oh, come on, Visual Studio, why won't you execute the 'File.AddNewProject' command?\n\nSorry about this, maybe you could use File | Add | New Project... instead.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
