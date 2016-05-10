// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Packages.InstallPackages;

namespace Sitecore.Rocks.ContentTrees.Commands.Packages
{
    [Command, CommandId(CommandIds.SitecoreExplorer.CreatePackage, typeof(ContentTreeContext)), Feature(FeatureNames.Packages)]
    public class InstallPackage : CommandBase
    {
        public InstallPackage()
        {
            Text = "Install Package...";
            Group = "Exporting";
            SortingValue = 500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IFileSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Files.Count() != 1)
            {
                return false;
            }

            var file = context.Files.FirstOrDefault();
            if (file == null)
            {
                return false;
            }

            if (string.Compare(Path.GetExtension(file.FileUri.FileName), @".zip", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IFileSelectionContext;
            if (context == null)
            {
                return;
            }

            var file = context.Files.FirstOrDefault();
            if (file == null)
            {
                return;
            }

            var d = new InstallPackageDialog();
            d.Initialize(file.FileUri.Site, file.FileUri.ToServerPath());

            AppHost.Shell.ShowDialog(d);
        }
    }
}
