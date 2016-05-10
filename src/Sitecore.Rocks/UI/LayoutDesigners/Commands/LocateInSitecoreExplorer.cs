// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Input;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands
{
    [Command, ToolbarElement(typeof(LayoutDesignerContext), 4400, "Rendering", "Navigate", Icon = "Resources/32x32/Nudge-Left.png", Text = "Rendering")]
    public class LocateInSitecoreExplorer : CommandBase, IToolbarElement
    {
        public LocateInSitecoreExplorer()
        {
            Text = Resources.LocateInContentTree_LocateInContentTree_Locate_in_Sitecore_Explorer;
            Group = "Navigate";
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/synchronize.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return false;
            }

            return ActiveContext.ActiveContentTree != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    AppHost.OpenContentEditor(item.ItemUri);
                }
                else
                {
                    contentTree.Locate(item.ItemUri);
                }
            }
        }
    }
}
