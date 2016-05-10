// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Templates
{
    [Command]
    public class DesignTemplate : CommandBase
    {
        public DesignTemplate()
        {
            Text = Resources.DesignTemplate_DesignTemplate_Design_Template;
            Group = "Templates";
            SortingValue = 100;
            Icon = new Icon("Resources/16x16/designtemplate.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (!item.IsTemplate)
            {
                return false;
            }

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditTemplate) != DataServiceFeatureCapabilities.EditTemplate)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = (ContentTreeContext)parameter;
            if (context == null)
            {
                return;
            }

            var item = (ItemTreeViewItem)context.SelectedItems.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenTemplateDesigner(item.ItemUri);
        }
    }
}
