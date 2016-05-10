// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Items
{
    [Command(Submenu = "Tools", ExcludeFromSearch = true)]
    public class BrowseItemSubmenu : CommandBase
    {
        public const string Name = "Browse Item";

        public BrowseItemSubmenu()
        {
            Text = Resources.BrowseSubmenu_BrowseSubmenu_Browse;
            Group = "Tools";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            return CommandManager.HasCommands(parameter, Name);
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            return CommandManager.GetCommands(parameter, Name);
        }
    }
}
