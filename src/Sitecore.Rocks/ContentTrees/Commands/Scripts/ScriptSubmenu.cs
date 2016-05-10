// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Scripts
{
    [Command(Submenu = "Tools", ExcludeFromSearch = true)]
    public class ScriptSubmenu : CommandBase
    {
        public ScriptSubmenu()
        {
            Text = Resources.ScriptSubmenu_ScriptSubmenu_Scripts;
            Group = "Extensions";
            SortingValue = 8000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!CommandManager.HasCommands(parameter, "Script"))
            {
                return false;
            }

            return context.Items.Any();
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            return CommandManager.GetCommands(parameter, "Script");
        }
    }
}
