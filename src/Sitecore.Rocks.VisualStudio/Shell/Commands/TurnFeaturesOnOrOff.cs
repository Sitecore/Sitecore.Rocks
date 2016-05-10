// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Features;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.TurnFeaturesOnOrOff)]
    public class TurnFeaturesOnOrOff : CommandBase
    {
        public TurnFeaturesOnOrOff()
        {
            Text = "Turn Features On or Off...";
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            var dialog = new FeaturesDialog();
            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
