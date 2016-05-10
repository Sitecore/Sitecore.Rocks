// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Macros.Commands
{
    /* [Command(Submenu = "Tools", ExcludeFromSearch = true)] */

    public class MacrosSubmenu : CommandBase
    {
        public MacrosSubmenu()
        {
            Text = Resources.MacrosSubmenu_MacrosSubmenu_Macros;
            Group = "Tools";
            SortingValue = 9300;
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            return CommandManager.HasCommands(parameter, "Macros") || MacroManager.HasMacros(parameter);
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var result = new List<ICommand>();

            var macros = MacroManager.GetMacros(parameter);

            var sortingValue = 1000;

            foreach (var macro in macros)
            {
                var q = new MacroCommand
                {
                    Macro = macro,
                    Text = macro.Text,
                    Group = "Macros",
                    SortingValue = sortingValue
                };

                result.Add(q);
                sortingValue++;
            }

            result.AddRange(CommandManager.GetCommands(parameter, "Macros"));

            return result;
        }
    }
}
