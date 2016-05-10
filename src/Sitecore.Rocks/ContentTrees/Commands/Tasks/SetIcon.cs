// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.SetIcon, typeof(ContentTreeContext))]
    public class SetIcon : CommandBase
    {
        public SetIcon()
        {
            Text = Resources.SetIcon_SetIcon_Set_Icon;
            Group = "Fields";
            SortingValue = 3000;
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

            if (!context.Items.All(i => i.ItemUri.Site.DataService.CanExecuteAsync("Items.Save")))
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

            var firstItem = context.Items.FirstOrDefault();
            if (firstItem == null)
            {
                return;
            }

            var iconPath = firstItem.Icon.IconPath;

            foreach (var item in context.Items)
            {
                if (item.Icon.IconPath != iconPath)
                {
                    iconPath = string.Empty;
                }
            }

            var n = iconPath.IndexOf("~/icon/", StringComparison.InvariantCultureIgnoreCase);
            if (n >= 0)
            {
                iconPath = iconPath.Mid(n + 7);
            }

            n = iconPath.IndexOf("/IconCache/", StringComparison.InvariantCultureIgnoreCase);
            if (n >= 0)
            {
                iconPath = iconPath.Mid(n + 11);
            }

            var d = new SetIconDialog();
            d.Initialize(firstItem.ItemUri.Site, iconPath);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var value = d.FileName.Replace("32x32", "16x16");

            foreach (var item in context.Items)
            {
                ItemModifier.Edit(item.ItemUri, FieldIds.Icon, value);
            }
        }
    }
}
