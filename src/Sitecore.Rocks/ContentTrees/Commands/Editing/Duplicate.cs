// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command, CommandId(CommandIds.SitecoreExplorer.Duplicate, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Editing, Priority = 0x0110, Icon = "Resources/16x16/duplicate.png")]
    public class Duplicate : CommandBase
    {
        public Duplicate()
        {
            Text = Resources.Duplicate_Duplicate_Duplicate;
            Group = "Edit";
            SortingValue = 2000;
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

            return context.SelectedItems.FirstOrDefault() as ICanDuplicate != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ICanDuplicate;
            if (item == null)
            {
                return;
            }

            item.Duplicate();
        }
    }
}
