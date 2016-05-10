// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers.Commands
{
    [Command]
    public class LocateInContentTree : CommandBase
    {
        public LocateInContentTree()
        {
            Text = Resources.LocateInContentTree_LocateInContentTree_Locate_in_Sitecore_Explorer;
            Group = "Navigate";
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/synchronize.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LanguageViewerContext;
            if (context == null)
            {
                return false;
            }

            if (context.LanguageViewer.LanguageList.SelectedItems.Count != 1)
            {
                return false;
            }

            if (ActiveContext.ActiveContentTree == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LanguageViewerContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.LanguageViewer.LanguageList.SelectedItem as LanguageViewer.LanguageDescriptor;
            if (selectedItem == null)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(selectedItem.ItemUri);
            }
        }
    }
}
