// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
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
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return false;
            }

            if (context.DesignSurface.SelectedItems.Count() != 1)
            {
                return false;
            }

            return ActiveContext.ActiveContentTree != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return;
            }

            if (context.DesignSurface.SelectedItems.Count() != 1)
            {
                return;
            }

            var selectedItem = context.DesignSurface.SelectedItems.FirstOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            var item = selectedItem.ShapeContent as IItemUri;
            if (item == null)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(item.ItemUri);
            }
        }
    }
}
