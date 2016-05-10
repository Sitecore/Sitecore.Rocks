// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command(Submenu = SpeakSubmenu.Name)]
    public class UpdatePlaceholders : CommandBase
    {
        public UpdatePlaceholders()
        {
            Text = "Update Place Holders Field";
            Group = "Placeholders";
            SortingValue = 7000;
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

            var item = context.Items.FirstOrDefault() as ITemplatedItem;
            if (item == null)
            {
                return false;
            }

            if (!item.ItemUri.Site.IsSitecoreVersion(SitecoreVersion.Version70))
            {
                return false;
            }

            if (item.TemplateId != CreateParametersTemplate.ViewRenderingId)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var placeHolders = AppHost.Prompt("Placeholders", "Placeholders", response);
                if (placeHolders == null)
                {
                    return;
                }

                AppHost.Server.UpdateItem(item.ItemUri, "Place Holders", placeHolders);
            };

            AppHost.Server.XmlLayouts.AnalyzePlaceholdersInView(item.ItemUri, completed);
        }
    }
}
