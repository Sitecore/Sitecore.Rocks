// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(ExcludeFromSearch = true)]
    public class BrowseSiteSubmenu : CommandBase
    {
        public const string Name = "Browse Site";

        public BrowseSiteSubmenu()
        {
            Text = Resources.BrowseSubmenu_BrowseSubmenu_Browse;
            Group = "Browse";
            SortingValue = 9000;
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

            var item = context.SelectedItems.First();

            if (!(item is SiteTreeViewItem))
            {
                return false;
            }

            return CommandManager.HasCommands(parameter, Name);
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands([CanBeNull] object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            return new List<ICommand>(CommandManager.GetCommands(parameter, Name));
        }
    }
}
