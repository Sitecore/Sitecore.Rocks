// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using EnvDTE80;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.Extending
{
    [Command, StartPageCommand("Create a Visual Studio project for a new Sitecore Rocks server component", StartPageSitecoreRocksExtendingGroup.Name, 2905)]
    public class CreateServerPluginVisualStudioProject : StartPageCommandBase
    {
        public override void Execute(StartPageContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var dialog = new SaveFileDialog
            {
                Title = "Create Visual Studio Project",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = @"Sitecore.Rocks.Server.Plugin",
                DefaultExt = @".csproj",
                Filter = @"Visual Studio Projects (.csproj)|*.csproj|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (!dialog.FileName.StartsWith("Sitecore.Rocks.Server."))
            {
                AppHost.MessageBox("Sitecore Rocks server components must start with 'Sitecore.Rocks.Server.' for security reasons.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var destination = Path.GetDirectoryName(dialog.FileName);
            var projectName = Path.GetFileNameWithoutExtension(dialog.FileName);

            try
            {
                var solution = (Solution2)SitecorePackage.Instance.Dte.Solution;

                var templatePath = solution.GetProjectTemplate("Sitecore Rocks Server Plugin.zip", "CSharp");
                solution.AddFromTemplate(templatePath, destination, projectName);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Failed to create the Visual Studio project:\n\n" + ex.Message + "\n\nPlease try again.");
            }
        }
    }
}
