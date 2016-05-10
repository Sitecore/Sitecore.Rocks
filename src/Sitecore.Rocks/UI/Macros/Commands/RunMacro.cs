// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Macros.Dialogs;

namespace Sitecore.Rocks.UI.Macros.Commands
{
    [Command(Submenu = "Tools")]
    public class RunMacro : CommandBase
    {
        public RunMacro()
        {
            Text = "Run Macro...";
            Group = "Tools";
            SortingValue = 9295;
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            return MacroManager.HasMacros(parameter);
        }

        public override void Execute(object parameter)
        {
            var d = new RunMacroDialog();
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var macro = d.Macro;
            if (macro == null)
            {
                return;
            }

            macro.Run(parameter);
        }
    }
}
