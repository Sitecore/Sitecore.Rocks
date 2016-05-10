// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Tools
{
    [Command(Submenu = "Tools"), CommandId(CommandIds.SitecoreExplorer.ValidationIssues, typeof(ContentTreeContext)), Feature(FeatureNames.Validation)]
    public class ValidationIssues : CommandBase
    {
        public ValidationIssues()
        {
            Text = Resources.ValidationIssues_ValidationIssues_Validation_Issues;
            Group = "Tools";
            SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.First();

            return item.ItemUri.Site.DataService.CanExecuteAsync("Validation.GetValidationIssues");
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            if (context.Items.Count() != 1)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            AppHost.Windows.OpenValidationIssues(item.ItemUri);
        }
    }
}
