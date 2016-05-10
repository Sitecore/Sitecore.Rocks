// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.CopyToClipboard, typeof(ContentTreeContext))]
    public class CopyToClipboard : CommandBase
    {
        public CopyToClipboard()
        {
            Text = Resources.CopyToClipboard_CopyToClipboard_Copy;
            Group = "Edit";
            SortingValue = 3000;
            Icon = new Icon("Resources/16x16/copy.png");
        }

        public override bool CanExecute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            return selection.Items.Any();
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var sb = new StringBuilder(@"Sitecore.Clipboard.Copy:");
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
