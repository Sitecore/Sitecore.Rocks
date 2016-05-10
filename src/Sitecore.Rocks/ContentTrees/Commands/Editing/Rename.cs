// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command, CommandId(CommandIds.SitecoreExplorer.Rename, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Editing, Icon = "Resources/16x16/item_rename.png", Priority = 0x0130)]
    public class Rename : CommandBase
    {
        public Rename()
        {
            Text = Resources.Rename_Rename_Rename;
            Group = "Edit";
            SortingValue = 4000;
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

            var item = context.SelectedItems.FirstOrDefault() as ICanRename;
            if (item == null)
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

            var item = context.SelectedItems.FirstOrDefault() as ICanRename;
            if (item == null)
            {
                return;
            }

            item.Rename();
        }
    }
}
