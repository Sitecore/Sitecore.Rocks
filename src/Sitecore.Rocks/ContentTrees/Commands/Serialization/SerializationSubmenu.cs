// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    [Command(Submenu = ToolsSubmenu.Name, ExcludeFromSearch = true)]
    public class SerializationSubmenu : CommandBase
    {
        public const string Name = "Serialization";

        public SerializationSubmenu()
        {
            Text = Resources.SerializationSubmenu_SerializationSubmenu_Serialization;
            Group = "Tools";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is IItemSelectionContext))
            {
                return false;
            }

            return CommandManager.HasCommands(parameter, "Serialization");
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

            return CommandManager.GetCommands(parameter, "Serialization");
        }
    }
}
