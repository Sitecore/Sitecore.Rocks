// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.IItemSelectionContextExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Packages.PackageBuilders;

namespace Sitecore.Rocks.ContentTrees.Commands.Packages
{
    [Command(Submenu = "Tools"), CommandId(CommandIds.SitecoreExplorer.CreatePackage, typeof(ContentTreeContext)), Feature(FeatureNames.Packages)]
    public class CreatePackage : CommandBase
    {
        public CreatePackage()
        {
            Text = Resources.CreatePackage_CreatePackage_Create_Package___;
            Group = "Exporting";
            SortingValue = 5500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            if (!context.IsSameDatabase())
            {
                return false;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            return (item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            PackageBuilder.Load(context.Items);

            /*
      var databaseName = context.GetDatabaseName();
      if (databaseName == DatabaseName.Empty)
      {
        return;
      }

      var site = context.GetSite();
      if (site == Site.Empty)
      {
        return;
      }

      var dialog = new CreatePackageDialog();
      dialog.Initialize(context.Items);

      if (AppHost.Shell.ShowDialog(dialog) != true)
      {
        return;
      }

      var fileName = dialog.FileName.Text;
      var items = string.Join(@"|", dialog.SelectedItems.Select(i => i.ItemId.ToString()));

      ExecuteCompleted completed = delegate(string response, ExecuteResult result)
      {
        if (!DataService.HandleExecute(response, result))
        {
          return;
        }

        var client = new WebClient();
        client.DownloadFile(response, fileName);
      };

      site.DataService.ExecuteAsync("Packages.CreatePackage", completed, databaseName.ToString(), items);
      */
        }
    }
}
