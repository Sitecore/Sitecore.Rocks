// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.Commandy.Commands
{
    [Command, Feature(FeatureNames.Commandy)]
    public class OpenCommandy : CommandBase
    {
        public OpenCommandy()
        {
            Text = "Commandy";
            Group = "Commandy";
            SortingValue = 9998;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter != null && !(parameter is IToolBarContext);
        }

        public override void Execute(object parameter)
        {
            if (parameter != null)
            {
                Commandy.Open(parameter);
            }
        }
    }
}
