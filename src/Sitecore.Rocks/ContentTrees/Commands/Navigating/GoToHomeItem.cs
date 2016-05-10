// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command, Feature(FeatureNames.AdvancedNavigation)]
    public class GoToHomeItem : CommandBase
    {
        public GoToHomeItem()
        {
            Text = "Go to Home Item";
            Group = "Navigate";
            SortingValue = 5300;
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

            var site = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (site == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return;
            }

            var site = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (site == null)
            {
                return;
            }

            var itemUri = new ItemUri(new DatabaseUri(site.Site, new DatabaseName("master")), new ItemId(new Guid("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}")));

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(itemUri);
            }
        }
    }
}
