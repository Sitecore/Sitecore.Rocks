// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.CutToClipboard, typeof(ContentTreeContext))]
    public class CutToClipboard : CommandBase
    {
        public CutToClipboard()
        {
            Text = Resources.CutToClipboard_CutToClipboard_Cut;
            Group = "Edit";
            SortingValue = 3100;
            Icon = new Icon("Resources/16x16/cut.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            var selection = context.Items.ToList();
            if (!selection.Any())
            {
                return false;
            }

            foreach (var item in selection)
            {
                var treeViewItem = item as ItemTreeViewItem;
                if (treeViewItem == null)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var sb = new StringBuilder(@"Sitecore.Clipboard.Cut:");
            var first = true;

            foreach (var item in selection.Items)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(';');
                }

                sb.Append(item.ItemUri.Site.Name);
                sb.Append('/');
                sb.Append(item.ItemUri.DatabaseName);
                sb.Append('/');
                sb.Append(item.ItemUri.ItemId);
                sb.Append('/');
                sb.Append(item.Name.Replace(@"/", @"&slash"));
                sb.Append('/');
                sb.Append(item.Icon.IconPath.Replace(@"/", @"&slash"));
            }

            AppHost.Clipboard.SetText(sb.ToString());
        }
    }
}
